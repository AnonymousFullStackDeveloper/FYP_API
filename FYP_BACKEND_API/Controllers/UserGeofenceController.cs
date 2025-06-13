//using FYP_BACKEND_API.Controllers.DB;
//using FYP_BACKEND_API.Controllers.Model;
//using Microsoft.AspNetCore.Mvc;
//using System.Data;

//namespace FYP_BACKEND_API.Controllers
//{

//    [ApiController]
//    [Route("/[controller]")]
//    public class UserGeofenceController : Controller
//    {

//        DatabaseService databaseService = new DatabaseService();

//        [HttpGet("GetAll")]
//        public IActionResult GetAll()
//        {
//            DataTable dataTable = databaseService.GetData("SELECT * from UserGeofence");

//            List<UserGeofences> userGeofenceList = [];
//            foreach (DataRow row in dataTable.Rows)
//            {
//                var userGeofenceModel = new UserGeofences()
//                {
//                    User_Id = Convert.ToInt32(row["User_Id"]),
//                    Geofence_Id = Convert.ToInt32(row["GeoFence_Id"]),
                    
//                };
//                userGeofenceList.Add(userGeofenceModel);
//            }
//            return Ok(userGeofenceList);
//        }


//        [HttpGet("GetByUserId/{id}")]
//        public IActionResult GetByUserId(int id)
//        {
//            DataTable dataTable = databaseService.GetData("SELECT * from UserGeofence where User_Id = "+id+";");

//            List<UserGeofences> userGeofenceList = [];
//            foreach (DataRow row in dataTable.Rows)
//            {
//                var userGeofenceModel = new UserGeofences()
//                {
//                    User_Id = Convert.ToInt32(row["User_Id"]),
//                    Geofence_Id = Convert.ToInt32(row["GeoFence_Id"]),

//                };
//                userGeofenceList.Add(userGeofenceModel);
//            }
//            return Ok(userGeofenceList);
//        }

//        [HttpGet("GetByGeofenceId/{id}")]
//        public IActionResult GetByGeofenceId(int id)
//        {
//            DataTable dataTable = databaseService.GetData("SELECT * from UserGeofence where GeoFence_Id = " + id + ";");

//            List<UserGeofences> userGeofenceList = [];
//            foreach (DataRow row in dataTable.Rows)
//            {
//                var userGeofenceModel = new UserGeofences()
//                {
//                    User_Id = Convert.ToInt32(row["User_Id"]),
//                    Geofence_Id = Convert.ToInt32(row["GeoFence_Id"]),

//                };
//                userGeofenceList.Add(userGeofenceModel);
//            }
//            return Ok(userGeofenceList);
//        }



//        [HttpPost("AddUserGeofence")]
//        public IActionResult AddUserGeofence([FromBody] UserGeofences userGeofence)
//        {
//            if (userGeofence == null || userGeofence.User_Id <= 0 || userGeofence.Geofence_Id <= 0)
//            {
//                return BadRequest("Invalid request payload.");
//            }

//            string query = "INSERT INTO UserGeofence (User_Id, GeoFence_Id) VALUES (@UserId, @GeofenceId);";
//            var parameters = new Dictionary<string, object>
//                {
//                    { "@UserId", userGeofence.User_Id },
//                    { "@GeofenceId", userGeofence.Geofence_Id }
//                };

//            string rowsAffected = databaseService.AddData(query, parameters);

//            if (int.Parse(rowsAffected) > 0)
//            {
//                return Ok("Geofence added successfully.");
//            }
//            else
//            {
//                return StatusCode(500, "Failed to add geofence.");
//            }
//        }

//        [HttpDelete("DeleteUserGeofence")]
//        public IActionResult DeleteUserGeofence([FromBody] UserGeofences userGeofences)
//        {
//            if (userGeofences == null|| userGeofences.User_Id <= 0 || userGeofences.Geofence_Id <= 0)
//            {
//                return BadRequest("Invalid User_Id.");
//            }

//            string query = "DELETE FROM UserGeofence WHERE User_Id = @UserId and Geofence_Id=@GeofenceId;";
//            var parameters = new Dictionary<string, object>
//                {
//                    { "@UserId", userGeofences.User_Id },
//                    { "@GeofenceId", userGeofences.Geofence_Id },

//                };

//            string  rowsAffected = databaseService.AddData(query, parameters);

//            if (int.Parse(rowsAffected) > 0)
//            {
//                return Ok("Geofence deleted successfully.");
//            }
//            else
//            {
//                return NotFound($"No record found");
//            }
//        }



//    }
//}
