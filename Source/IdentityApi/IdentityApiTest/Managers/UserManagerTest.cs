using FakeItEasy;
using IdentityApi.DbModels;
using IdentityApi.Interfaces;
using IdentityApi.Managers;
using IdentityApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IdentityApiTest.Managers
{
    public class UserManagerTest
    {
        private User GetUser()
        {
            return new User()
            {
                ID = 1,
                Email = "a@b.com",
                FirstName = "jon",
                LastName = "stevensen",
                PhoneNumber = "13246578"
            };
        }

        private DbUser GetDbUser()
        {
            return new DbUser()
            {
                ID = 1,
                Email = "a@b.com",
                HashedPassword = "C123469DB6AEE601787C5974D80A7D7223486F1523B00803E82EF60CD5EB27918C1CA9F77AE8B61E73CF944CE82B469491CEEDEDFE89C936DAE3220F6C7A78B9",
                Salt = "CVkTFhIFerV0uxOMbZ0fFlE4HFVrwOs2PW5kkPEvzSoFcVJkNP",
                FirstName = "jon",
                LastName = "stevensen",
                PhoneNumber = "13246578"
            };
        }


        private UserLogin GetUserLogin()
        {
            return new UserLogin()
            {
                Email = "a@b.com",
                Password = "tron"
            };
        }

        private UserCreate GetUserCreate()
        {
            return new UserCreate()
            {
                Email = "a@b.com",
                Password = "abc",
                FirstName = "jon",
                LastName = "stevensen",
                PhoneNumber = "13246578"
            };
        }

        [Fact]
        public void User_Should_Login()
        {
            var expected = GetUser();
            var userLogin = GetUserLogin();

            var userProvider = A.Fake<IUserProvider>();
            A.CallTo(() => userProvider.GetUserByEmail(userLogin.Email)).Returns(GetDbUser());

            var userManager = new UserManager(userProvider);
            var actual =  userManager.Login(userLogin).Result;

            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.ID, actual.ID);
        }

        [Fact]
        public void User_Should_ThrowOnLogin()
        {
            var expected = GetUser();
            var userLogin = GetUserLogin();
            userLogin.Password = "PasswordThatIsWrong";

            var userProvider = A.Fake<IUserProvider>();
            A.CallTo(() => userProvider.GetUserByEmail(userLogin.Email)).Returns(GetDbUser());

            var userManager = new UserManager(userProvider);
            

            Assert.ThrowsAny<Exception>(() => userManager.Login(userLogin).Result);
        }

        [Fact]
        public void User_Should_ThrowOnRegister()
        {
            var expected = GetUser();
            var userCreate = GetUserCreate();

            var userProvider = A.Fake<IUserProvider>();
            A.CallTo(() => userProvider.CreateUser(A<DbUser>.Ignored)).Returns(GetDbUser());

            var userManager = new UserManager(userProvider);

            Assert.ThrowsAny<Exception>(() => userManager.CreateUser(userCreate).Result);
        }
    }
}
