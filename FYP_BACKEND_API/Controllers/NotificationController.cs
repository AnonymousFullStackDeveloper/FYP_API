using FYP_BACKEND_API.Controllers.DB;
using FYP_BACKEND_API.Controllers.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
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

        [HttpGet("GetUserUnreadCount/{userId}")]
        public IActionResult GetUnreadNotificationCount(int userId)
        {
            string query = "SELECT COUNT(*) FROM Notifications WHERE UserID = @UserId AND Status != 'Read'";

            using (SqlConnection connection = new SqlConnection("Server=DEVELOPER;Database=FYP_DB;User Id=sa;Password=123456;TrustServerCertificate=True;"))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    connection.Open();
                    int unreadCount = (int)command.ExecuteScalar();
                    connection.Close();
                    return Ok(unreadCount.ToString());
                }
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
            string query = "SELECT * FROM Notifications WHERE UserID = " + userId + " ORDER BY NotificationID DESC;";


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
        [HttpGet("GetUserNotificationsByUserId/{userId}")]
        public IActionResult GetAllNotificationsByUserId(int userId)
        {
            if (userId <= 0)
                return BadRequest("Invalid user ID.");

            string query = "SELECT * FROM Notifications WHERE UserID = @UserId ORDER BY NotificationID DESC;";

            // Corrected SqlConnection syntax
            using (SqlConnection connection = new SqlConnection("Server=DEVELOPER;Database=FYP_DB;User Id=sa;Password=123456;TrustServerCertificate=True;"))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();

                    connection.Open(); // IMPORTANT: Open connection before filling
                    adapter.Fill(dataTable);

                    if (dataTable.Rows.Count == 0)
                        return NotFound("No notifications found for the given user ID.");

                    List<NotificationEntity> notifications = new List<NotificationEntity>();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        notifications.Add(new NotificationEntity
                        {
                            NotificationId = Convert.ToInt32(row["NotificationID"]),
                            UserId = Convert.ToInt32(row["UserID"]),
                            Type = Convert.ToString(row["Type"]),
                            Status = Convert.ToString(row["Status"]),
                            CreatedAt = Convert.ToDateTime(row["CreatedAt"])
                        });
                    }

                    return Ok(notifications);
                }
            }
        }




        [HttpGet("GetAndMarkNotificationsAsRead/{userId}")]
        public IActionResult GetAndMarkNotificationsAsRead(int userId)
        {
            if (userId <= 0)
                return BadRequest("Invalid user ID.");

            string selectQuery = "SELECT * FROM Notifications WHERE UserID = @UserId ORDER BY NotificationID DESC;";
            string updateQuery = "UPDATE Notifications SET Status = 'Read' WHERE UserID = @UserId AND Status != 'Read';";

            using (SqlConnection connection = new SqlConnection("Server=DEVELOPER;Database=FYP_DB;User Id=sa;Password=123456;TrustServerCertificate=True;"))
            {
                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@UserId", userId);
                    updateCommand.Parameters.AddWithValue("@UserId", userId);

                    SqlDataAdapter adapter = new SqlDataAdapter(selectCommand);
                    DataTable dataTable = new DataTable();

                    connection.Open();
                    adapter.Fill(dataTable);

                    if (dataTable.Rows.Count == 0)
                        return NotFound("No notifications found for the given user ID.");

                    // Update all statuses to "Read"
                    updateCommand.ExecuteNonQuery();

                    List<NotificationEntity> notifications = new List<NotificationEntity>();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        notifications.Add(new NotificationEntity
                        {
                            NotificationId = Convert.ToInt32(row["NotificationID"]),
                            UserId = Convert.ToInt32(row["UserID"]),
                            Type = Convert.ToString(row["Type"]),
                            Status = "Read", // Mark as read since we've updated it
                            CreatedAt = Convert.ToDateTime(row["CreatedAt"])
                        });
                    }

                    return Ok(notifications);
                }
            }
        }




    }
}
