using Microsoft.Extensions.Options;
using MongoDB.Driver;
using QRBee.Api;
using QRBee.Api.Services;
using QRBee.Api.Services.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddSingleton<IQRBeeAPI,QRBeeAPI>()
    .AddSingleton<IStorage, Storage>()
    .Configure<DatabaseSettings>(builder.Configuration.GetSection("QRBeeDatabase"))
    .AddSingleton<IMongoClient>( cfg => new MongoClient(cfg.GetRequiredService<IOptions<DatabaseSettings>>().Value.ToMongoDbSettings()))
    ;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
