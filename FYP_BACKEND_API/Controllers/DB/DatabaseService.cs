using Microsoft.Data.SqlClient;
using System.Data;

namespace FYP_BACKEND_API.Controllers.DB
{
    public class DatabaseService
    {

        //private readonly string _connectionString = "Server=KAMRAN;Database=FYP_1_TEST;User Id=sa;Password=123456;TrustServerCertificate=True;";
        private readonly static SqlConnection _connection = new("Server=DEVELOPER;Database=FYP_DB;User Id=sa;Password=123456;TrustServerCertificate=True;");


        //public DatabaseService()
        //{
        //    _connection = new SqlConnection(_connectionString);
        //}

        public void OpenConnection()
        {
            if (_connection.State == ConnectionState.Closed || _connection.State == ConnectionState.Broken)
            {
                _connection.Open();
            }
        }

        public void CloseConnection()
        {
            if (_connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
        }

        public DataTable GetData(string query)
        {
            DataTable dataTable = new();


            try
            {
                _connection.Open();
                SqlDataAdapter sqlDataAdapter = new(query, _connection);
                sqlDataAdapter.Fill(dataTable);
                _connection.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Database Service While Running GetData Method : " + ex.ToString());
            }

            return dataTable;

        }


        public DataTable GetData1(string query, Dictionary<string, object>? parameters = null)
        {
            DataTable dataTable = new();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connection.ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // ✅ Add parameters if provided
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataAdapter sqlDataAdapter = new(command))
                        {
                            sqlDataAdapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in Database Service (GetData): " + ex.Message);
            }

            return dataTable;
        }


        public String AddData(string query, IDictionary<string, object> parameters)
        {
            _connection.Open();
            String ret;
            using (var command = new SqlCommand(query, _connection))
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
                ret = command.ExecuteNonQuery().ToString();
            }
            _connection.Close();
            //if (ret == null)
            //{
            //    return ret;
            //}


            return ret;
        }

        public int AddData2(string query, SqlParameter[] parameters)
        {
            try
            {
                _connection.Open();

                using (var command = new SqlCommand(query, _connection))
                {
                    // Add parameters to the SQL command
                    command.Parameters.AddRange(parameters);

                    // Execute the query and return the number of rows affected
                    int result = command.ExecuteNonQuery();

                    return result;  // return the number of rows affected
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle as needed
                throw new Exception("Database error: " + ex.Message);
            }
            finally
            {
                _connection.Close();  // Ensure the connection is closed
            }
        }


        public bool AddData1(string query, IDictionary<string, object> parameters)
        {
            _connection.Open();
            String ret;
            bool b = false;
            using (var command = new SqlCommand(query, _connection))
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
                ret = command.ExecuteNonQuery().ToString();
                b = true;

            }
            _connection.Close();
            
            return b;
        }

        public object? GetScalarValue(string query, Dictionary<string, object> parameters)
        {
            using (var cmd = new SqlCommand(query, _connection))
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }

                var result = cmd.ExecuteScalar();
                return result != DBNull.Value ? result : null;
            }
        }


        public Object Update(string query, IDictionary<string, object> parameters)
        {
            var c = AddData(query, parameters); // Reuse the Add logic for parameterized commands
            return c;
        }

        public Object DeleteAsync(string query, IDictionary<string, object> parameters)
        {
            var d = AddData(query, parameters); // Reuse the Add logic for delete
            return d;
        }

        public void Dispose()
        {
            CloseConnection();
            _connection.Dispose();
        }
    }
}
