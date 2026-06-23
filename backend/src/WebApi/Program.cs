using Infrastructure.Extensions;
using WebApi.Extentions;

var builder = WebApplication.CreateBuilder(args);

builder
    .ConfigureApplicationServices()
    .ConfigureDatabase("NeonPostgresql")
    .ConfigureIdentity()
    .AddDI();

var app = builder.Build();

// Migrate and seed the database on application startup
await app.MigrateAndSeedDatabaseAsync();

app.ConfigurePipeline();

app.Run();
