using FYP_BACKEND_API.Controllers.DB;
using FYP_BACKEND_API.Controllers.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace FYP_BACKEND_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserTripLog : ControllerBase
    {
        DatabaseService databaseService = new DatabaseService();

        [HttpPost("Add")]
        public IActionResult Add([FromBody] UserTripLogEntity location)
        {
            if (location == null || location.UserId <= 0)
                return BadRequest("Invalid payload.");

            string query = @"INSERT INTO UserTripLog (UserId, StartLatitude, StartLongitude, StartTimestamp, EndLatitude, EndLongitude, EndTimestamp)
                         VALUES (@UserId, @StartLat, @StartLng, @StartTime, @EndLat, @EndLng, @EndTime);";

            var parameters = new Dictionary<string, object>
        {
            { "@UserId", location.UserId },
            { "@StartLat", location.StartLatitude },
            { "@StartLng", location.StartLongitude },
            { "@StartTime", location.StartTimestamp },
            { "@EndLat", location.EndLatitude },
            { "@EndLng", location.EndLongitude },
            { "@EndTime", location.EndTimestamp }
        };

            var result = databaseService.AddData(query, parameters);
            databaseService.CloseConnection();

            return Ok("Location history added.");
        }

        [HttpGet("GetByUserId/{userId}")]
        public IActionResult GetByUserId(int userId)
        {
            DataTable table = databaseService.GetData("SELECT * FROM UserTripLog WHERE UserId = " + userId);
            List<UserTripLogEntity> list = new();

            foreach (DataRow row in table.Rows)
            {
                list.Add(new UserTripLogEntity
                {
                    LocationHistoryId = Convert.ToInt32(row["LocationHistoryId"]),
                    UserId = Convert.ToInt32(row["UserId"]),
                    StartLatitude = Convert.ToDecimal(row["StartLatitude"]),
                    StartLongitude = Convert.ToDecimal(row["StartLongitude"]),
                    StartTimestamp = Convert.ToDateTime(row["StartTimestamp"]),
                    EndLatitude = Convert.ToDecimal(row["EndLatitude"]),
                    EndLongitude = Convert.ToDecimal(row["EndLongitude"]),
                    EndTimestamp = Convert.ToDateTime(row["EndTimestamp"])
                });
            }

            databaseService.CloseConnection();
            return Ok(list);
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            DataTable table = databaseService.GetData("SELECT * FROM UserTripLog");
            List<UserTripLogEntity> list = new();

            foreach (DataRow row in table.Rows)
            {
                list.Add(new UserTripLogEntity
                {
                    LocationHistoryId = Convert.ToInt32(row["LocationHistoryId"]),
                    UserId = Convert.ToInt32(row["UserId"]),
                    StartLatitude = Convert.ToDecimal(row["StartLatitude"]),
                    StartLongitude = Convert.ToDecimal(row["StartLongitude"]),
                    StartTimestamp = Convert.ToDateTime(row["StartTimestamp"]),
                    EndLatitude = Convert.ToDecimal(row["EndLatitude"]),
                    EndLongitude = Convert.ToDecimal(row["EndLongitude"]),
                    EndTimestamp = Convert.ToDateTime(row["EndTimestamp"])
                });
            }

            databaseService.CloseConnection();
            return Ok(list);
        }
    }

}
