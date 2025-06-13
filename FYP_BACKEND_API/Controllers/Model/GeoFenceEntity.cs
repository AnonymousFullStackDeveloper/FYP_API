namespace FYP_BACKEND_API.Controllers.Model
{
    public class UserGeofence
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Radius { get; set; }
        public string Name { get; set; }

        public List<GeofenceFriend> Friends { get; set; }
    }

    public class GeofenceFriend
    {
        public int Id { get; set; }
        public int GeofenceId { get; set; }
        public int FriendId { get; set; }
        public bool Arrive { get; set; }
        public bool Leave { get; set; }

        //public UserGeofence? Geofence { get; set; }
    }


}
