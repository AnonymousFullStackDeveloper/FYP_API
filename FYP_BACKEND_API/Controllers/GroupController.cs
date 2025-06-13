using FYP_Backend.Controllers.Model;
using FYP_BACKEND_API.Controllers.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace FYP_BACKEND_API.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class GroupController : Controller
    {
        DatabaseService databaseService = new();

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            DataTable dataTable = databaseService.GetData("SELECT * from Groups");

            List<GroupEntity> groupList = [];
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
                groupList.Add(groupEntity);
            }
            databaseService.CloseConnection();

            return Ok(groupList);
        }

        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {

            DataTable dataTable = databaseService.GetData("SELECT * from Groups where Id= " + id + ";");

            GroupEntity groupEntity1 = new GroupEntity();
            List<GroupEntity> groupList = [];
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
                groupList.Add(groupEntity);
                groupEntity1 = groupEntity;
            }
            databaseService.CloseConnection();

            return Ok(groupEntity1);



        }


        [HttpPost("AddData")]
        public IActionResult AddGroup([FromBody] GroupEntity newGroup)
        {

            var query = "INSERT INTO Groups (Name,Group_Code,Created_at,IsPrivate) VALUES (@Group_Name,@Group_Code,@CreatedAt,@IsPrivate)";
            var parameters = new Dictionary<string, object>
                {
                    { "@Group_Name", newGroup.GroupName },
                    { "@Group_Code", newGroup.Group_Code },
                    { "@CreatedAt", DateTime.Now },
                    { "@IsPrivate", newGroup.IsPrivate },

                };
            databaseService.CloseConnection();

            return Ok(databaseService.AddData(query, parameters));
        }


        [HttpPost("CreateUserGroup")]
        public async Task<IActionResult> CreateUserGroupAsync([FromBody] GroupEntity newGroup)
        {
            if (newGroup == null || string.IsNullOrEmpty(newGroup.GroupName) || string.IsNullOrEmpty(newGroup.Group_Code) || newGroup.Group_Id <= 0)
                return BadRequest("Invalid data.");

            try
            {
                string connectionString = "Server=DEVELOPER;Database=FYP_DB;User Id=sa;Password=123456;TrustServerCertificate=True;";

                using (var connection = new SqlConnection(connectionString)) // ✅ Fixed Syntax Error
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "AddGroupWithMember"; // ✅ Use stored procedure
                        command.CommandType = CommandType.StoredProcedure;

                        // ✅ Corrected Parameter Names
                        command.Parameters.Add(new SqlParameter("@GroupName", newGroup.GroupName));
                        command.Parameters.Add(new SqlParameter("@GroupCode", newGroup.Group_Code));
                        command.Parameters.Add(new SqlParameter("@IsPrivate", newGroup.IsPrivate));
                        command.Parameters.Add(new SqlParameter("@UserID", newGroup.Group_Id)); // ✅ Fixed Wrong Parameter

                        // ✅ Execute and get Group ID
                        var result = await command.ExecuteScalarAsync();
                        int groupId = result != null ? Convert.ToInt32(result) : 0;
                        databaseService.CloseConnection();

                        return Ok(new { message = "Group and Member added successfully!", groupId });
                    }
                }
            }
            catch (Exception ex)
            {
                databaseService.CloseConnection();

                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
            }
        }




        [HttpPut("UpdateById")]
        public IActionResult UpdateGroupById([FromBody] GroupEntity groupEntity)
        {
            if (groupEntity == null)
            {
                return BadRequest("Invalid group data.");
            }

            string query = @"
        UPDATE Groups
        SET 
            Name = @GroupName,
            Group_Code = @GroupCode,
            Created_at = @CreatedAt,
            IsPrivate = @IsPrivate
        WHERE 
            Id = @GroupId";

            var parameters = new Dictionary<string, object>
    {
        { "@GroupId", groupEntity.Group_Id },
        { "@GroupName", groupEntity.GroupName },
        { "@GroupCode", groupEntity.Group_Code },
        { "@CreatedAt", DateTime.Now },
        { "@IsPrivate", groupEntity.IsPrivate }
    };

            string rowsAffected = databaseService.AddData(query, parameters);

            databaseService.CloseConnection();
            if (int.Parse(rowsAffected ) > 0)
            {
                return Ok("Group record updated successfully.");
            }
            else
            {
                return NotFound("Group record not found.");
            }
        }
        [HttpDelete("DeleteById{id}")]
        public IActionResult DeleteGroup(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid Group ID.");
            }

            string query = @"
        DELETE FROM Groups
        WHERE Id = @GroupId";

            var parameters = new Dictionary<string, object>
    {
        { "@GroupId", id }
    };

            string rowsAffected = databaseService.AddData(query, parameters);

            databaseService.CloseConnection();
            if (int.Parse(rowsAffected) > 0)
            {
                return Ok("Group record deleted successfully.");
            }
            else
            {
                return NotFound("Group record not found.");
            }
        }


    }
}
