using TaxiDataETL.Core.Models;

namespace TaxiDataETL.Core.Interfaces
{
    public interface IDuplicatesWriter
    {
        Task WriteDuplicatesAsync(IReadOnlyCollection<TaxiTrip> duplicates, CancellationToken ct = default);
    }
}