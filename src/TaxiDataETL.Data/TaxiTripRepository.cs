using System.Data;
using Microsoft.Data.SqlClient;
using TaxiDataETL.Core.Interfaces;
using TaxiDataETL.Core.Models;

namespace TaxiDataETL.Data
{
    public class TaxiTripRepository : ITaxiTripRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public TaxiTripRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task BulkInsertAsync(
            IReadOnlyCollection<TaxiTrip> trips,
            CancellationToken ct = default)
        {
            if (trips.Count == 0)
                return;

            using var connection = _connectionFactory.Create();
            await connection.OpenAsync(ct);

            using var bulk = new SqlBulkCopy(connection)
            {
                DestinationTableName = "dbo.TaxiTrips"
            };

            bulk.ColumnMappings.Add(nameof(TaxiTrip.PickupDateTime), "PickupDateTime");
            bulk.ColumnMappings.Add(nameof(TaxiTrip.DropoffDateTime), "DropoffDateTime");
            bulk.ColumnMappings.Add(nameof(TaxiTrip.PassengerCount), "PassengerCount");
            bulk.ColumnMappings.Add(nameof(TaxiTrip.TripDistance), "TripDistance");
            bulk.ColumnMappings.Add(nameof(TaxiTrip.StoreAndFwdFlag), "StoreAndFwdFlag");
            bulk.ColumnMappings.Add(nameof(TaxiTrip.PULocationID), "PULocationID");
            bulk.ColumnMappings.Add(nameof(TaxiTrip.DOLocationID), "DOLocationID");
            bulk.ColumnMappings.Add(nameof(TaxiTrip.FareAmount), "FareAmount");
            bulk.ColumnMappings.Add(nameof(TaxiTrip.TipAmount), "TipAmount");

            using var table = ToDataTable(trips);

            await bulk.WriteToServerAsync(table, ct);
        }

        private static DataTable ToDataTable(IEnumerable<TaxiTrip> trips)
        {
            var table = new DataTable();

            table.Columns.Add("PickupDateTime", typeof(DateTime));
            table.Columns.Add("DropoffDateTime", typeof(DateTime));
            table.Columns.Add("PassengerCount", typeof(byte));
            table.Columns.Add("TripDistance", typeof(decimal));
            table.Columns.Add("StoreAndFwdFlag", typeof(string));
            table.Columns.Add("PULocationID", typeof(int));
            table.Columns.Add("DOLocationID", typeof(int));
            table.Columns.Add("FareAmount", typeof(decimal));
            table.Columns.Add("TipAmount", typeof(decimal));

            foreach (var t in trips)
            {
                table.Rows.Add(
                    t.PickupDateTime,
                    t.DropoffDateTime,
                    t.PassengerCount,
                    t.TripDistance,
                    t.StoreAndFwdFlag,
                    t.PULocationID,
                    t.DOLocationID,
                    t.FareAmount,
                    t.TipAmount);
            }

            return table;
        }
    }
}
