using CsvHelper.Configuration;
using TaxiDataETL.Core.Models;

namespace TaxiDataETL.Core.Services
{
    public sealed class TaxiTripCsvMap : ClassMap<TaxiTrip>
    {
        public TaxiTripCsvMap()
        {
            Map(m => m.PickupDateTime).Name("tpep_pickup_datetime");
            Map(m => m.DropoffDateTime).Name("tpep_dropoff_datetime");
            Map(m => m.PassengerCount).Name("passenger_count");
            Map(m => m.TripDistance).Name("trip_distance");
            Map(m => m.StoreAndFwdFlag).Name("store_and_fwd_flag");
            Map(m => m.PULocationID).Name("PULocationID");
            Map(m => m.DOLocationID).Name("DOLocationID");
            Map(m => m.FareAmount).Name("fare_amount");
            Map(m => m.TipAmount).Name("tip_amount");
        }
    }
}