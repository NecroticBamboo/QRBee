using log4net;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using QRBee.Api;
using QRBee.Api.Services;
using QRBee.Api.Services.Database;
using QRBee.Core.Security;

var builder = WebApplication.CreateBuilder(args);


builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    GlobalContext.Properties["LOGS_ROOT"] = Environment.GetEnvironmentVariable("LOGS_ROOT") ?? "";
    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    logging.AddLog4Net("log4net.config");
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddSingleton<IQRBeeAPI,QRBeeAPIService>()
    .AddSingleton<IStorage, Storage>()
    .Configure<DatabaseSettings>(builder.Configuration.GetSection("QRBeeDatabase"))
    .AddSingleton<IMongoClient>( cfg => new MongoClient(cfg.GetRequiredService<IOptions<DatabaseSettings>>().Value.ToMongoDbSettings()))
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
