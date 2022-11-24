using IdentityApi.Models;

namespace IdentityApiIntegrationTest
{
    public static class TestDataHelper
    {
        #region TestData

        /// <summary>
        /// Helper method to generate a new valid user request.
        /// </summary>
        public static UserCreate GenerateNewUserRequest()
        {
            var rnd = Guid.NewGuid()
                .ToString("n")
                .Substring(0, 8);

            return new UserCreate()
            {
                Email = $"test{rnd}@test.dk",
                Password = $"test{rnd}test",
                FirstName = $"test{rnd}",
                LastName = $"test",
                PhoneNumber = "454545452"
            };
        }
        #endregion
    }
}
