using IdentityApi.DbModels;
using System.Data;
using System.Data.SqlClient;

namespace IdentityApi.Providers
{
    public class SqlProvider
    {
        protected readonly IConfiguration _configuration;

        protected SqlProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        /// <summary>
        /// Executes stored procedure, with <paramref name="spElements"/> as sql parameters
        /// </summary>
        /// <returns>Datatable with results</returns>
        protected virtual async Task<DataTable> RunSpAsync(string spName, params SpElement[] spElements)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("SQLserver")))
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
            catch (Exception e)
            {
                // TODO: log here

                throw e; // TODO: Change to better error
            }
        }

        /// <summary>
        /// Runs sql query
        /// </summary>
        /// <returns>Datatable with results</returns>
        protected virtual async Task<DataTable> RunQueryAsync(string query)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("SQLserver")))
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
            catch (Exception e)
            {
                // TODO: log here

                throw e; // TODO: Change to better error
            }
        }
    }
}
