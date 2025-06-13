using FYP_BACKEND_API.Controllers.DB;
using FYP_BACKEND_API.Controllers.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace FYP_BACKEND_API.Controllers
{

    [ApiController]
    [Route("/[controller]")]
    public class NotificationController : Controller
    {
        DatabaseService databaseService = new DatabaseService();



        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            DataTable dataTable = databaseService.GetData("SELECT * FROM Notifications");

            List<NotificationEntity> notifications = new List<NotificationEntity>();
            foreach (DataRow row in dataTable.Rows)
            {
                var notification = new NotificationEntity
                {
                    NotificationId = Convert.ToInt32(row["NotificationID"]),
                    UserId = Convert.ToInt32(row["UserID"]),
                    Type = Convert.ToString(row["Type"]),
                    Status = Convert.ToString(row["Status"]),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"])
                };
                notifications.Add(notification);
            }
            return Ok(notifications);
        }


        [HttpPost("AddNotification")]
        public IActionResult AddNotification([FromBody] NotificationEntity request)
        {
            if (request == null || request.UserId <= 0 || string.IsNullOrEmpty(request.Type) || string.IsNullOrEmpty(request.Status))
            {
                return BadRequest("Invalid request payload.");
            }

            // Construct the insert query
            string query = "INSERT INTO Notifications (UserID, Type, Status, CreatedAt) VALUES (@UserId, @Type, @Status, GETDATE());";

            // Prepare parameters
            var parameters = new Dictionary<string, object>
                {
                    { "@UserId", request.UserId },
                    { "@Type", request.Type },
                    { "@Status", request.Status }
                };

            // Execute the query
            string rowsAffected = databaseService.AddData(query, parameters);

            if (int.Parse(rowsAffected) > 0)
            {
                return Ok("Notification added successfully.");
            }
            else
            {
                return StatusCode(500, "Failed to add notification.");
            }
        }


        [HttpPut("UpdateNotification")]
        public IActionResult UpdateNotification([FromBody] NotificationEntity request)
        {
            if (request == null || request.NotificationId <= 0 || string.IsNullOrEmpty(request.Status))
            {
                return BadRequest("Invalid request payload.");
            }

            // Construct the update query
            string query = "UPDATE Notifications SET Status = @Status, Type = @Type WHERE NotificationID = @NotificationId;";

            // Prepare parameters
            var parameters = new Dictionary<string, object>
    {
        { "@NotificationId", request.NotificationId },
        { "@Status", request.Status },
        { "@Type", request.Type }
    };

            // Execute the query
            string rowsAffected = databaseService.AddData(query, parameters);

            if (int.Parse(rowsAffected) > 0)
            {
                return Ok("Notification updated successfully.");
            }
            else
            {
                return StatusCode(500, "Failed to update notification.");
            }
        }


        [HttpDelete("DeleteNotification/{id}")]
        public IActionResult DeleteNotification(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid notification ID.");
            }

            // Construct the delete query
            string query = "DELETE FROM Notifications WHERE NotificationID = @NotificationId;";

            // Prepare parameters
            var parameters = new Dictionary<string, object>
    {
        { "@NotificationId", id }
    };

            // Execute the query
            string rowsAffected = databaseService.AddData(query, parameters);

            if (int.Parse(rowsAffected) > 0)
            {
                return Ok("Notification deleted successfully.");
            }
            else
            {
                return StatusCode(500, "Failed to delete notification.");
            }
        }



        [HttpGet("GetByUserId/{userId}")]
        public IActionResult GetNotificationsByUserId(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            // Construct the select query
            string query = "SELECT * FROM Notifications WHERE UserID = "+userId+" ORDER BY NotificationID DESC;";

            
            // Execute the query and retrieve data
            DataTable dataTable = databaseService.GetData(query);

            if (dataTable.Rows.Count == 0)
            {
                return NotFound("No notifications found for the given user ID.");
            }

            List<NotificationEntity> notifications = new List<NotificationEntity>();
            foreach (DataRow row in dataTable.Rows)
            {
                var notification = new NotificationEntity
                {
                    NotificationId = Convert.ToInt32(row["NotificationID"]),
                    UserId = Convert.ToInt32(row["UserID"]),
                    Type = Convert.ToString(row["Type"]),
                    Status = Convert.ToString(row["Status"]),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"])
                };
                notifications.Add(notification);
            }

            return Ok(notifications);
        }


    }
}
