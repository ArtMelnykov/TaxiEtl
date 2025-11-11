using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using TaxiDataETL.Core;
using TaxiDataETL.Core.Interfaces;
using TaxiDataETL.Data;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// DI
builder.Services.AddCoreServices();
builder.Services.AddDataServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var etl = scope.ServiceProvider.GetRequiredService<ITaxiEtlService>();
    await etl.RunAsync();
}

Log.Information("ETL finished");
