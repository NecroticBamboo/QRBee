using log4net;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OpenTelemetry.Metrics;
using QRBee.Api;
using QRBee.Api.Services;
using QRBee.Api.Services.Database;
using QRBee.Core.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
GlobalContext.Properties["LOGS_ROOT"] = Environment.GetEnvironmentVariable("LOGS_ROOT") ?? "";
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
builder.Logging.AddLog4Net("log4net.config");

builder.Services.AddOpenTelemetry()
    .WithMetrics(options =>
    {
        options
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddPrometheusExporter()
            ;
    });

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .Configure<DatabaseSettings>(builder.Configuration.GetSection("QRBeeDatabase"))
    ;

builder.Services
    .AddSingleton<IQRBeeAPI,QRBeeAPIService>()
    .AddSingleton<IStorage, Storage>()
    .AddSingleton<IMongoClient>( cfg => 
    {
        var section = cfg.GetRequiredService<IOptions<DatabaseSettings>>().Value
            ?? throw new ApplicationException("Configuration for DatabaseSettings is not found");
        return new MongoClient(section.ToMongoDbSettings());
    })
    .AddSingleton<IPrivateKeyHandler, ServerPrivateKeyHandler>()
    .AddSingleton<ISecurityService, SecurityService>()
    .AddSingleton<IPaymentGateway, PaymentGateway>()
    .AddSingleton<TransactionMonitoring>()
    ;

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
