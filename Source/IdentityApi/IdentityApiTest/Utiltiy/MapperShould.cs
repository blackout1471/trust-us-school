using IdentityApi.DbModels;
using IdentityApi.Models;
using Mapster;
using Microsoft.OpenApi.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityApiUnitTest.Utiltiy
{
    public class MapperShould
    {
        [Fact]
        public void GetUserInfo_WhenMapped_DbUserToUser()
        {
            // arrange
            var expected = GetDbUser();

            // act
            var actual = expected.Adapt<User>();

            // assert
            Assert.Equal(expected.ID, actual.ID);
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Equal(expected.IsLocked, actual.IsLocked);
            Assert.Equal(expected.IsVerified, actual.IsVerified);
            Assert.Equal(expected.FailedTries, actual.FailedTries);
            Assert.Equal(expected.PhoneNumber, actual.PhoneNumber);
            Assert.Equal(expected.LockedDate, actual.LockedDate);
        }

        [Fact]
        public void GetUserInfo_WhenMapped_UserToDbUser()
        {
            // arrange
            var expected = GetUser();

            // act
            var actual = expected.Adapt<DbUser>();

            // assert
            Assert.Equal(expected.ID, actual.ID);
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Equal(expected.IsLocked, actual.IsLocked);
            Assert.Equal(expected.IsVerified, actual.IsVerified);
            Assert.Equal(expected.FailedTries, actual.FailedTries);
            Assert.Equal(expected.PhoneNumber, actual.PhoneNumber);
            Assert.Equal(expected.LockedDate, actual.LockedDate);
            Assert.Null(actual.HashedPassword);
            Assert.Null(actual.Salt);            
        }

        [Fact]
        public void GetUserInfo_WhenMapped_UserCreateToUser()
        {
            // arrange
            var expected = GetUserCreate();

            // act
            var actual = expected.Adapt<User>();

            // assert
            Assert.Equal(0, actual.ID);
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.False(actual.IsLocked);
            Assert.False(actual.IsVerified);
            Assert.Equal(0, actual.FailedTries);
            Assert.Equal(expected.PhoneNumber, actual.PhoneNumber);
            Assert.Null(actual.LockedDate);
        }

        [Fact]
        public void GetUserInfo_WhenMapped_UserBaseToUserCreate()
        {
            // arrange
            var expected = GetUserBase();

            // act
            var actual = expected.Adapt<UserCreate>();

            // assert
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Equal(expected.PhoneNumber, actual.PhoneNumber);
            Assert.Null(actual.Password);
        }

        private DbUser GetDbUser()
        {
            return new DbUser()
            {
                ID = 1,
                Email = "a@b.com",
                FirstName = "f",
                LastName= "l",
                IsLocked = true,
                IsVerified = false,
                FailedTries = 2,
                LockedDate = DateTime.Parse("2022/11/24"),
                PhoneNumber = "123",
                HashedPassword = "123",
                Salt = "123"
            };
        }

        private User GetUser()
        {
            return new User()
            {
                ID = 1,
                Email = "a@b.com",
                FirstName = "f",
                LastName = "l",
                IsLocked = true,
                IsVerified = false,
                FailedTries = 2,
                LockedDate = DateTime.Parse("2022/11/24"),
                PhoneNumber = "123"
            };
        }

        private UserCreate GetUserCreate()
        {
            return new UserCreate()
            {
                Email = "a@b.com",
                FirstName = "f",
                LastName = "l",
                PhoneNumber = "123",
                Password = "123"
            };
        }

        private UserBase GetUserBase()
        {
            return new UserBase()
            {
                Email = "a@b.com",
                FirstName = "f",
                LastName = "l",
                PhoneNumber = "123"
            };
        } 
    }
}
