// See https://aka.ms/new-console-template for more information
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QRBee.Api.Services;
using QRBee.Droid.Services;
using QRBee.Load.Generator;
using Microsoft.Extensions.Configuration;

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
        .AddHttpClient<QRBee.Core.Client.Client, QRBee.Core.Client.Client>(httpClient => new QRBee.Core.Client.Client("https://localhost:7000/", httpClient))
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator });
        ;
    services
        .Configure<GeneratorSettings>(context.Configuration.GetSection("GeneratorSettings"))
        .AddSingleton<ClientPool>()
        .AddSingleton<PaymentRequestGenerator>()
        .AddSingleton<PrivateKeyHandlerFactory>(x => no => new PrivateKeyHandler(x.GetRequiredService<ILogger<ServerPrivateKeyHandler>>(), x.GetRequiredService<IConfiguration>(), no))
        .AddSingleton<SecurityServiceFactory>(x => no => new AndroidSecurityService(x.GetRequiredService<PrivateKeyHandlerFactory>()(no)))
        .AddHostedService<LoadGenerator>()
        ;
});



var host = builder.Build();
host.Run();
