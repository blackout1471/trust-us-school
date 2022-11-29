using IdentityApi.Interfaces;
using System.Data;
using IdentityApi.DbModels;

namespace IdentityApi.Providers
{
    public class UserProvider : SqlProvider, IUserProvider
    {
        public UserProvider(IConfiguration configuration) 
            : base(configuration.GetConnectionString("SQLserver"))
        {
        }

        /// <inheritdoc/>
        public async Task<DbUser> CreateUserAsync(DbUser userCreate)
        {
                var spElements = new SpElement[]
                {
                    new SpElement("Email", userCreate.Email, SqlDbType.VarChar),
                    new SpElement("HashedPassword", userCreate.HashedPassword, SqlDbType.VarChar),
                    new SpElement("Salt", userCreate.Salt, SqlDbType.VarChar),
                    new SpElement("FirstName", userCreate.FirstName, SqlDbType.VarChar),
                    new SpElement("LastName", userCreate.LastName, SqlDbType.VarChar),
                    new SpElement("PhoneNumber", userCreate.PhoneNumber, SqlDbType.VarChar),
                    new SpElement("SecretKey", userCreate.SecretKey, SqlDbType.VarChar),
                    new SpElement("Counter", userCreate.Counter, SqlDbType.BigInt)
                };

            var userTable = await RunSpAsync("SP_CreateUser", spElements);

            if (userTable?.Rows?.Count == 0 || userTable?.Rows == null)
                return null;

            return DRToUser(userTable.Rows[0]);
        }

        /// <inheritdoc/>
        public async Task<DbUser> GetUserByIDAsync(int userID)
        {
            var userTable = await RunQueryAsync($"select top(1)* from Users  where ID = {userID}");

            if (userTable?.Rows?.Count == 0 || userTable?.Rows == null)
                return null;

            return DRToUser(userTable.Rows[0]);
        }

        /// <inheritdoc/>
        public async Task<DbUser> GetUserByEmailAsync(string userEmail)
        {
            var userTable = await RunSpAsync("SP_UserExists", new SpElement("Email", userEmail, SqlDbType.VarChar));

            if (userTable?.Rows?.Count == 0 || userTable?.Rows == null)
                return null;

            return DRToUser(userTable.Rows[0]);
        }

        /// <inheritdoc/>
        public async Task<DbUser> UpdateUserFailedTries(int userID)
        {
            var userTable = await RunSpAsync("SP_UpdateUserFailedTries", new SpElement("UserID", userID, SqlDbType.Int));

            if (userTable?.Rows?.Count == 0 || userTable?.Rows == null)
                return null;

            return DRToUser(userTable.Rows[0]);
        }

        /// <inheritdoc/>
        public async Task<DbUser> UpdateUserLoginSuccess(int userID)
        {
            var userTable = await RunSpAsync("SP_UserLoggedIn", new SpElement("UserID", userID, SqlDbType.Int));

            if (userTable?.Rows?.Count == 0 || userTable?.Rows == null)
                return null;

            return DRToUser(userTable.Rows[0]);
        }

        /// <inheritdoc/>
        public async Task<DbUser> UpdateUserLoginNewLocation(int userID)
        {
            var userTable = await RunSpAsync("SP_UpdateLastRequest", new SpElement("UserID", userID, SqlDbType.Int));

            if (userTable?.Rows?.Count == 0 || userTable?.Rows == null)
                return null;

            return DRToUser(userTable.Rows[0]);
        }

        /// <inheritdoc />
        public async Task UpdateUserToVerifiedAsync(int userID)
        {
            await RunSpAsync("SP_UserRegistrationVerified", new SpElement("UserID", userID, SqlDbType.Int));
        }

        /// <summary>
        /// Maps datarow to db user
        /// </summary>
        private DbUser DRToUser(DataRow dr)
        {
            if (dr == null)
                return null;

            var dbUser = new DbUser();

            dbUser.Email = dr["Email"].ToString();
            dbUser.ID = Convert.ToInt32(dr["ID"]);
            dbUser.FirstName = dr["FirstName"].ToString();
            dbUser.LastName = dr["LastName"].ToString();
            dbUser.PhoneNumber = dr["PhoneNumber"].ToString();
            dbUser.IsVerified = (bool)dr["IsVerified"];
            dbUser.IsLocked = (bool)dr["IsLocked"];
            dbUser.LockedDate = !string.IsNullOrEmpty(dr["LockedDate"].ToString()) ? (DateTime?)dr["LockedDate"] : null;
            dbUser.FailedTries = Convert.ToInt32(dr["FailedTries"]);

            if (dr.Table.Columns.Contains("HashedPassword"))
                dbUser.HashedPassword = dr["HashedPassword"]?.ToString();


            if (dr.Table.Columns.Contains("Salt"))
                dbUser.Salt = dr["Salt"]?.ToString();

            if (dr.Table.Columns.Contains("SecretKey"))
                dbUser.SecretKey = dr["SecretKey"]?.ToString();

            if (dr.Table.Columns.Contains("Counter"))
                dbUser.Counter = Convert.ToInt64(dr["Counter"]);

            if (dr.Table.Columns.Contains("LastRequestDate"))
                dbUser.LastRequestDate = !string.IsNullOrEmpty(dr["LastRequestDate"].ToString()) ? (DateTime?)dr["LastRequestDate"] : null;
            return dbUser;
        }

        /// <inheritdoc/>
        public async Task<DbUser> UpdateUserLoginSuccessWithVerificationCode(int userID)
        {
            var userTable = await RunSpAsync("SP_UserLoggedInWithVerification", new SpElement("UserID", userID, SqlDbType.Int));

            if (userTable?.Rows?.Count == 0 || userTable?.Rows == null)
                return null;

            return DRToUser(userTable.Rows[0]);
        }
    }
}
