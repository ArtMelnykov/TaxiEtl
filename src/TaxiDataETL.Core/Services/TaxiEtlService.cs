using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaxiDataETL.Core.Interfaces;
using TaxiDataETL.Core.Models;

namespace TaxiDataETL.Core.Services
{
    public class TaxiEtlService : ITaxiEtlService
    {
        private readonly ICsvTripReader _reader;
        private readonly ITaxiTripRepository _repository;
        private readonly IDuplicatesWriter _duplicatesWriter;
        private readonly ILogger<TaxiEtlService> _logger;

        private readonly TimeZoneInfo? _sourceTimeZone;
        private readonly int _batchSize;

        public TaxiEtlService(ICsvTripReader reader, ITaxiTripRepository repository, IDuplicatesWriter duplicatesWriter, IConfiguration configuration, ILogger<TaxiEtlService> logger)
        {
            _reader = reader;
            _repository = repository;
            _duplicatesWriter = duplicatesWriter;
            _logger = logger;

            _batchSize = configuration.GetValue<int?>("Csv:BatchSize") ?? 5000;

            var tzId = configuration["Csv:SourceTimeZone"];
            if (!string.IsNullOrWhiteSpace(tzId))
            {
                try
                {
                    _sourceTimeZone = TimeZoneInfo.FindSystemTimeZoneById(tzId);
                }
                catch (TimeZoneNotFoundException)
                {
                    // fallback for Linux
                    if (tzId == "Eastern Standard Time")
                    {
                        try
                        {
                            _sourceTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
                        }
                        catch
                        {
                            _logger.LogWarning("Source timezone not found: {TzId}", tzId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Source timezone not found: {TzId}", tzId);
                    }
                }
            }
        }

        public async Task RunAsync(CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();

            var batch = new List<TaxiTrip>(_batchSize);
            var duplicates = new List<TaxiTrip>();
            var seenKeys = new HashSet<(DateTime, DateTime, byte)>();

            int total = 0;

            await foreach (var raw in _reader.ReadTripsAsync(ct))
            {
                ct.ThrowIfCancellationRequested();
                total++;

                var trip = Normalize(raw);

                var key = (trip.PickupDateTime, trip.DropoffDateTime, trip.PassengerCount);

                if (!seenKeys.Add(key))
                {
                    duplicates.Add(trip);
                    continue;
                }

                batch.Add(trip);

                if (batch.Count >= _batchSize)
                {
                    await _repository.BulkInsertAsync(batch, ct);
                    _logger.LogInformation("Inserted batch of {Count} trips (total processed: {Total})",
                        batch.Count, total);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                await _repository.BulkInsertAsync(batch, ct);
                _logger.LogInformation("Inserted final batch of {Count} trips (total processed: {Total})",
                    batch.Count, total);
            }

            if (duplicates.Count > 0)
            {
                await _duplicatesWriter.WriteDuplicatesAsync(duplicates, ct);
                _logger.LogInformation("Wrote {Count} duplicate trips to file", duplicates.Count);
            }

            _logger.LogInformation(
                "ETL summary: Total={Total}, Inserted={Inserted}, Duplicates={Duplicates}, ExecutionTime={Seconds:F1}s",
                total,
                seenKeys.Count,
                duplicates.Count,
                sw.Elapsed.TotalSeconds);
        }

        private TaxiTrip Normalize(TaxiTrip trip)
        {
            trip.StoreAndFwdFlag = trip.StoreAndFwdFlag?.Trim();

            // Trim
            if (!string.IsNullOrEmpty(trip.StoreAndFwdFlag))
            {
                trip.StoreAndFwdFlag = trip.StoreAndFwdFlag switch
                {
                    "Y" => "Yes",
                    "N" => "No",
                    _ => trip.StoreAndFwdFlag
                };
            }

            // EST -> UTC
            if (_sourceTimeZone is not null)
            {
                trip.PickupDateTime = TimeZoneInfo.ConvertTimeToUtc(trip.PickupDateTime, _sourceTimeZone);
                trip.DropoffDateTime = TimeZoneInfo.ConvertTimeToUtc(trip.DropoffDateTime, _sourceTimeZone);
            }

            return trip;
        }
    }
}
