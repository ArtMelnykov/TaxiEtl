using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using TaxiDataETL.Core.Interfaces;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// DI
builder.Services.AddCoreServices();
// builder.Services.AddDataServices(builder.Configuration);

var app = builder.Build();

//
// === ТЕСТ: читаем одну запись из CSV ===
//
using (var scope = app.Services.CreateScope())
{
    var csvReader = scope.ServiceProvider.GetRequiredService<ICsvTripReader>();

    await foreach (var trip in csvReader.ReadTripsAsync())
    {
        Console.WriteLine(
            $"Pickup: {trip.PickupDateTime}, " +
            $"Dropoff: {trip.DropoffDateTime}, " +
            $"Dist: {trip.TripDistance}, " +
            $"Tip: {trip.TipAmount}, " +
            $"PU: {trip.PULocationID}, DO: {trip.DOLocationID}");

        break;
    }
}

// НА ЭТОМ ЭТАПЕ ETL МОЖНО ВРЕМЕННО НЕ ЗАПУСКАТЬ
// var etl = app.Services.GetRequiredService<ITaxiEtlService>();
// await etl.RunAsync();

// Log.Information("ETL finished");

// var cs = builder.Configuration.GetConnectionString("Default");
// using var connection = new SqlConnection(cs);
// await connection.OpenAsync();
// using var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.TaxiTrips", connection);
// var count = (int)await cmd.ExecuteScalarAsync();
// Console.WriteLine($"Rows in TaxiTrips: {count}");
