using FYP_BACKEND_API.Controllers.DB;
using FYP_BACKEND_API.Controllers.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FYP_BACKEND_API.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class GeoFenceController : Controller
    {
        private const string _cn =
       "Server=DEVELOPER;Database=FYP_DB;User Id=sa;Password=123456;TrustServerCertificate=True;";

        DatabaseService databaseService = new();

        [HttpPost("AddData")]
        public async Task<IActionResult> Post([FromBody] UserGeofence model)
        {
            using var conn = new SqlConnection("Server=DEVELOPER;Database=FYP_DB;User Id=sa;Password=123456;TrustServerCertificate=True;");
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                // 1) Insert master geofence and get its new ID
                var insertGf = @"
        INSERT INTO UserGeofence (UserId,GroupId, Latitude, Longitude, Radius, Name)
        VALUES (@UserId,@GroupId, @Latitude, @Longitude, @Radius, @Name);
        SELECT SCOPE_IDENTITY();";
                using var cmdGf = new SqlCommand(insertGf, conn, tx);
                cmdGf.Parameters.AddWithValue("@GroupId", model.GroupId);
                cmdGf.Parameters.AddWithValue("@UserId", model.UserId);
                cmdGf.Parameters.AddWithValue("@Latitude", model.Latitude);
                cmdGf.Parameters.AddWithValue("@Longitude", model.Longitude);
                cmdGf.Parameters.AddWithValue("@Radius", model.Radius);
                cmdGf.Parameters.AddWithValue("@Name", model.Name);
                var geofenceId = Convert.ToInt32(await cmdGf.ExecuteScalarAsync());

                databaseService.CloseConnection();
                // 2) Insert each friend row
                var insertFr = @"
        INSERT INTO GeofenceFriends
          (GeofenceId, FriendId, Arrive, Leave)
        VALUES
          (@GeofenceId, @FriendId, @Arrive, @Leave);";
                foreach (var f in model.Friends)
                {
                    using var cmdFr = new SqlCommand(insertFr, conn, tx);
                    cmdFr.Parameters.AddWithValue("@GeofenceId", geofenceId);
                    cmdFr.Parameters.AddWithValue("@FriendId", f.FriendId);
                    cmdFr.Parameters.AddWithValue("@Arrive", f.Arrive);
                    cmdFr.Parameters.AddWithValue("@Leave", f.Leave);
                    await cmdFr.ExecuteNonQueryAsync();
                }

                tx.Commit();
                conn.Close();
                return Ok(new { geofenceId });
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        [HttpGet("GetAllByUser/{userId}")]
        public async Task<IActionResult> GetAllByUser(int userId)
        {
            var geofences = new List<UserGeofence>();

            using var conn = new SqlConnection(_cn);
            await conn.OpenAsync();

            // 1) load geofence masters
            var cmdGf = new SqlCommand(@"
            SELECT Id, UserId, GroupId, Latitude, Longitude, Radius, Name
              FROM UserGeofence
             WHERE UserId = @UserId
            ", conn);
            cmdGf.Parameters.AddWithValue("@UserId", userId);

            databaseService.CloseConnection();
            using var rdr = await cmdGf.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                geofences.Add(new UserGeofence
                {
                    Id = rdr.GetInt32(0),
                    UserId = rdr.GetInt32(1),
                    GroupId = rdr.GetInt32(2),
                    Latitude = rdr.GetDouble(3),
                    Longitude = rdr.GetDouble(4),
                    Radius = rdr.GetDouble(5),
                    Name = rdr.GetString(6),
                    Friends = new List<GeofenceFriend>()
                });
            }
            rdr.Close();

            // 2) load friends per geofence
            foreach (var gf in geofences)
            {
                var cmdFr = new SqlCommand(@"
                SELECT Id, FriendId, Arrive, Leave
                  FROM GeofenceFriends
                 WHERE GeofenceId = @GfId
                ", conn);
                cmdFr.Parameters.AddWithValue("@GfId", gf.Id);

                using var frRdr = await cmdFr.ExecuteReaderAsync();

                databaseService.CloseConnection();
                while (await frRdr.ReadAsync())
                {
                    gf.Friends.Add(new GeofenceFriend
                    {
                        Id = frRdr.GetInt32(0),
                        GeofenceId = gf.Id,
                        FriendId = frRdr.GetInt32(1),
                        Arrive = frRdr.GetBoolean(2),
                        Leave = frRdr.GetBoolean(3)
                    });
                }
                frRdr.Close();
            }

            return Ok(geofences);
        }

        // PUT update a geofence + its friends
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserGeofence model)
        {
            using var conn = new SqlConnection(_cn);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            databaseService.CloseConnection();

            try
            {
                // 1) update master
                var updGf = @"
UPDATE UserGeofence
   SET UserId    = @UserId,
       GroupId   = @GroupId,
       Latitude  = @Latitude,
       Longitude = @Longitude,
       Radius    = @Radius,
       Name      = @Name
 WHERE Id = @Id;";
                using var cmdGf = new SqlCommand(updGf, conn, tx);
                cmdGf.Parameters.AddWithValue("@Id", id);
                cmdGf.Parameters.AddWithValue("@UserId", model.UserId);
                cmdGf.Parameters.AddWithValue("@GroupId", model.GroupId);
                cmdGf.Parameters.AddWithValue("@Latitude", model.Latitude);
                cmdGf.Parameters.AddWithValue("@Longitude", model.Longitude);
                cmdGf.Parameters.AddWithValue("@Radius", model.Radius);
                cmdGf.Parameters.AddWithValue("@Name", model.Name);
                await cmdGf.ExecuteNonQueryAsync();

                // 2) delete old friends
                var delFr = new SqlCommand(
                    "DELETE FROM GeofenceFriends WHERE GeofenceId = @Id",
                    conn, tx);
                delFr.Parameters.AddWithValue("@Id", id);
                await delFr.ExecuteNonQueryAsync();

                databaseService.CloseConnection();
                // 3) insert updated friends
                var insFr = @"
INSERT INTO GeofenceFriends
  (GeofenceId, FriendId, Arrive, Leave)
VALUES
  (@GfId, @FriendId, @Arrive, @Leave);";
                foreach (var f in model.Friends)
                {
                    using var cmdFr = new SqlCommand(insFr, conn, tx);
                    cmdFr.Parameters.AddWithValue("@GfId", id);
                    cmdFr.Parameters.AddWithValue("@FriendId", f.FriendId);
                    cmdFr.Parameters.AddWithValue("@Arrive", f.Arrive);
                    cmdFr.Parameters.AddWithValue("@Leave", f.Leave);
                    await cmdFr.ExecuteNonQueryAsync();
                }

                tx.Commit();
                return Ok(new { message = "Updated successfully." });
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // DELETE a geofence + its friends
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var conn = new SqlConnection(_cn);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                // 1) delete friends
                var delFr = new SqlCommand(
                    "DELETE FROM GeofenceFriends WHERE GeofenceId = @Id",
                    conn, tx);
                delFr.Parameters.AddWithValue("@Id", id);
                await delFr.ExecuteNonQueryAsync();

                databaseService.CloseConnection();
                // 2) delete geofence
                var delGf = new SqlCommand(
                    "DELETE FROM UserGeofence WHERE Id = @Id",
                    conn, tx);
                delGf.Parameters.AddWithValue("@Id", id);
                await delGf.ExecuteNonQueryAsync();

                tx.Commit();

                databaseService.CloseConnection();
                return Ok(new { message = "Deleted successfully." });
            }
            catch
            {
                tx.Rollback();

                databaseService.CloseConnection();
                throw;
            }
        }

        //        [HttpGet("GetAllGeofencesByUserId/{userId}")]
        //        public async Task<IActionResult> GetByUserId(int userId)
        //        {
        //            using var conn = new SqlConnection("Server=DEVELOPER;Database=FYP_DB;User Id=sa;Password=123456;TrustServerCertificate=True;");
        //            await conn.OpenAsync();

        //            try
        //            {
        //                var query = @"
        //SELECT 
        //    gf.GeofenceId,
        //    gf.GroupId,
        //    gf.Latitude,
        //    gf.Longitude,
        //    gf.Radius,
        //    gf.GeofenceName,
        //    gfF.UserId,
        //    gfF.UserArrive,
        //    gfF.UserLeave
        //FROM GroupGeofenceFriends gfF
        //JOIN GroupGeofences gf ON gf.GeofenceId = gfF.GeofenceId
        //WHERE gfF.UserId = @UserId;";

        //                using var cmd = new SqlCommand(query, conn);
        //                cmd.Parameters.AddWithValue("@UserId", userId);

        //                using var reader = await cmd.ExecuteReaderAsync();

        //                var geofences = new List<UserGeofence>();

        //                while (await reader.ReadAsync())
        //                {
        //                    geofences.Add(new UserGeofence
        //                    {
        //                        Id = reader.GetInt32(0),
        //                        GroupId = reader.GetInt32(1),
        //                        Latitude = reader.GetDouble(2),
        //                        Longitude = reader.GetDouble(3),
        //                        Radius = reader.GetDouble(4),
        //                        Name = reader.GetString(5),
        //                        Friends = new List<FriendModel>
        //                {
        //                    new FriendModel
        //                    {
        //                        UserId = reader.GetInt32(6),
        //                        Arrive = reader.GetBoolean(7),
        //                        Leave = reader.GetBoolean(8)
        //                    }
        //                }
        //                    });
        //                }

        //                return Ok(geofences);
        //            }
        //            catch (Exception ex)
        //            {
        //                return StatusCode(500, $"Internal server error: {ex.Message}");
        //            }
        //        }


        //        [HttpPut("UpdateByGeofenceId{id}")]
        //        public async Task<IActionResult> Update(int id, [FromBody] GeofenceEntiity model)
        //        {
        //            using var conn = new SqlConnection("Server=DEVELOPER;Database=FYP_DB;User Id=sa;Password=123456;TrustServerCertificate=True;");
        //            await conn.OpenAsync();
        //            using var tx = conn.BeginTransaction();

        //            try
        //            {
        //                // 1) Update main geofence data
        //                var updateGf = @"
        //UPDATE GroupGeofences
        //SET GroupId = @GroupId,
        //    Latitude = @Latitude,
        //    Longitude = @Longitude,
        //    Radius = @Radius,
        //    GeofenceName = @Name
        //WHERE GeofenceId = @GeofenceId;";

        //                using var cmdGf = new SqlCommand(updateGf, conn, tx);
        //                cmdGf.Parameters.AddWithValue("@GroupId", model.GroupId);
        //                cmdGf.Parameters.AddWithValue("@Latitude", model.Latitude);
        //                cmdGf.Parameters.AddWithValue("@Longitude", model.Longitude);
        //                cmdGf.Parameters.AddWithValue("@Radius", model.Radius);
        //                cmdGf.Parameters.AddWithValue("@Name", model.Name);
        //                cmdGf.Parameters.AddWithValue("@GeofenceId", id);
        //                await cmdGf.ExecuteNonQueryAsync();

        //                // 2) Delete existing friends
        //                var deleteFr = @"DELETE FROM GroupGeofenceFriends WHERE GeofenceId = @GeofenceId;";
        //                using var cmdDel = new SqlCommand(deleteFr, conn, tx);
        //                cmdDel.Parameters.AddWithValue("@GeofenceId", id);
        //                await cmdDel.ExecuteNonQueryAsync();

        //                // 3) Insert updated friends
        //                var insertFr = @"
        //INSERT INTO GroupGeofenceFriends (GeofenceId, UserId, UserArrive, UserLeave)
        //VALUES (@GeofenceId, @UserId, @Arrive, @Leave);";

        //                foreach (var f in model.Friends)
        //                {
        //                    using var cmdFr = new SqlCommand(insertFr, conn, tx);
        //                    cmdFr.Parameters.AddWithValue("@GeofenceId", id);
        //                    cmdFr.Parameters.AddWithValue("@UserId", f.UserId);
        //                    cmdFr.Parameters.AddWithValue("@Arrive", f.Arrive);
        //                    cmdFr.Parameters.AddWithValue("@Leave", f.Leave);
        //                    await cmdFr.ExecuteNonQueryAsync();
        //                }

        //                tx.Commit();
        //                conn.Close();
        //                return Ok(new { message = "Geofence updated successfully" });
        //            }
        //            catch (Exception ex)
        //            {
        //                tx.Rollback();
        //                return StatusCode(500, $"Error updating geofence: {ex.Message}");
        //            }
        //        }

        //        [HttpDelete("DeleteGeofence{id}")]
        //        public async Task<IActionResult> Delete(int id)
        //        {
        //            using var conn = new SqlConnection("Server=DEVELOPER;Database=FYP_DB;User Id=sa;Password=123456;TrustServerCertificate=True;");
        //            await conn.OpenAsync();
        //            using var tx = conn.BeginTransaction();

        //            try
        //            {
        //                // 1) Delete related friends first to maintain FK constraint
        //                var deleteFriends = @"DELETE FROM GroupGeofenceFriends WHERE GeofenceId = @GeofenceId;";
        //                using var cmdFr = new SqlCommand(deleteFriends, conn, tx);
        //                cmdFr.Parameters.AddWithValue("@GeofenceId", id);
        //                await cmdFr.ExecuteNonQueryAsync();

        //                // 2) Delete the geofence record
        //                var deleteGf = @"DELETE FROM GroupGeofences WHERE GeofenceId = @GeofenceId;";
        //                using var cmdGf = new SqlCommand(deleteGf, conn, tx);
        //                cmdGf.Parameters.AddWithValue("@GeofenceId", id);
        //                var rowsAffected = await cmdGf.ExecuteNonQueryAsync();

        //                if (rowsAffected == 0)
        //                {
        //                    tx.Rollback();
        //                    return NotFound("Geofence not found.");
        //                }

        //                tx.Commit();
        //                return Ok(new { message = "Geofence deleted successfully" });
        //            }
        //            catch (Exception ex)
        //            {
        //                tx.Rollback();
        //                return StatusCode(500, $"Error deleting geofence: {ex.Message}");
        //            }
        //        }










        //    [HttpGet("GetAll")]
        //    public IActionResult GetAll()
        //    {
        //        DataTable dataTable = databaseService.GetData("SELECT * from GeoFence");

        //        List<GeoFenceEntity> geoFenceList = [];
        //        foreach (DataRow row in dataTable.Rows)
        //        {
        //            var geoFenceEntity = new GeoFenceEntity()
        //            {
        //                GeoFenceId = Convert.ToInt32(row["Id"]),
        //                Name = Convert.ToString(row["Name"]),
        //                Status = Convert.ToString(row["Status"]),
        //                CreatedAt = Convert.ToString(row["Created_at"]),
        //                Radius = Convert.ToInt64(row["Radius"]),
        //                Latitude = Convert.ToDecimal(row["Latitude"]),
        //                Longitude = Convert.ToDecimal(row["Longitude"])

        //            };
        //            geoFenceList.Add(geoFenceEntity);
        //        }
        //        return Ok(geoFenceList);
        //    }


        //    [HttpPost("AddGeofence")]
        //    public IActionResult AddGeofence([FromBody] UserGroupGeofence geofence)
        //    {
        //        var query = @"
        //    INSERT INTO UserGroupGeofences 
        //    (UserId, GroupId, GeofenceName, Arrive, Leave, Latitude, Longitude, Radius, CreatedAt)
        //    VALUES 
        //    (@UserId, @GroupId, @GeofenceName, @Arrive, @Leave, @Latitude, @Longitude, @Radius, @CreatedAt)";

        //        var parameters = new Dictionary<string, object>
        //{
        //    { "@UserId", geofence.UserId },
        //    { "@GroupId", geofence.GroupId },
        //    { "@GeofenceName", geofence.GeofenceName },
        //    { "@Arrive", geofence.Arrive },
        //    { "@Leave", geofence.Leave },
        //    { "@Latitude", geofence.Latitude },
        //    { "@Longitude", geofence.Longitude },
        //    { "@Radius", geofence.Radius },
        //    { "@CreatedAt", DateTime.UtcNow }
        //};

        //        databaseService.CloseConnection(); // Optional, depending on your logic

        //        var result = databaseService.AddData(query, parameters);
        //        return Ok(result);
        //    }







        //    [HttpGet("GetById/{id}")]
        //    public IActionResult GetById(int id)
        //    {

        //        DataTable dataTable = databaseService.GetData("SELECT * from GeoFence where Id = " + id + ";");

        //        List<GeoFenceEntity> geoFenceList = [];
        //        foreach (DataRow row in dataTable.Rows)
        //        {
        //            var geoFenceEntity = new GeoFenceEntity()
        //            {
        //                GeoFenceId = Convert.ToInt32(row["Id"]),
        //                Name = Convert.ToString(row["Name"]),
        //                Status = Convert.ToString(row["Status"]),
        //                CreatedAt = Convert.ToString(row["Created_at"]),
        //                Radius = Convert.ToInt64(row["Radius"]),
        //                Latitude = Convert.ToDecimal(row["Latitude"]),
        //                Longitude = Convert.ToDecimal(row["Longitude"])

        //            };
        //            geoFenceList.Add(geoFenceEntity);
        //        }
        //        return Ok(geoFenceList);
        //    }



        //    [HttpPost("AddData")]
        //    public IActionResult AddGroup([FromBody] GeoFenceEntity geofenceEntity)
        //    {

        //        var query = "INSERT INTO GeoFence (Name,Radius,Status,Latitude,Longitude) VALUES (@Name,@Radius,@Status,@Latitude,@Longitude)";
        //        var parameters = new Dictionary<string, object>
        //            {
        //                { "@Name", geofenceEntity.Name },
        //                { "@Radius", geofenceEntity.Radius },
        //                { "@Status", geofenceEntity.Status },
        //                { "@Latitude", geofenceEntity.Latitude },
        //                { "@Longitude", geofenceEntity.Longitude },


        //            };
        //        return Ok(databaseService.AddData(query, parameters)+" Data has Been ADDED");
        //    }


        //    [HttpPut("UpdateById")]
        //    public IActionResult UpdateGeoFence([FromBody] GeoFenceEntity geoFenceEntity)
        //    {
        //        if (geoFenceEntity == null)
        //        {
        //            return BadRequest("Invalid GeoFence data.");
        //        }

        //        string query = @"
        //            UPDATE GeoFence
        //            SET 
        //                Name = @Name,
        //                Radius = @Radius,
        //                Created_at = @CreatedAt,
        //                Status = @Status,
        //                Latitude = @Latitude,
        //                Longitude = @Longitude
        //            WHERE 
        //                Id = @GeoFenceId";

        //        var parameters = new Dictionary<string, object>
        //{
        //    { "@GeoFenceId", geoFenceEntity.GeoFenceId },
        //    { "@Name", geoFenceEntity.Name },
        //    { "@Status", geoFenceEntity.Status },
        //    { "@CreatedAt", DateTime.Now },
        //    { "@Radius", geoFenceEntity.Radius },
        //    { "@Latitude", geoFenceEntity.Latitude },
        //    { "@Longitude", geoFenceEntity.Longitude }
        //};

        //        string rowsAffected = databaseService.AddData(query, parameters);

        //        if (int.Parse(rowsAffected) > 0)
        //        {
        //            return Ok("GeoFence record updated successfully.");
        //        }
        //        else
        //        {
        //            return NotFound("GeoFence record not found.");
        //        }
        //    }
        //    [HttpDelete("DeleteByd{id}")]
        //    public IActionResult DeleteById(int id)
        //    {
        //        if (id <= 0)
        //        {
        //            return BadRequest("Invalid GeoFence ID.");
        //        }

        //                    string query = @"
        //                DELETE FROM GeoFence
        //                WHERE Id = @GeoFenceId";

        //                    var parameters = new Dictionary<string, object>
        //            {
        //                { "@GeoFenceId", id }
        //            };

        //        string rowsAffected = databaseService.AddData(query, parameters);

        //        if (int.Parse(rowsAffected) > 0)
        //        {
        //            return Ok("GeoFence record deleted successfully.");
        //        }
        //        else
        //        {
        //            return NotFound("GeoFence record not found.");
        //        }
        //    }

    }
}
