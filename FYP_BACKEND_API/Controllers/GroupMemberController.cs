using FYP_Backend.Controllers.Model;
using FYP_BACKEND_API.Controllers.DB;
using FYP_BACKEND_API.Controllers.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace FYP_BACKEND_API.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class GroupMemberController : Controller
    {

        DatabaseService databaseService = new();

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {

            DataTable dataTable = databaseService.GetData("SELECT * from Member");

            List<GroupMemberEntity> list = [];
            foreach (DataRow row in dataTable.Rows)
            {
                var memberEntity = new GroupMemberEntity()
                {
                    User_Id = Convert.ToInt32(row["User_Id"]),
                    GroupId = Convert.ToInt32(row["Group_Id"]),
                    Role = Convert.ToString(row["Role"]),
                    
                };
                list.Add(memberEntity);
            }
            databaseService.CloseConnection();

            return Ok(list);

        }


        [HttpGet("GetByUserId/{id}")]
        public IActionResult GetByUserId(int id)
        {
            DataTable dataTable = databaseService.GetData("SELECT * FROM Groups WHERE id IN (SELECT Group_Id FROM Member WHERE User_Id ="+id+");");

            List<GroupEntity> list = [];
            foreach (DataRow row in dataTable.Rows)
            {
                var groupEntity = new GroupEntity
                {
                    Group_Id = Convert.ToInt32(row["Id"]),
                    GroupName = Convert.ToString(row["Name"]),
                    Group_Code = Convert.ToString(row["Group_Code"]),
                    CreatedAt = Convert.ToString(row["Created_at"]),
                    IsPrivate = Convert.ToBoolean(row["IsPrivate"])

                };
                list.Add(groupEntity);
            }
            databaseService.CloseConnection();

            return Ok(list);
        }

        [HttpGet("GetByGroupId/{id}")]
        public IActionResult GetByGroupId(int id)
        {
            DataTable dataTable = databaseService.GetData("SELECT * FROM Users WHERE Id IN (SELECT User_Id FROM Member WHERE Group_Id = "+id+");");

            List<UserEntity> list = [];
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
                list.Add(userModel);
            }
            databaseService.CloseConnection();

            return Ok(list);
        }




        [HttpPost("AddData")]
        public IActionResult AddGroup([FromBody] GroupMemberEntity memberEntity)
        {
            var query = "INSERT INTO Member (User_Id,Group_Id,Role) VALUES (@User_Id,@Group_Id,@Role)";
            var parameters = new Dictionary<string, object>
                {
                    { "@User_Id", memberEntity.User_Id },
                    { "@Group_Id", memberEntity.GroupId },
                    { "@Role", memberEntity.Role },
                    
                };
            databaseService.CloseConnection();

            return Ok(databaseService.AddData(query, parameters));
        }


        [HttpPost("AddGroupMember")]
        public IActionResult AddMemberInGroup([FromBody] AddingMemberInGroup memberEntity)
        {
            try
            {
                // Step 1: Get Group_Id from Group_Code
                string getGroupIdQuery = "SELECT Id FROM Groups WHERE Group_Code = @Group_Code";
                var groupIdResult = databaseService.GetScalarValue(getGroupIdQuery, new Dictionary<string, object>
        {
            { "@Group_Code", memberEntity.Group_Code }
        });

                databaseService.CloseConnection();
                if (groupIdResult == null)
                {
                    databaseService.CloseConnection();
                    return NotFound("Group code not found.");
                }

                int groupId = Convert.ToInt32(groupIdResult);

                // Step 2: Insert into Member table
                string insertQuery = "INSERT INTO Member (User_Id, Group_Id, Role) VALUES (@User_Id, @Group_Id, @Role)";
                var insertParams = new Dictionary<string, object>
        {
            { "@User_Id", memberEntity.User_Id },
            { "@Group_Id", groupId },
            { "@Role", memberEntity.Role }
        };

                var result = databaseService.AddData(insertQuery, insertParams);
                databaseService.CloseConnection();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }



        [HttpPut("UpdateGroupMember")]
        public IActionResult UpdateGroupMember([FromBody] GroupMemberEntity memberEntity)
        {
            if (memberEntity == null || memberEntity.User_Id <= 0 || memberEntity.GroupId <= 0 || string.IsNullOrEmpty(memberEntity.Role))
            {
                return BadRequest("Invalid request payload.");
            }

            var query = "UPDATE Member SET Role = @Role WHERE User_Id = @User_Id AND Group_Id = @Group_Id;";
            var parameters = new Dictionary<string, object>
                {
                    { "@User_Id", memberEntity.User_Id },
                    { "@Group_Id", memberEntity.GroupId },
                    { "@Role", memberEntity.Role }
                };

            string rowsAffected = databaseService.AddData(query, parameters);
            databaseService.CloseConnection();

            if (int.Parse(rowsAffected) > 0)
            {
                databaseService.CloseConnection();

                return Ok("Group member updated successfully.");
            }
            else
            {
                databaseService.CloseConnection();

                return NotFound("No matching record found to update.");
            }
        }

        [HttpDelete("DeleteGroupMember")]
        public IActionResult DeleteGroupMember([FromBody] GroupMemberEntity memberEntity)
        {
            if (memberEntity == null || memberEntity.User_Id <= 0 || memberEntity.GroupId <= 0)
            {
                return BadRequest("Invalid request payload.");
            }

            var query = "DELETE FROM Member WHERE User_Id = @User_Id AND Group_Id = @Group_Id;";
            var parameters = new Dictionary<string, object>
                {
                    { "@User_Id", memberEntity.User_Id },
                    { "@Group_Id", memberEntity.GroupId }
                };

            string rowsAffected = databaseService.AddData(query, parameters);
            databaseService.CloseConnection();

            if (int.Parse(rowsAffected) > 0)
            {
                databaseService.CloseConnection();

                return Ok("Group member deleted successfully.");
            }
            else
            {
                databaseService.CloseConnection();

                return NotFound("No matching record found to delete.");
            }
        }


    }
}
