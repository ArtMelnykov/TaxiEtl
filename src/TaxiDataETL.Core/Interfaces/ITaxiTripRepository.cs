using TaxiDataETL.Core.Models;

namespace TaxiDataETL.Core.Interfaces
{
    public interface ITaxiTripRepository
    {
        Task BulkInsertAsync(IReadOnlyCollection<TaxiTrip> trips, CancellationToken ct = default);
    }
}