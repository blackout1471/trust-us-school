using FakeItEasy;
using IdentityApi.DbModels;
using IdentityApi.Providers;
using System.Data;

namespace IdentityApiUnitTest.Providers
{
    public class UserProviderShould
    {
        public UserProviderShould()
        {

        }

        [Fact]
        public async Task ExpectUserData_WhenUserHasBeenCreated_CreateUserAsync()
        {
            // Arrange
            var dataResult = new DataTable();
            dataResult.Columns.Add(new DataColumn("Email", typeof(string)));
            dataResult.Columns.Add(new DataColumn("ID", typeof(int)));
            dataResult.Columns.Add(new DataColumn("FirstName", typeof(string)));
            dataResult.Columns.Add(new DataColumn("LastName", typeof(string)));
            dataResult.Columns.Add(new DataColumn("PhoneNumber", typeof(string)));
            dataResult.Columns.Add(new DataColumn("IsVerified", typeof(bool)));
            dataResult.Columns.Add(new DataColumn("IsLocked", typeof(bool)));
            dataResult.Columns.Add(new DataColumn("LockedDate", typeof(DateTime)));
            dataResult.Columns.Add(new DataColumn("FailedTries", typeof(int)));
            dataResult.Columns.Add(new DataColumn("HashedPassword", typeof(string)));
            dataResult.Columns.Add(new DataColumn("Salt", typeof(string)));
            dataResult.Columns.Add(new DataColumn("SecretKey", typeof(string)));
            dataResult.Columns.Add(new DataColumn("Counter", typeof(long)));
            dataResult.Columns.Add(new DataColumn("LastRequestDate", typeof(DateTime)));

            var row = dataResult.NewRow();
            row[0] = "email";
            row[1] = 1;
            row[2] = "firstname";
            row[3] = "lastname";
            row[4] = "phone";
            row[5] = false;
            row[6] = true;
            row[7] = DateTime.Now;
            row[8] = 2;
            row[9] = "pass";
            row[10] = "salt";
            row[11] = "secret";
            row[12] = 3;
            row[13] = DateTime.Now;
            dataResult.Rows.Add(row);

            var provider = new FakeSqlProvider();
            provider.Result = dataResult;

            // Act
            var actual = await provider.CreateUserAsync(A.Fake<DbUser>());

            // Assert
            Assert.Equal(row[0], actual.Email);
            Assert.Equal(row[1], actual.ID);
            Assert.Equal(row[2], actual.FirstName);
            Assert.Equal(row[3], actual.LastName);
            Assert.Equal(row[4], actual.PhoneNumber);
            Assert.Equal(row[5], actual.IsVerified);
            Assert.Equal(row[6], actual.IsLocked);
            Assert.Equal(row[7], actual.LockedDate);
            Assert.Equal(row[8], actual.FailedTries);
            Assert.Equal(row[9], actual.HashedPassword);
            Assert.Equal(row[10], actual.Salt);
            Assert.Equal(row[11], actual.SecretKey);
            Assert.Equal(row[12], actual.Counter);
            Assert.Equal(row[13], actual.LastRequestDate);
        }

        internal class FakeSqlProvider : UserProvider
        {
            public DataTable Result { get; set; } = default!;

            public FakeSqlProvider() : base(null) { }

            protected override Task<DataTable> RunQueryAsync(string query)
            {
                return Task.FromResult(Result);
            }

            protected override Task<DataTable> RunSpAsync(string spName, params SpElement[] spElements)
            {
                return Task.FromResult(Result);
            }
        }
    }
}
