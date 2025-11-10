using TaxiDataETL.Core.Models;

namespace TaxiDataETL.Core.Interfaces
{
    public interface ICsvTripReader
    {
        IAsyncEnumerable<TaxiTrip> ReadTripsAsync(CancellationToken ct = default);
    }
}