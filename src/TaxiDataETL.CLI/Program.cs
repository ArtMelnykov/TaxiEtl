using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using TaxiDataETL.Core;
using TaxiDataETL.Core.Interfaces;
using TaxiDataETL.Core.Models;
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

builder.Services.AddCoreServices();
builder.Services.AddDataServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var csvReader = scope.ServiceProvider.GetRequiredService<ICsvTripReader>();
    var repo = scope.ServiceProvider.GetRequiredService<ITaxiTripRepository>();

    await foreach (var trip in csvReader.ReadTripsAsync())
    {
        Console.WriteLine(
            $"Pickup: {trip.PickupDateTime}, Dropoff: {trip.DropoffDateTime}, " +
            $"Dist: {trip.TripDistance}, Tip: {trip.TipAmount}, PU: {trip.PULocationID}, DO: {trip.DOLocationID}");
        break;
    }

    var batch = new List<TaxiTrip>();
    int count = 0;

    await foreach (var trip in csvReader.ReadTripsAsync())
    {
        batch.Add(trip);
        count++;
        if (count >= 100)
            break;
    }

    await repo.BulkInsertAsync(batch);

    Console.WriteLine($"Inserted {batch.Count} trips into dbo.TaxiTrips.");
}
