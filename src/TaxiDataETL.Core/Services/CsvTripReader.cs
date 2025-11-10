using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using TaxiDataETL.Core.Interfaces;
using TaxiDataETL.Core.Models;

namespace TaxiDataETL.Core.Services;

public class CsvTripReader : ICsvTripReader
{
    private readonly string _csvPath;

    public CsvTripReader(IConfiguration configuration)
    {
        _csvPath = configuration["InputCsvPath"]
                   ?? throw new InvalidOperationException("InputCsvPath is not configured");
    }

    public async IAsyncEnumerable<TaxiTrip> ReadTripsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!File.Exists(_csvPath))
            throw new FileNotFoundException($"CSV file not found at path: {_csvPath}");

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            BadDataFound = null
        };

        using var reader = new StreamReader(_csvPath);
        using var csv = new CsvReader(reader, csvConfig);

        csv.Context.RegisterClassMap<TaxiTripCsvMap>();

        await foreach (var record in csv.GetRecordsAsync<TaxiTrip>(ct))
        {
            yield return record;
        }
    }
}
