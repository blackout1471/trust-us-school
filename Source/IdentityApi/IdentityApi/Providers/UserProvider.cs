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

        public async Task<DbUser> CreateUser(DbUser userCreate)
        {
            try
            {
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

        public async Task<DbUser> GetUserByID(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<DbUser> GetUserByEmail(string userEmail)
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

                    return DRToUser(dataTable.Rows[0]);
                }
            }
        }

        private DbUser DRToUser(DataRow dr)
        {
            try
            {
                if (dr == null)
                    return null;

                var dbUser = new DbUser();

                dbUser.Email = dr["Email"].ToString();
                dbUser.ID = Convert.ToInt32(dr["ID"]);
                dbUser.HashedPassword = dr["HashedPassword"].ToString();
                dbUser.Salt = dr["Salt"].ToString();

                // TODO: Add the rest of the information

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
