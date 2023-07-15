// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QRBee.Core.Client;
using QRBee.Core.Data;
using QRBee.Load.Generator;

internal class LoadGenerator : IHostedService
{
    private readonly Client _client;
    private readonly ClientPool _clientPool;
    private readonly PaymentRequestGenerator _paymentRequestGenerator;
    private readonly TransactionDefiler _transactionDefiler;
    private readonly ILogger<LoadGenerator> _logger;
    private readonly IOptions<GeneratorSettings> _settings;

    private TimeSpan _spikeDuration;
    private TimeSpan _spikeDelay;
    private double _spikeProbability;

    public LoadGenerator( 
        QRBee.Core.Client.Client client, 
        ClientPool clientPool, 
        PaymentRequestGenerator paymentRequestGenerator,
        TransactionDefiler transactionDefiler,
        ILogger<LoadGenerator> logger,
        IOptions<GeneratorSettings> settings
        ) 
    {
        _client                  = client;
        _clientPool              = clientPool;
        _paymentRequestGenerator = paymentRequestGenerator;
        _transactionDefiler      = transactionDefiler;
        _logger                  = logger;
        _settings                = settings;

        var loadSpike     = _settings.Value.LoadSpike;
        _spikeDuration    = TimeSpan.Zero;
        _spikeDelay       = TimeSpan.Zero;
        _spikeProbability = loadSpike?.Probability ?? 0.0;

        if (loadSpike != null && loadSpike.Probability > 0.0)
        {
            if (!loadSpike.Parameters.TryGetValue("Duration", out var duration)
                || !TimeSpan.TryParse(duration, out _spikeDuration))
            {
                _spikeProbability = 0.0;
            }
            else
            {
                if (!loadSpike.Parameters.TryGetValue("Delay", out duration)
                    || !TimeSpan.TryParse(duration, out _spikeDelay))
                {
                    _spikeDelay = TimeSpan.FromMilliseconds(10);
                }
            }
        }
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

        while (true)
        {
            if (DateTime.Now > nextReport)
            {
                _logger.LogInformation($"S: {_paymentsGenerated,10:N0} R: {_paymentsProcessed,10:N0} C: {_paymentsConfirmed,10:N0} F: {_paymentsFailed,10:N0}");
                nextReport = DateTime.Now + TimeSpan.FromSeconds(1);
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
                    newQueue = Interlocked.Exchange(ref _responseQueue, newQueue);

                if (newQueue.Count == 0)
                {
                    await Task.Delay(_rng.NextInRange(300, 600));
                    continue;
                }

                var tasks = newQueue.ToList();

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
                        else
                            Interlocked.Increment(ref _paymentsFailed);
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

        var spikeEnd         = DateTime.MinValue;

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

                _responseQueue.Add(resp);
                Interlocked.Increment(ref _paymentsGenerated);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _paymentsFailed);
                _logger.LogError(ex, "Generation thread");
            }

            if (DateTime.Now > spikeEnd)
            {
                var dice = _rng.NextDouble();
                if (dice < _spikeProbability)
                {
                    // start load spike
                    spikeEnd = DateTime.Now + _spikeDuration;
                    _logger.LogWarning($"Anomaly: Load spike until {spikeEnd} Dice={dice}");
                    await Task.Delay(_spikeDelay);
                }
                else
                {
                    await Task.Delay(_rng.NextInRange(
                        _settings.Value.DelayBetweenMessagesMSec,
                        _settings.Value.DelayBetweenMessagesMSec + _settings.Value.DelayJitterMSec
                        ));
                }
            }
            else
            {
                await Task.Delay(_spikeDelay);
            }
        }
    }
}