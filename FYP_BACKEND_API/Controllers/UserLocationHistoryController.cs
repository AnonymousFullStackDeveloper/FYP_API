using FYP_Backend.Controllers.Model;
using FYP_BACKEND_API.Controllers.DB;
using FYP_BACKEND_API.Controllers.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace FYP_BACKEND_API.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class UserLocationHistoryController : Controller
    {

        DatabaseService databaseService = new();

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {

            DataTable dataTable = databaseService.GetData("SELECT * from User_Location_History");

            List<User_Location_History> list = [];
            foreach (DataRow row in dataTable.Rows)
            {
                var newEntity = new User_Location_History()
                {
                    User_Id = Convert.ToInt32(row["User_Id"]),
                    Location_History_Id = Convert.ToInt32(row["Location_History_Id"]),
                    
                };
                list.Add(newEntity);
            }
            databaseService.CloseConnection();

            return Ok(list);

        }


        [HttpGet("GetByUserId/{id}")]
        public IActionResult GetByUserId(int id)
        {
            DataTable dataTable = databaseService.GetData("SELECT * FROM Location_History WHERE id IN(SELECT Location_History_Id FROM User_Location_History WHERE User_Id = "+id+");");

            List<LocationEntity> list = [];
            foreach (DataRow row in dataTable.Rows)
            {
                var locationEntity = new LocationEntity()
                {
                    LocationId = Convert.ToInt32(row["Id"]),
                    Name = Convert.ToString(row["Name"]),
                    Latitude = Convert.ToDecimal(row["Latitude"]),
                    Longitude = Convert.ToDecimal(row["Longitude"]),
                    Timestamp = Convert.ToString(row["Timestamp"]),

                };
                list.Add(locationEntity);
            }
            databaseService.CloseConnection();

            return Ok(list);
        }

        [HttpGet("GetByLocationId/{id}")]
        public IActionResult GetByLocationId(int id)
        {
            DataTable dataTable = databaseService.GetData("SELECT * from User_Location_History where Location_History_Id = " + id + ";");

            List<User_Location_History> list = [];
            foreach (DataRow row in dataTable.Rows)
            {
                var newEntity = new User_Location_History()
                {
                    User_Id = Convert.ToInt32(row["User_Id"]),
                    Location_History_Id = Convert.ToInt32(row["Location_History_Id"]),

                };
                list.Add(newEntity);
            }
            databaseService.CloseConnection();

            return Ok(list);
        }




        [HttpPost("AddData")]
        public IActionResult AddGroup([FromBody] User_Location_History user_Location_History)
        {
            var query = "INSERT INTO User_Location_History (User_Id,Location_History_Id) VALUES (@User_Id,@LocationId)";
            var parameters = new Dictionary<string, object>
                {
                    { "@User_Id", user_Location_History.User_Id },
                    { "@LocationId", user_Location_History.Location_History_Id },

                };
            databaseService.CloseConnection();

            return Ok(databaseService.AddData(query, parameters));
        }


        [HttpDelete("DeleteUserLocationHistory")]
        public IActionResult DeleteUserGeofence([FromBody] User_Location_History user_Location_History)
        {
            if (user_Location_History == null || user_Location_History.User_Id <= 0 || user_Location_History.Location_History_Id <= 0)
            {
                return BadRequest("Invalid User_Id.");
            }

            string query = "DELETE FROM User_Location_History WHERE User_Id = @UserId and Location_History_Id=@LocationId;";
            var parameters = new Dictionary<string, object>
                {
                    { "@UserId", user_Location_History.User_Id },
                    { "@LocationId", user_Location_History.Location_History_Id },

                };

            string rowsAffected = databaseService.AddData(query, parameters);

            databaseService.CloseConnection();
            if (int.Parse(rowsAffected) > 0)
            {
                databaseService.CloseConnection();


                return Ok("Location History deleted successfully.");
            }
            else
            {
                return NotFound($"No record found");
            }
        }
    }
}
