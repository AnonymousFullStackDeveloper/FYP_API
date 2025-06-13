using FYP_Backend.Controllers.Model;
using FYP_BACKEND_API.Controllers.DB;
using FYP_BACKEND_API.Controllers.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data;

using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using BCrypt.Net;
using Microsoft.Data.SqlClient; // Install BCrypt.Net-Next via NuGet
namespace FYP_BACKEND_API.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class UserController : Controller
    {
        DatabaseService databaseService = new DatabaseService();

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            DataTable dataTable = databaseService.GetData("SELECT * from Users");

            List<UserEntity> userList = [];
            foreach (DataRow row in dataTable.Rows)
            {
                var userModel = new UserEntity
                {
                    User_Id = Convert.ToInt32(row["Id"]),
                    Name = Convert.ToString(row["Name"]),
                    Email = Convert.ToString(row["Email"]),
                    Password = Convert.ToString(row["Password"]),
                    CreatedAt = Convert.ToString(row["created_at"])
                };
                userList.Add(userModel);
            }
            databaseService.CloseConnection();

            return Ok(userList);
        }

        [HttpPost("Login")]
        public IActionResult LoginUser([FromBody] LoginUserEntity loginUser)
        {
            DataTable dataTable = databaseService.GetData("SELECT * from Users where Email = '" + loginUser.Email + "' and Password = '" + loginUser.Password + "';");
            UserEntity userModel1 = new UserEntity();
            List<UserEntity> userList = [];
            foreach (DataRow row in dataTable.Rows)
            {
                var userModel = new UserEntity
                {
                    User_Id = Convert.ToInt32(row["Id"]),
                    Name = Convert.ToString(row["Name"]),
                    Email = Convert.ToString(row["Email"]),
                    Password = Convert.ToString(row["Password"]),
                    CreatedAt = Convert.ToString(row["created_at"])

                };
                userList.Add(userModel);
                userModel1 = userModel;
            }
            databaseService.CloseConnection();

            return Ok(userModel1);
        }



        [HttpPost("Signup")]
        public async Task<IActionResult> AddUser([FromBody] SignupDetails newUser)
        {
           
            var checkQuery = "SELECT * FROM Users WHERE Email = "+newUser.Email+";";
            DataTable result = databaseService.GetData(checkQuery);

            if (result != null)
            {
                foreach (DataRow dr in result.Rows)
                {
                    databaseService.CloseConnection();

                    return Ok(false);
                }
            }
            databaseService.CloseConnection();


            var insertQuery = "INSERT INTO Users (Name, Email, Password) VALUES (@Name, @Email, @Password)";
            var insertParams = new Dictionary<string, object>
            {
                { "@Name", newUser.Name },
                { "@Email", newUser.Email },
                { "@Password", newUser.Password },
            };

            try
            {
                bool isInserted = databaseService.AddData1(insertQuery, insertParams);

                databaseService.CloseConnection();

                return Created("Success", new { message = isInserted.ToString() });
            }
            catch (SqlException ex) when (ex.Number == 2627) // Handle UNIQUE KEY constraint violation
            {
                databaseService.CloseConnection();
                return Conflict(new { message = "Email already exists. Please log in." });
            }
            catch (Exception ex)
            {

                databaseService.CloseConnection();
                return StatusCode(500, new { message = "Internal Server Error: " + ex.Message });
            }
        }


        [HttpGet("GetById/{id}")]
        public IActionResult GetUserByID(int id)
        {
            DataTable dataTable = databaseService.GetData("SELECT * from Users where Id= " + id + ";");

            List<UserEntity> userList = [];
            foreach (DataRow row in dataTable.Rows)
            {
                var userModel = new UserEntity
                {
                    User_Id = Convert.ToInt32(row["Id"]),
                    Name = Convert.ToString(row["Name"]),
                    Email = Convert.ToString(row["Email"]),
                    Password = Convert.ToString(row["Password"]),
                    CreatedAt = Convert.ToString(row["created_at"])

                };
                userList.Add(userModel);
            }
            databaseService.CloseConnection();

            return Ok(userList);

        }

        [HttpPut("UpdateById/{id}")]
        public IActionResult UpdateUserById(int id, [FromBody] UserEntity newUser)
        {
            
            var query = "UPDATE Users SET Name = @Name, Email = @Email, Password=@Password,Created_at=@CreatedAt WHERE Id = @UserId;";

            var parameters = new Dictionary<string, object>
                {
                    { "@Name", newUser.Name },
                    { "@Email", newUser.Email },
                    { "@Password", newUser.Password },
                    { "@CreatedAt", DateTime.Now },
                    { "@UserId", id }
                };
            databaseService.CloseConnection();

            return Ok(databaseService.Update(query, parameters));


        }
        [HttpDelete("DeleteById/{id}")]
        public IActionResult DeleteById(int id)
        {
            var query = "Delete from Users WHERE Id = @UserId;";

            var parameters = new Dictionary<string, object>
                {

                    { "@UserId", id }
                };
            databaseService.CloseConnection();

            return Ok(databaseService.DeleteAsync(query, parameters).ToString());

        }

        [HttpGet("GetAllDetails/{id}")]
        public IActionResult GetDetails(int id)
        {
            UserEntity user = new UserEntity
            {
                UserDetails = new UserDetails()
            };


            DataTable dataTable4 = databaseService.GetData("SELECT * from Users where Id = "+id+";");

            List<UserEntity> userList = [];
            foreach (DataRow row in dataTable4.Rows)
            {
                var userModel = new UserEntity
                {
                    User_Id = Convert.ToInt32(row["Id"]),
                    Name = Convert.ToString(row["Name"]),
                    Email = Convert.ToString(row["Email"]),
                    Password = Convert.ToString(row["Password"]),
                    CreatedAt = Convert.ToString(row["created_at"])
                };
                user.Name = userModel.Name;
                user.Email = userModel.Email;
                user.User_Id = userModel.User_Id;
                user.Password = userModel.Password;
                user.CreatedAt = userModel.CreatedAt;
                //userList.Add(userModel);
            }

            


            DataTable dataTable = databaseService.GetData("SELECT * from Member where User_Id = "+id+";");

            databaseService.CloseConnection();
            List<GroupMemberEntity> groupMemberList = new List<GroupMemberEntity>();
            foreach (DataRow row in dataTable.Rows)
            {
                var memberEntity = new GroupMemberEntity()
                {
                    User_Id = Convert.ToInt32(row["User_Id"]),
                    GroupId = Convert.ToInt32(row["Group_Id"]),
                    Role = Convert.ToString(row["Role"]),

                };
                groupMemberList.Add(memberEntity);
            }

            DataTable dataTable1 = databaseService.GetData("SELECT * from UserFriends where User_Id = "+id+";");

            databaseService.CloseConnection();
            List<UserFriends> userFriendList = new List<UserFriends>();
            foreach (DataRow row in dataTable1.Rows)
            {
                var userFriendModel = new UserFriends
                {
                    User_Id = Convert.ToInt32(row["User_Id"]),
                    Friend_Id = Convert.ToInt32(row["Friend_Id"]),
                    Status = Convert.ToString(row["Friendship_Status"])

                };
                userFriendList.Add(userFriendModel);
            }


            DataTable dataTable2 = databaseService.GetData("SELECT * from UserGeofence where User_Id = " + id + ";");


            databaseService.CloseConnection();


            DataTable dataTable3 = databaseService.GetData("SELECT * from User_Location_History where User_Id=" + id + ";");

            List<User_Location_History> userLocationHistorylist = [];
            foreach (DataRow row in dataTable3.Rows)
            {
                var userLocationHistoryEntity = new User_Location_History()
                {
                    User_Id = Convert.ToInt32(row["User_Id"]),
                    Location_History_Id = Convert.ToInt32(row["Location_History_Id"]),

                };
                userLocationHistorylist.Add(userLocationHistoryEntity);
            }



           

            if (groupMemberList.Any())
            {
                user.UserDetails.groupMembers = groupMemberList;
            }

            if (userFriendList.Any())
            {
                user.UserDetails.userFriends = userFriendList;
            }

            


            databaseService.CloseConnection();

            return Ok(user);

            

        }




    }
}
