using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using TaxiDataETL.Core.Interfaces;
using TaxiDataETL.Core.Models;

namespace TaxiDataETL.Core.Services
{
    public class DuplicatesCsvWriter : IDuplicatesWriter
    {
        private readonly string _outputPath;

        public DuplicatesCsvWriter(IConfiguration configuration)
        {
            _outputPath = configuration["Csv:DuplicatesOutputPath"] ?? "duplicates.csv";
        }

        public async Task WriteDuplicatesAsync(IReadOnlyCollection<TaxiTrip> duplicates, CancellationToken ct = default)
        {
            if (duplicates.Count == 0)
                return;

            var dir = Path.GetDirectoryName(_outputPath);

            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using var writer = new StreamWriter(_outputPath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            await csv.WriteRecordsAsync(duplicates, ct);
        }
    }
}
