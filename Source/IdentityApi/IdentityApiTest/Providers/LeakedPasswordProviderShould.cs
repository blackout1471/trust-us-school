using IdentityApi.DbModels;
using IdentityApi.Providers;
using System.Data;

namespace IdentityApiUnitTest.Providers
{
    public class LeakedPasswordProviderShould
    {
        public LeakedPasswordProviderShould() {}

        [Fact]
        public async Task ExpectFalse_WhenDatatableRowIs0_GetIsPasswordLeakedAsync()
        {
            // Arrange
            var dataResult = new DataTable();
            var provider = new FakeSqlProvider();
            provider.Result = dataResult;

            // Act
            var expected = await provider.GetIsPasswordLeakedAsync("");

            // Assert
            Assert.False(expected);
        }

        [Fact]
        public async Task ExpectTrue_WhenDatatableResultIs1_GetIsPasswordLeakedAsync()
        {
            // Arrange
            var dataResult = new DataTable();
            dataResult.Columns.Add(new DataColumn("Col1", typeof(int)));
            
            var row = dataResult.NewRow();
            row[0] = 1;
            dataResult.Rows.Add(row);

            var provider = new FakeSqlProvider();
            provider.Result = dataResult;

            // Act
            var expected = await provider.GetIsPasswordLeakedAsync("");

            // Assert
            Assert.True(expected);
        }

        [Fact]
        public async Task ExpectFalse_WhenDatatableResultIs0_GetIsPasswordLeakedAsync()
        {
            // Arrange
            var dataResult = new DataTable();
            dataResult.Columns.Add(new DataColumn("Col1", typeof(int)));

            var row = dataResult.NewRow();
            row[0] = 0;
            dataResult.Rows.Add(row);

            var provider = new FakeSqlProvider();
            provider.Result = dataResult;

            // Act
            var expected = await provider.GetIsPasswordLeakedAsync("");

            // Assert
            Assert.False(expected);
        }

        internal class FakeSqlProvider : LeakedPasswordProvider
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
