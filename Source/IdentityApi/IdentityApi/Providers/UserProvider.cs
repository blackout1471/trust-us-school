using IdentityApi.Interfaces;
using IdentityApi.Models;
using System.Data.SqlClient;
using System.Data;
using IdentityApi.DbModels;
using System.Data.Common;

namespace IdentityApi.Providers
{
    public class UserProvider : IUserProvider
    {
        private readonly IConfiguration _configuration;

        public UserProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<DbUser> CreateUserAsync(DbUser userCreate)
        {
            try
            {
                // TODO: Make this usings into a nice method
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("SQLserver")))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_CreateUser", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@Email", SqlDbType.VarChar).Value = userCreate.Email;
                        cmd.Parameters.Add("@HashedPassword", SqlDbType.VarChar).Value = userCreate.HashedPassword;
                        cmd.Parameters.Add("@Salt", SqlDbType.VarChar).Value = userCreate.Salt;

                        con.Open();
                        var dataReader = await cmd.ExecuteReaderAsync();

                        var dataTable = new DataTable();
                        dataTable.Load(dataReader);

                        if (dataTable.Rows.Count == 0)
                            return null;

                        return DRToUser(dataTable.Rows[0]);
                    }
                }
            }
            catch (Exception e)
            {
                // TODO: log here

                throw e; // TODO: Change to better error
            }
        }

        /// <inheritdoc/>
        public async Task<DbUser> GetUserByIDAsync(int id)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("SQLserver")))
            {
                // no need for sp since input is an integer therefore sql injection is not possible
                using (SqlCommand cmd = new SqlCommand($"select top(1)* from Users  where ID = {id}", con))
                {
                    con.Open();
                    var dataReader = await cmd.ExecuteReaderAsync();

                    var dataTable = new DataTable();
                    dataTable.Load(dataReader);

                    if (dataTable.Rows.Count == 0)
                        return null;

                    return DRToUser(dataTable.Rows[0]);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<DbUser> GetUserByEmailAsync(string userEmail)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("SQLserver")))
            {
                using (SqlCommand cmd = new SqlCommand("SP_UserExists", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Email", SqlDbType.VarChar).Value = userEmail;

                    con.Open();
                    var dataReader = await cmd.ExecuteReaderAsync();

                    var dataTable = new DataTable();
                    dataTable.Load(dataReader);

                    if (dataTable.Rows.Count == 0)
                        return null;

                    return DRToUser(dataTable.Rows[0]);
                }
            }
        }

        /// <summary>
        /// Maps datarow to db user
        /// </summary>
        private DbUser DRToUser(DataRow dr)
        {
            try
            {
                if (dr == null)
                    return null;

                var dbUser = new DbUser();

                dbUser.Email = dr["Email"].ToString();
                dbUser.ID = Convert.ToInt32(dr["ID"]);
                dbUser.FirstName = dr["FirstName"].ToString();
                dbUser.LastName = dr["LastName"].ToString();
                dbUser.PhoneNumber = dr["PhoneNumber"].ToString();
                dbUser.IsVerified = (bool)dr["IsLocked"];
                dbUser.IsLocked = (bool)dr["IsLocked"];
                dbUser.LockedDate = dr["LockedDate"] == null ? (DateTime?)dr["LockedDate"] : null;
                dbUser.FailedTries = Convert.ToInt32(dr["FailedTries"]);

                if(dr.Table.Columns.Contains("HashedPassword"))
                    dbUser.HashedPassword = dr["HashedPassword"]?.ToString();

                if (dr.Table.Columns.Contains("Salt"))
                    dbUser.Salt = dr["Salt"]?.ToString();
                return dbUser;
            }
            catch (Exception e)
            {
                // TODO: Add log and replace throw

                throw e;
            }
        }

    }
}
