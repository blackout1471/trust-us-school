using IdentityApi.DbModels;
using IdentityApi.Interfaces;

namespace IdentityApi.Providers
{
    public class LeakedPasswordProvider : SqlProvider, ILeakedPasswordProvider
    {
        public LeakedPasswordProvider(IConfiguration configuration) : base(configuration.GetConnectionString("SQLserverLeaked"))
        {
        }

        /// <inheritdoc />
        public async Task<bool> GetIsPasswordLeakedAsync(string password)
        {
            var result = await RunSpAsync("SP_HashedPasswordExists", 
                new SpElement("HashedPassword", password, System.Data.SqlDbType.VarChar)
            );

            if (result.Rows.Count == 0)
                return false;

            return Convert.ToBoolean(result.Rows[0][0]);
        }
    }
}
