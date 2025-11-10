namespace TaxiDataETL.Core.Models
{
    public class TaxiTrip
    {
        public DateTime PickupDateTime { get; set; }
        public DateTime DropoffDateTime { get; set; }
        public byte PassengerCount { get; set; }
        public decimal TripDistance { get; set; }
        public string StoreAndFwdFlag { get; set; } = "";
        public int PULocationID { get; set; }
        public int DOLocationID { get; set; }
        public decimal FareAmount { get; set; }
        public decimal TipAmount { get; set; }
    }
}