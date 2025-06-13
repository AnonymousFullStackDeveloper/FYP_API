using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FYP_BACKEND_API.Controllers.Model
{
    public class UserTripLogEntity
    {
        public int LocationHistoryId { get; set; }
        public int UserId { get; set; }

        public decimal StartLatitude { get; set; }

        public decimal StartLongitude { get; set; }

        public DateTime StartTimestamp { get; set; }

        public decimal EndLatitude { get; set; }

        public decimal EndLongitude { get; set; }

        public DateTime EndTimestamp { get; set; }
    }
}
