﻿using IdentityApi.DbModels;
using IdentityApi.Interfaces;
using IdentityApi.Models;
using System.Data;

namespace IdentityApi.Providers
{
    public class UserLocationProvider : SqlProvider, IUserLocationProvider
    {
        public UserLocationProvider(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<bool> IsIPLocked(string ipAddress)
        {
            // Return amount of times the user has logged in from this location
            var table = await RunSpAsync("SP_IsIPLocked", new SpElement("IP", ipAddress, SqlDbType.VarChar));


            if (table?.Rows?.Count == 0 || table?.Rows == null)
                return false;

            // If user logged in from the location 0 times return false
            return !(Convert.ToInt32(table?.Rows[0]["UnSuccessfulIPCount"]?.ToString()) == 0);
        }

        public async Task<UserLocation> LogLocation(UserLocation userLocation)
        {
            var spElements = new SpElement[]
            {
                new SpElement("UserID", userLocation.UserID, System.Data.SqlDbType.Int),
                new SpElement("IP", userLocation.IP, System.Data.SqlDbType.VarChar),
                new SpElement("UserAgent", userLocation.UserAgent, System.Data.SqlDbType.VarChar),
                new SpElement("SuccessFul", userLocation.Successful, System.Data.SqlDbType.Bit)
            };

            // Return amount of times the user has logged in from this location
            var table = await RunSpAsync("SP_AddLocation", spElements);

            if (table?.Rows?.Count == 0 || table?.Rows == null)
                return null;

            // If user logged in from the location 0 times return false
            return DrToUserLocation(table.Rows[0]);
        }

        public async Task<bool> UserWasLoggedInFromLocation(UserLocation userLocation)
        {
            var spElements = new SpElement[]
            {
                new SpElement("UserID", userLocation.UserID, System.Data.SqlDbType.Int),
                new SpElement("IP", userLocation.IP, System.Data.SqlDbType.VarChar),
                new SpElement("UserAgent", userLocation.UserAgent, System.Data.SqlDbType.VarChar)
            };

            // Return amount of times the user has logged in from this location
            var table = await RunSpAsync("SP_UserLoggedInLocations", spElements);


            if (table?.Rows?.Count == 0 || table?.Rows == null)
                return false;

            // If user logged in from the location 0 times return false
            return !(Convert.ToInt32(table?.Rows[0]["LoggedInCount"]?.ToString()) == 0);
        }

        private UserLocation DrToUserLocation(DataRow dr)
        {
            if (dr == null)
                return null;

            UserLocation userLocation = new UserLocation();

            userLocation.ID = Convert.ToInt32(dr["ID"]);
            userLocation.UserID = Convert.ToInt32(dr["UserID"]);
            userLocation.IP = dr["IP"].ToString();
            userLocation.UserAgent = dr["UserAgent"].ToString();
            userLocation.Successful = (bool)dr["Successful"];
            userLocation.CreateDate = dr["CreateDate"] == null ? (DateTime?)dr["CreateDate"] : null;

            return userLocation;
        }
    }
}
