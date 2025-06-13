namespace FYP_BACKEND_API.Controllers.Model
{
    public class UserLiveLocationEntity
    {

        public string name { get; set; }
        public int LocationId { get; set; }
        public int User_Id {  get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string time { get; set; }
    }
}
