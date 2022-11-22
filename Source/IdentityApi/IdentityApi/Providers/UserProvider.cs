﻿using IdentityApi.Interfaces;
using IdentityApi.Models;
using System.Data.SqlClient;
using System.Data;
using IdentityApi.DbModels;
using System.Data.Common;

namespace IdentityApi.Providers
{
    public class UserProvider : SqlProvider, IUserProvider
    {
        public UserProvider(IConfiguration configuration) : base(configuration)
        {

        }

        /// <inheritdoc/>
        public async Task<DbUser> CreateUserAsync(DbUser userCreate)
        {
            try
            {
                var spElements = new SpElement[]
                {
                    new SpElement("Email", userCreate.Email, SqlDbType.VarChar),
                    new SpElement("HashedPassword", userCreate.HashedPassword, SqlDbType.VarChar),
                    new SpElement("Salt", userCreate.Salt, SqlDbType.VarChar),
                };

                var userTable = await RunSpAsync("SP_CreateUser", spElements);

                if (userTable?.Rows?.Count == 0 || userTable?.Rows == null)
                    return null;

                return DRToUser(userTable.Rows[0]);
            }
            catch (Exception e)
            {
                // TODO: log here

                throw e; // TODO: Change to better error
            }
        }

        /// <inheritdoc/>
        public async Task<DbUser> GetUserByIDAsync(int userID)
        {
            try
            {
                var userTable = await RunQueryAsync($"select top(1)* from Users  where ID = {userID}");

                if (userTable?.Rows?.Count == 0 || userTable?.Rows == null)
                    return null;

                return DRToUser(userTable.Rows[0]);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        /// <inheritdoc/>
        public async Task<DbUser> GetUserByEmailAsync(string userEmail)
        {
            try
            {
                var userTable = await RunSpAsync("SP_CreateUser", new SpElement("Email", userEmail, SqlDbType.VarChar));

                if (userTable?.Rows?.Count == 0 || userTable?.Rows == null)
                    return null;

                return DRToUser(userTable.Rows[0]);
            }
            catch (Exception e)
            {
                // TODO: log here

                throw e; // TODO: Change to better error
            }
        }

        public async Task<DbUser> UpdateUserFailedTries(int userID)
        {
            try
            {
                var userTable = await RunSpAsync("SP_UpdateUserFailedTries", new SpElement("UserID", userID, SqlDbType.Int));

                if (userTable?.Rows?.Count == 0 || userTable?.Rows == null)
                    return null;

                return DRToUser(userTable.Rows[0]);
            }
            catch (Exception e)
            {
                // TODO: log here

                throw e; // TODO: Change to better error
            }
        }


        public async Task<DbUser> UpdateUserLoginSuccess(int userID)
        {
            try
            {
                var userTable = await RunSpAsync("SP_UserLoggedIn", new SpElement("UserID", userID, SqlDbType.Int));

                if (userTable?.Rows?.Count == 0 || userTable?.Rows == null)
                    return null;

                return DRToUser(userTable.Rows[0]);
            }
            catch (Exception e)
            {
                // TODO: log here

                throw e; // TODO: Change to better error
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

                if (dr.Table.Columns.Contains("HashedPassword"))
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
