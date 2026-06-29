using Infrastructure.Extensions;
using WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder
    .ConfigureApplicationServices()
    .ConfigureDatabase("NeonPostgresql")
    .ConfigureIdentity()
    .ConfigureAuth()
    .AddDI();

var app = builder.Build();

// Migrate and seed the database on application startup
await app.MigrateAndSeedDatabaseAsync();

app.ConfigurePipeline();

app.Run();
