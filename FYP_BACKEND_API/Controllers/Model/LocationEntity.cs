namespace FYP_Backend.Controllers.Model
{
    public class LocationEntity
    {
        public int UserId { get; set; }
        public int LocationId { get; set; }
        public string Name { get; set; }
        public Decimal Latitude { get; set; }
        public Decimal Longitude { get; set; }
        public string Timestamp { get; set; }
    }
}
