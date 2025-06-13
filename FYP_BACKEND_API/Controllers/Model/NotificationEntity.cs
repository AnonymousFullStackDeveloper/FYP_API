namespace FYP_BACKEND_API.Controllers.Model
{
    public class NotificationEntity
    {

        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
