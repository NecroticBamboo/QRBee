// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QRBee.Core.Client;
using QRBee.Core.Data;
using QRBee.Load.Generator;

internal class LoadGenerator : IHostedService
{
    private readonly Client                      _client;
    private readonly ClientPool                  _clientPool;
    private readonly PaymentRequestGenerator     _paymentRequestGenerator;
    private readonly TransactionDefiler          _transactionDefiler;
    private readonly UnconfirmedTransactions     _unconfirmedTransactions;
    private readonly LoadSpike                   _loadSpike;
    private readonly ILogger<LoadGenerator>      _logger;
    private readonly IOptions<GeneratorSettings> _settings;

    public LoadGenerator( 
        QRBee.Core.Client.Client client, 
        ClientPool clientPool, 
        PaymentRequestGenerator paymentRequestGenerator,
        TransactionDefiler transactionDefiler,
        UnconfirmedTransactions unconfirmedTransactions,
        LoadSpike loadSpike,
        ILogger<LoadGenerator> logger,
        IOptions<GeneratorSettings> settings
        ) 
    {
        _client                  = client;
        _clientPool              = clientPool;
        _paymentRequestGenerator = paymentRequestGenerator;
        _transactionDefiler      = transactionDefiler;
        _unconfirmedTransactions = unconfirmedTransactions;
        _loadSpike               = loadSpike;
        _logger                  = logger;
        _settings                = settings;

        _logger.LogInformation($"Connected to QRBee on {_client.BaseUrl}");
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitClients();
        _ = Task.Run(ReportingThread);
        _ = Task.Run(ConfirmationThread);
        _ = Task.Run(ReceivingThread);
        _ = Task.Run(GenerateLoad);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async Task InitClients()
    {
        _logger.LogInformation($"Initializing {_settings.Value.NumberOfClients} clients...");
        for ( var i = 1; i < _settings.Value.NumberOfClients+1; i++ )
        {
            await _clientPool.GetClient(i);
        }
        _logger.LogInformation($"Initializing {_settings.Value.NumberOfMerchants} merchants...");
        for (var i = 1; i < _settings.Value.NumberOfMerchants + 1; i++)
        {
            await _clientPool.GetMerchant(i);
        }
        _logger.LogInformation($"=== Initialization complete ===");
    }

    private async Task ReportingThread()
    {
        DateTime nextReport = DateTime.MinValue;

        var lastPaymentsGenerated = 0L;
        var lastPaymentsProcessed = 0L;
        var lastPaymentsConfirmed = 0L;
        var lastPaymentsFailed    = 0L;

        while (true)
        {
            if (DateTime.Now > nextReport)
            {
                _logger.LogInformation(
                    $"S: {_paymentsGenerated-lastPaymentsGenerated,10:N0} " +
                    $"R: {_paymentsProcessed-lastPaymentsProcessed,10:N0} " +
                    $"C: {_paymentsConfirmed-lastPaymentsConfirmed,10:N0} " +
                    $"F: {_paymentsFailed-lastPaymentsFailed,10:N0}"
                    );

                nextReport = DateTime.Now + TimeSpan.FromSeconds(1);

                lastPaymentsGenerated = _paymentsGenerated;
                lastPaymentsProcessed = _paymentsProcessed;
                lastPaymentsConfirmed = _paymentsConfirmed;
                lastPaymentsFailed    = _paymentsFailed;
            }
            await Task.Delay(1000);
        }
    }

    private async Task GenerateLoad()
    {
        _logger.LogInformation("Generator started");

        var threadTasks = Enumerable.Range(0, _settings.Value.NumberOfThreads)
            .Select(_ => GenerationThread())
            .ToArray();

        await Task.WhenAll(threadTasks);

        _logger.LogInformation("Generator finished");
    }

    private List<Task<PaymentResponse>> _responseQueue = new();
    private List<Task> _confirmationQueue              = new();
    private ThreadSafeRandom _rng                      = new();
    private object _lock                               = new();

    private long _paymentsGenerated;
    private long _paymentsProcessed;
    private long _paymentsConfirmed;
    private long _paymentsFailed;

    private async Task ConfirmationThread()
    {
        while (true)
        {
            try
            {
                var newQueue = new List<Task>();

                lock (_lock)
                    newQueue = Interlocked.Exchange(ref _confirmationQueue, newQueue);

                if (newQueue.Count == 0)
                {
                    await Task.Delay( _rng.NextInRange(300, 600));
                    continue;
                }

                var tasks = newQueue.ToList();

                while (tasks.Any())
                {
                    try
                    {
                        var t = await Task.WhenAny(tasks);
                        tasks.Remove(t);
                        
                        Interlocked.Increment(ref _paymentsConfirmed);
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref _paymentsFailed);
                        _logger.LogError(ex, "Confirmation thread");
                        tasks = tasks.Where(x => x != null).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Receining thread");
            }
        }
    }


    private async Task ReceivingThread()
    {
        while (true)
        {
            try
            {
                var newQueue = new List<Task<PaymentResponse>>();

                lock (_lock)
                {
                    (newQueue, _responseQueue) = (_responseQueue, newQueue);
                }

                if (newQueue.Count == 0)
                {
                    await Task.Delay(_rng.NextInRange(300, 600));
                    continue;
                }

                var tasks = newQueue.Where(x => x != null).ToList();

                while ( tasks.Any() )
                {
                    try
                    {
                        var t = await Task.WhenAny(tasks);
                        tasks.Remove(t);

                        var res = await t;

                        Interlocked.Increment(ref _paymentsProcessed);

                        if (res?.Success ?? false)
                        {
                            if (_unconfirmedTransactions.ShouldConfirm())
                            {
                                var paymentConfirmation = new PaymentConfirmation
                                {
                                    MerchantId            = res.PaymentRequest.ClientResponse.MerchantRequest.MerchantId,
                                    MerchantTransactionId = res.PaymentRequest.ClientResponse.MerchantRequest.MerchantTransactionId,
                                    GatewayTransactionId  = res.GatewayTransactionId
                                };

                                _transactionDefiler.CorruptPaymentConfirmation(paymentConfirmation);

                                var confirmationTask = _client.ConfirmPayAsync(paymentConfirmation);
                                _confirmationQueue.Add(confirmationTask);
                            }
                        }
                        else
                            Interlocked.Increment(ref _paymentsFailed);
                    }
                    catch (TaskCanceledException )
                    {
                        Interlocked.Increment(ref _paymentsFailed);
                        // no message - too noisy
                    }
                    catch (HttpRequestException httpEx)
                    {
                        if ( httpEx.InnerException is IOException )
                        {
                            Interlocked.Increment(ref _paymentsFailed);
                            // no message - too noisy
                        }
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref _paymentsFailed);
                        _logger.LogError(ex, "Receining thread (confirmation)");
                    }
                }
            }
            catch ( Exception ex )
            {
                _logger.LogError(ex, "Receining thread");
            }
        }
    }


    private async Task GenerationThread()
    {

        // initial delay
        await Task.Delay(500 + _rng.Next() % 124);

        while (true)
        {
            try
            {
                var req = await _paymentRequestGenerator.GeneratePaymentRequest(
                    _rng.NextInRange(1, _settings.Value.NumberOfClients + 1),
                    _rng.NextInRange(1, _settings.Value.NumberOfMerchants + 1)
                    );

                _transactionDefiler.CorruptPaymentRequest(req);

                var resp = _client.PayAsync(req);

                lock (_lock)
                    _responseQueue.Add(resp);
                Interlocked.Increment(ref _paymentsGenerated);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _paymentsFailed);
                _logger.LogError(ex, "Generation thread");
            }

            await _loadSpike.Delay();
        }
    }
}