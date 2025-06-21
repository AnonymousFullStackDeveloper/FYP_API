using FYP_Backend.Controllers.Model;
using FYP_BACKEND_API.Controllers.DB;
using FYP_BACKEND_API.Controllers.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FYP_BACKEND_API.Controllers
{

    [ApiController]
    [Route("/[controller]")]
    public class UserFriendController : Controller
    {
        DatabaseService databaseService = new DatabaseService();

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            DataTable dataTable = databaseService.GetData("SELECT * from UserFriends");

            List<UserFriends> userFriendList = [];
            foreach (DataRow row in dataTable.Rows)
            {
                var userFriendModel = new UserFriends
                {
                    User_Id = Convert.ToInt32(row["User_Id"]),
                    Friend_Id = Convert.ToInt32(row["Friend_Id"]),
                    Status = Convert.ToString(row["Friendship_Status"])

                };
                userFriendList.Add(userFriendModel);
            }
            databaseService.CloseConnection();

            return Ok(userFriendList);
        }


        [HttpGet("GetFriendsByUserId/{id}")]
        public IActionResult GetByUserId(int id)
        {
            DataTable dataTable = databaseService.GetData("SELECT * from UserFriends where User_Id = "+id+";");

            List<UserFriends> userFriendList = [];
            foreach (DataRow row in dataTable.Rows)
            {
                var userFriendModel = new UserFriends
                {
                    User_Id = Convert.ToInt32(row["User_Id"]),
                    Friend_Id = Convert.ToInt32(row["Friend_Id"]),
                    Status = Convert.ToString(row["Friendship_Status"]),
                    Permission = Convert.ToString(row["Permission"]),

                };
                userFriendList.Add(userFriendModel);
            }
            databaseService.CloseConnection();

            return Ok(userFriendList);
        }




        [HttpGet("GetUserFriendsLiveLocation/{id}")]
        public IActionResult GetUserFriendsLiveLocation(int id)
        {
            DataTable dataTable = databaseService.GetData("SELECT UL.LocationID, UL.UserID, UL.Latitude, UL.Longitude, UL.Timestamp, U.Name FROM UserLiveLocation UL  JOIN UserFriends F ON UL.UserID = F.Friend_Id  JOIN Users U ON UL.UserID = U.Id WHERE F.User_Id = " + id+ " and F.Friendship_Status='Accepted' and F.Permission='Allow';");

            List<UserLiveLocationEntity> userFriendLiveLocationList = [];
            foreach (DataRow row in dataTable.Rows)
            {
                var userFriendModel = new UserLiveLocationEntity
                {
                    name = Convert.ToString(row["Name"]),
                    LocationId = Convert.ToInt32(row["LocationID"]),
                    User_Id = Convert.ToInt32(row["UserID"]),
                    latitude = Convert.ToDouble(row["Latitude"]),
                    longitude = Convert.ToDouble(row["Longitude"]),
                    time = row["Timestamp"].ToString(),

                };
                userFriendLiveLocationList.Add(userFriendModel);
            }
            databaseService.CloseConnection();

            return Ok(userFriendLiveLocationList);
        }




            
        [HttpGet("GetGroupFriendsLiveLocation/{id}")]
        public IActionResult GetLiveLocationUser(int id)
        {
            DataTable dataTable = databaseService.GetData("SELECT ull.UserId, ull.Latitude, ull.Longitude, ull.Timestamp, u.Name FROM Member m INNER JOIN Users u ON m.User_Id = u.Id INNER JOIN UserFriends uf ON uf.Friend_Id = m.User_Id INNER JOIN UserLiveLocation ull ON ull.UserId = m.User_Id WHERE m.Group_Id = "+id+" and uf.Permission='Allow';");

            List<UserLiveLocationEntity> userFriendLiveLocationList = [];
            foreach (DataRow row in dataTable.Rows)
            {
                var userFriendModel = new UserLiveLocationEntity
                {
                    name = row["Name"].ToString(),
                    User_Id = Convert.ToInt32(row["UserID"]),
                    latitude = Convert.ToDouble(row["Latitude"]),
                    longitude = Convert.ToDouble(row["Longitude"]),
                    time = row["Timestamp"].ToString(),

                };
                userFriendLiveLocationList.Add(userFriendModel);
            }
            databaseService.CloseConnection();

            return Ok(userFriendLiveLocationList);
        }







        [HttpPost("UpdateUserLiveLocation")]
        public IActionResult UpdateUserLocation([FromBody] UserLiveLocationEntity userLocationUpdate)
        {
            try
            {
                // Check if the input data is valid
                if (userLocationUpdate == null || userLocationUpdate.User_Id <= 0)
                {
                    return BadRequest("Invalid location data.");
                }

                // Log the input to verify the received data
                //_logger.LogInformation($"Updating location for User: {userLocationUpdate.User_Id}, Latitude: {userLocationUpdate.latitude}, Longitude: {userLocationUpdate.longitude}");

                // SQL query to update user location
                string query = @"UPDATE UserLiveLocation 
                         SET Latitude = @Latitude, Longitude = @Longitude, Timestamp = @Timestamp
                         WHERE UserID = @UserId";

                // Prepare parameters for the SQL query
                var parameters = new SqlParameter[]
                {
            new SqlParameter("@UserId", SqlDbType.Int) { Value = userLocationUpdate.User_Id },
            new SqlParameter("@Latitude", SqlDbType.Float) { Value = userLocationUpdate.latitude },
            new SqlParameter("@Longitude", SqlDbType.Float) { Value = userLocationUpdate.longitude },
            new SqlParameter("@Timestamp", SqlDbType.DateTime) { Value = DateTime.Now.ToString() }
                };

                // Execute the query to update the location
                int result = databaseService.AddData2(query, parameters);
                databaseService.CloseConnection();


                if (result > 0)
                {
                    return Ok(new { message = "Location updated successfully" });
                }
                else
                {
                    return StatusCode(500, new { message = "Failed to update location, no rows affected." });
                }
                
           }
            catch (Exception ex)
            {
                databaseService.CloseConnection();

                // Log the exception details
                //_logger.LogError(ex, "Error updating user location.");

                // Return a detailed error response
                return StatusCode(500, new { message = "An error occurred while updating location", error = ex.Message });
            }
        }




        [HttpGet("GetFriendsDetailByUserId/{id}")]
        public IActionResult GetFriendsDetailsByUserId(int id)
        {
            DataTable dataTable = databaseService.GetData(
                "SELECT U.id, U.name, UF.Friendship_Status, UF.Permission " +
                "FROM UserFriends UF " +
                "JOIN Users U ON UF.friend_id = U.id " +
                "WHERE UF.user_id = " + id + " AND UF.Friendship_Status = 'Accepted' "
            );

            List<Dictionary<string, object>> userFriendList = new List<Dictionary<string, object>>();

            foreach (DataRow row in dataTable.Rows)
            {
                Dictionary<string, object> friendData = new Dictionary<string, object>();

                foreach (DataColumn column in dataTable.Columns)
                {
                    friendData[column.ColumnName] = row[column];
                }

                userFriendList.Add(friendData);
            }
            databaseService.CloseConnection();


            return Ok(userFriendList);
        }


        [HttpGet("GetByFriendId/{id}")]
        public IActionResult GetByFriendId(int id)
        {
            DataTable dataTable = databaseService.GetData("SELECT * from UserFriends where Friend_Id = " + id + ";");

            List<UserFriends> userFriendList = [];
            foreach (DataRow row in dataTable.Rows)
            {
                var userFriendModel = new UserFriends
                {
                    User_Id = Convert.ToInt32(row["User_Id"]),
                    Friend_Id = Convert.ToInt32(row["Friend_Id"]),
                    Status = Convert.ToString(row["Friendship_Status"])

                };
                userFriendList.Add(userFriendModel);
            }
            databaseService.CloseConnection();

            return Ok(userFriendList);
        }


        [HttpGet("GetAllFriendDetails/{id}")]
        public IActionResult GetDetails(int id)
        {
            
            DataTable dataTable = databaseService.GetData("SELECT * from UserFriends where User_Id = " + id + ";");
            databaseService.CloseConnection();

            List<UserFriends> userFriendList = [];
            foreach (DataRow row in dataTable.Rows)
            {
                var userFriendModel = new UserFriends
                {
                    User_Id = Convert.ToInt32(row["User_Id"]),
                    Friend_Id = Convert.ToInt32(row["Friend_Id"]),
                    Status = Convert.ToString(row["Friendship_Status"])

                };
                
                userFriendList.Add(userFriendModel);
            }

            List<UserEntity> userFriendsDetailsList = [];
            foreach (var userFriendId in userFriendList) 
            {

                DataTable dataTable1 = databaseService.GetData("SELECT * from Users where Id = " + userFriendId.Friend_Id + ";");
                databaseService.CloseConnection();

                foreach (DataRow row in dataTable1.Rows)
                {
                    var userModel = new UserEntity
                    {
                        User_Id = Convert.ToInt32(row["Id"]),
                        Name = Convert.ToString(row["Name"]),
                        Email = Convert.ToString(row["Email"]),
                        Password = Convert.ToString(row["Password"]),
                        CreatedAt = Convert.ToString(row["created_at"])
                    };
                    userFriendsDetailsList.Add(userModel);
                }

            }

            //userFriend.userFriendsList = userFriendsDetailsList;
            databaseService.CloseConnection();

            return Ok(userFriendsDetailsList);

        }

        [HttpPost("AddUserFriend")]
        public IActionResult AddUserFriend([FromBody] UserFriends request)
        {
            if (request == null || request.User_Id <= 0 || request.Friend_Id <= 0 || string.IsNullOrEmpty(request.Status))
            {
                return BadRequest("Invalid request payload.");
            }

            // Construct the insert query
            string query = "INSERT INTO UserFriends (User_Id, Friend_Id, Friendship_Status) VALUES (@UserId, @FriendId, @Status);";

            // Prepare parameters
            var parameters = new Dictionary<string, object>
                {
                    { "@UserId", request.User_Id },
                    { "@FriendId", request.Friend_Id },
                    { "@Status", request.Status }
                };

            // Execute the query
            string rowsAffected = databaseService.AddData(query, parameters);
            databaseService.CloseConnection();

            if (int.Parse(rowsAffected) > 0)
            {
                databaseService.CloseConnection();

                return Ok("User friend added successfully.");
            }
            else
            {
                databaseService.CloseConnection();

                return StatusCode(500, "Failed to add user friend.");
            }
        }

        [HttpPost("SendRequestUsingEmail")]
        public IActionResult AddMember([FromBody] AddFriendEntity request)
        {
            if (request == null || request.userId <= 0 || string.IsNullOrEmpty(request.email))
            {
                return BadRequest("Invalid request payload.");
            }

            // 1. Get FriendId from Users table using email
            string getFriendIdQuery = "SELECT Id FROM Users WHERE Email = @Email";
            var parametersForFriend = new Dictionary<string, object>
    {
        { "@Email", request.email }
    };

            var friendIdResult = databaseService.GetScalarValue(getFriendIdQuery, parametersForFriend);
            databaseService.CloseConnection();

            if (friendIdResult == null)
            {
                return NotFound("Friend email not found.");
            }

            int friendId = Convert.ToInt32(friendIdResult);

            // 2. Check if friendship already exists
            string checkFriendshipQuery = "SELECT COUNT(*) FROM UserFriends WHERE User_Id = @User_Id AND Friend_Id = @Friend_Id";
            var parametersForFriendship = new Dictionary<string, object>
    {
        { "@User_Id", request.userId },
        { "@Friend_Id", friendId }
    };

            var existingFriendshipCount = databaseService.GetScalarValue(checkFriendshipQuery, parametersForFriendship);
            databaseService.CloseConnection();

            if (Convert.ToInt32(existingFriendshipCount) > 0)
            {
                return Conflict("Friend already exists.");
            }

            // 3. Insert into UserFriends table
            string insertFriendQuery = @"
        INSERT INTO UserFriends (User_Id, Friend_Id, Friendship_Status, Permission) 
        VALUES (@UserId, @FriendId, @Status, @Permission)";

            var insertParameters = new Dictionary<string, object>
    {
        { "@UserId", request.userId },
        { "@FriendId", friendId },
        { "@Status", request.status },
        { "@Permission", request.permission }
    };

            string rowsAffected = databaseService.AddData(insertFriendQuery, insertParameters);
            databaseService.CloseConnection();

            if (int.TryParse(rowsAffected, out int rows) && rows > 0)
            {
                // 4. Get sender's name
                string getUserNameQuery = "SELECT Name FROM Users WHERE Id = @UserId";
                var parametersForUserName = new Dictionary<string, object>
        {
            { "@UserId", request.userId }
        };

                var userName = databaseService.GetScalarValue(getUserNameQuery, parametersForUserName);
                databaseService.CloseConnection();

                // 5. Insert notification
                string insertNotificationQuery = @"
            INSERT INTO Notifications (UserID, Type, Status, CreatedAt) 
            VALUES (@UserId, @Type, @Status, @CreatedAt)";

                var notificationParameters = new Dictionary<string, object>
        {
            { "@UserId", request.userId },
            { "@Type", $"{userName} sent a friend request" },
            { "@Status", "UnRead" },
            { "@CreatedAt", DateTime.Now }
        };

                string notificationResult = databaseService.AddData(insertNotificationQuery, notificationParameters);
                databaseService.CloseConnection();

                return Ok("Friend request sent and notification added successfully.");
            }

            return StatusCode(500, "Failed to add friend.");
        }

        [HttpPut("UpdateFriendshipStatus")]
        public IActionResult UpdateFriendshipStatus([FromBody] UserFriends request)
        {
            if (request == null || string.IsNullOrEmpty(request.Status))
            {
                return BadRequest("Invalid request payload.");
            }

            // Construct the update query
            string query = "UPDATE UserFriends SET Friendship_Status = @Status WHERE Friend_Id = @FriendId and User_Id= @UserId;";

            // Prepare parameters
            var parameters = new Dictionary<string, object>
                {
                    { "@Status", request.Status },
                    { "@FriendId", request.Friend_Id },
                    { "@UserId", request.User_Id },

                };

            // Execute the query
            string  rowsAffected = databaseService.AddData(query, parameters);
            databaseService.CloseConnection();

            if (int.Parse(rowsAffected) > 0)
            {
                databaseService.CloseConnection();

                return Ok("Friendship status updated successfully.");
            }
            else
            {
                return NotFound($"No record found ");
            }
        }




        [HttpPut("UpdateUserFriendStatus")]
        public IActionResult UpdateUserFriendPermission([FromBody] UserFriends request)
        {
            if (request == null || string.IsNullOrEmpty(request.Status))
            {

                return BadRequest("Invalid request payload.");
            }

            // Construct the update query
            string query = "UPDATE UserFriends SET Friendship_Status = @status WHERE Friend_Id = @FriendId and User_Id= @UserId;";

            // Prepare parameters
            var parameters = new Dictionary<string, object>
                {
                    { "@status", request.Permission },
                    { "@FriendId", request.Friend_Id },
                    { "@UserId", request.User_Id },

                };

            // Execute the query
            string rowsAffected = databaseService.AddData(query, parameters);
            databaseService.CloseConnection();

            if (int.Parse(rowsAffected) > 0)
            {
                databaseService.CloseConnection();

                return Ok("User Friend Permission updated successfully.");
            }
            else
            {
                return NotFound($"No record found ");
            }
        }


        [HttpPut("UpdateUserFriendPermis")]
        public IActionResult UpdateUserFriendPermis([FromBody] UserFriends request)
        {
            if (request == null || string.IsNullOrEmpty(request.Status))
            {

                return BadRequest("Invalid request payload.");
            }

            // Construct the update query
            string query = "UPDATE UserFriends SET Permission = @Permission WHERE Friend_Id = @FriendId and User_Id= @UserId;";

            // Prepare parameters
            var parameters = new Dictionary<string, object>
                {
                    { "@Permission", request.Permission },
                    { "@FriendId", request.Friend_Id },
                    { "@UserId", request.User_Id },

                };

            // Execute the query
            string rowsAffected = databaseService.AddData(query, parameters);
            databaseService.CloseConnection();

            if (int.Parse(rowsAffected) > 0)
            {
                databaseService.CloseConnection();

                return Ok("User Friend Permission updated successfully.");
            }
            else
            {
                return NotFound($"No record found ");
            }
        }


        [HttpDelete("DeleteUserFriend")]
        public IActionResult DeleteUserFriend([FromBody] UserFriends request)
        {
            if (request == null || request.User_Id <= 0 || request.Friend_Id <= 0)
            {
                return BadRequest("Invalid request payload.");
            }

            // Construct the delete query
            string query = "DELETE FROM UserFriends WHERE Friend_Id = @FriendId AND User_Id = @UserId;";

            // Prepare parameters
            var parameters = new Dictionary<string, object>
                {
                    { "@FriendId", request.Friend_Id },
                    { "@UserId", request.User_Id }
                };

            // Execute the query
            string rowsAffected = databaseService.AddData(query, parameters);

            if (int.Parse(rowsAffected) > 0)
            {
                databaseService.CloseConnection();

                return Ok("User friend deleted successfully.");
            }
            else
            {
                databaseService.CloseConnection();

                return NotFound("No record found.");
            }
        }





    }
}
