using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using OficinaApi.Data;
using OficinaApi.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração do MongoDB (lê de variável de ambiente MONGODB_URI ou do appsettings)
var mongoConnectionString = Environment.GetEnvironmentVariable("MONGODB_URI")
    ?? builder.Configuration["MongoDb:ConnectionString"]!;
var mongoDatabaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE")
    ?? builder.Configuration["MongoDb:DatabaseName"]
    ?? "OficinaDb";

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName));

// 2. Registro das Camadas (Injeção de Dependência)
builder.Services.AddScoped<IVeiculoRepository, VeiculoRepository>();
builder.Services.AddScoped<VeiculoService>();
builder.Services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
builder.Services.AddScoped<OrdemServicoService>();

builder.Services.AddControllers();

// 3. Configuração do Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "OficinaAPI — Gestão de Oficina Mecânica",
        Description = "API REST para controle de veículos e ordens de serviço."
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "OficinaAPI v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();