using IdentityApi.DbModels;
using System.Data;
using System.Data.SqlClient;

namespace IdentityApi.Providers
{
    public class SqlProvider
    {
        private readonly string _databaseConnection;

        protected SqlProvider(string databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        /// <summary>
        /// Executes stored procedure, with <paramref name="spElements"/> as sql parameters
        /// </summary>
        /// <returns>Datatable with results</returns>
        protected virtual async Task<DataTable> RunSpAsync(string spName, params SpElement[] spElements)
        {
            using (SqlConnection con = new SqlConnection(_databaseConnection))
            {
                using (SqlCommand cmd = new SqlCommand(spName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (spElements != null)
                        for (int i = 0; i < spElements.Length; i++)
                        {
                            cmd.Parameters.Add($"@{spElements[i].Key}", spElements[i].ValueType).Value = spElements[i].Value;
                        }


                    con.Open();
                    var dataReader = await cmd.ExecuteReaderAsync();
                    var dataTable = new DataTable();
                    dataTable.Load(dataReader);

                    return dataTable;
                }
            }
        }

        /// <summary>
        /// Runs sql query
        /// </summary>
        /// <returns>Datatable with results</returns>
        protected virtual async Task<DataTable> RunQueryAsync(string query)
        {
            using (SqlConnection con = new SqlConnection(_databaseConnection))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    var dataReader = await cmd.ExecuteReaderAsync();

                    var dataTable = new DataTable();
                    dataTable.Load(dataReader);

                    return dataTable;
                }
            }
        }
    }
}
