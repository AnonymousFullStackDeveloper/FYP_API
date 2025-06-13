using FYP_Backend.Controllers.Model;
using FYP_BACKEND_API.Controllers.DB;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace FYP_BACKEND_API.Controllers
{

    [ApiController]
    [Route("/[controller]")]
    public class LocationController : Controller
    {

        DatabaseService databaseService = new();

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {

            DataTable dataTable = databaseService.GetData("SELECT * from Location_History");

            List<LocationEntity> locationList = [];
            foreach (DataRow row in dataTable.Rows)
            {
                var locationEntity = new LocationEntity()
                {
                    LocationId = Convert.ToInt32(row["Id"]),
                    UserId = Convert.ToInt32(row["UserId"]),
                    Name = Convert.ToString(row["Name"]),
                    Latitude = Convert.ToDecimal(row["Latitude"]),
                    Longitude = Convert.ToDecimal(row["Longitude"]),
                    Timestamp = Convert.ToString(row["Timestamp"]),

                };
                locationList.Add(locationEntity);
            }
            databaseService.CloseConnection();

            return Ok(locationList);

        }




        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            DataTable dataTable = databaseService.GetData("SELECT * from Location_History where Id = " + id + ";");

            List<LocationEntity> locationList = [];
            foreach (DataRow row in dataTable.Rows)
            {
                var locationEntity = new LocationEntity()
                {
                    UserId = Convert.ToInt32(row["UserId"]),

                    LocationId = Convert.ToInt32(row["Id"]),
                    Name = Convert.ToString(row["Name"]),
                    Latitude = Convert.ToInt64(row["Latitude"]),
                    Longitude = Convert.ToInt64(row["Longitude"]),
                    Timestamp = Convert.ToString(row["Timestamp"]),

                };
                locationList.Add(locationEntity);
            }


            databaseService.CloseConnection();
            return Ok(locationList);
        }


        [HttpGet("GetByUserId/{id}")]
        public IActionResult GetByUserId(int id)
        {
            DataTable dataTable = databaseService.GetData("SELECT * from Location_History where UserId = " + id + ";");

            List<LocationEntity> locationList = [];
            foreach (DataRow row in dataTable.Rows)
            {
                var locationEntity = new LocationEntity()
                {
                    UserId = Convert.ToInt32(row["UserId"]),
                    LocationId = Convert.ToInt32(row["Id"]),
                    Name = Convert.ToString(row["Name"]),
                    Latitude = Convert.ToDecimal(row["Latitude"]),
                    Longitude = Convert.ToDecimal(row["Longitude"]),
                    Timestamp = Convert.ToString(row["Timestamp"]),

                };
                locationList.Add(locationEntity);
            }
            databaseService.CloseConnection();

            return Ok(locationList);
        }




        [HttpPost("AddData")]
        public IActionResult AddGroup([FromBody] LocationEntity locationEntity)
        {
            var query = "INSERT INTO Location_History (UserId,Name,Latitude,Longitude,Timestamp) VALUES (@UserID,@Name,@Latitude,@Longitude,@Timestamp)";
            var parameters = new Dictionary<string, object>
                {
                    {"@UserId",locationEntity.UserId },
                    { "@Name", locationEntity.Name },
                    { "@Latitude", locationEntity.Latitude },
                    { "@Longitude", locationEntity.Longitude },
                    { "@Timestamp", DateTime.Now },

                };
            databaseService.CloseConnection();

            return Ok(databaseService.AddData(query, parameters));
        }

        [HttpPut("{id}")]
        public IActionResult UpdateLocationHistory(int id, [FromBody] LocationEntity locationEntity)
        {
            if (locationEntity == null)
            {
                return BadRequest("Invalid location data.");
            }

            string query = @"
        UPDATE Location_History
        SET 
            Name = @Name,
            Latitude = @Latitude,
            Longitude = @Longitude,
            Timestamp = @Timestamp
        WHERE 
            Id = @Id";

            var parameters = new Dictionary<string, object>
    {
        { "@Id", id },
        { "@Name", locationEntity.Name },
        { "@Latitude", locationEntity.Latitude },
        { "@Longitude", locationEntity.Longitude },
        { "@Timestamp", DateTime.Now }
    };

            try
            {
                string rowsAffected = databaseService.AddData(query, parameters);

                databaseService.CloseConnection();
                if (int.Parse(rowsAffected) > 0)
                {
                    databaseService.CloseConnection();

                    return Ok("Location history updated successfully.");
                }
                else
                {
                    databaseService.CloseConnection();

                    return NotFound("Location record not found.");
                }
            }
            catch (Exception ex)
            {
                databaseService.CloseConnection();

                // Log the exception
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("DeleteById{id}")]
        public IActionResult DeleteLocationHistory(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid location ID.");
            }

            string query = @"
        DELETE FROM Location_History
        WHERE Id = @Id";

            var parameters = new Dictionary<string, object>
    {
        { "@Id", id }
    };

            try
            {
                string rowsAffected = databaseService.AddData(query, parameters);

                databaseService.CloseConnection();
                if (int.Parse(rowsAffected) > 0)
                {
                    databaseService.CloseConnection();

                    return Ok("Location history deleted successfully.");
                }
                else
                {
                    databaseService.CloseConnection();

                    return NotFound("Location record not found.");
                }
            }
            catch (Exception ex)
            {
                databaseService.CloseConnection();

                // Log the exception
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



    }
}
