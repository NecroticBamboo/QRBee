﻿using log4net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QRBee.Api.Services;
using QRBee.Droid.Services;
using QRBee.Load.Generator;
using Microsoft.Extensions.Configuration;
using System.Net;
using Microsoft.Extensions.Options;

Console.WriteLine("=== QRBee artificaial load generator ===");

var builder = Host.CreateDefaultBuilder();

builder.ConfigureServices((context, services) =>
{
    services.AddLogging(logging =>
    {
        logging.ClearProviders();
        GlobalContext.Properties["LOGS_ROOT"] = Environment.GetEnvironmentVariable("LOGS_ROOT") ?? "";
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        logging.AddLog4Net("log4net.config");
    });

    services
        .AddHttpClient<QRBee.Core.Client.Client, QRBee.Core.Client.Client>((httpClient,x) => 
        {
            var settings = x.GetRequiredService<IOptions<GeneratorSettings>>();
            var url = settings.Value.QRBeeURL;
            return new QRBee.Core.Client.Client(url, httpClient); 
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator });
        ;
    services
        .Configure<GeneratorSettings>(context.Configuration.GetSection("GeneratorSettings"))
        .AddSingleton<ClientPool>()
        .AddSingleton<PaymentRequestGenerator>()
        .AddSingleton<TransactionDefiler>()
        .AddSingleton<UnconfirmedTransactions>()
        .AddSingleton<LoadSpike>()
        .AddSingleton<LargeAmount>()
        .AddSingleton<IAnomalyReporter, AnomalyReporter>()
        .AddSingleton<PrivateKeyHandlerFactory>(x => no => new PrivateKeyHandler(x.GetRequiredService<ILogger<ServerPrivateKeyHandler>>(), x.GetRequiredService<IConfiguration>(), no))
        .AddSingleton<SecurityServiceFactory>(x => no => new AndroidSecurityService(x.GetRequiredService<PrivateKeyHandlerFactory>()(no)))
        .AddHostedService<LoadGenerator>()
        ;
});

// ServicePointManager.DefaultConnectionLimit = 500;
ServicePointManager.ReusePort = true;
ServicePointManager.CheckCertificateRevocationList = false;


var host = builder.Build();
host.Run();
