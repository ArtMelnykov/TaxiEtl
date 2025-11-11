namespace TaxiDataETL.Core.Interfaces
{
    public interface ITaxiEtlService
    {
        Task RunAsync(CancellationToken ct = default);
    }
}