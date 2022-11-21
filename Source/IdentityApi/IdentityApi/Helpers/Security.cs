using System.Security.Cryptography;
using System.Text;

namespace IdentityApi.Helpers
{
    public static class Security
    {
        /// <summary>
        /// Uses Sha512 to compute the hash of the password
        /// </summary>
        /// <returns>Sha512 computed hash</returns>
        private static string GetEncryptedPassword(string password)
        {
            SHA512 algorithm = SHA512.Create();

            byte[] data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
            string hashed = "";

            for (int i = 0; i <= data.Length - 1; i++)
                hashed += data[i].ToString("x2").ToUpperInvariant();

            return hashed;
        }

        /// <summary>
        /// Generates random salt based on the salt length
        /// </summary>
        /// <returns>Random generated salt</returns>
        public static string GetSalt(int saltLength = 50)
        {
            byte[] buffer = RandomNumberGenerator.GetBytes(saltLength);

            return Convert.ToBase64String(buffer).Substring(1, saltLength);
        }

        /// <summary>
        /// Adds salt to the password and computes password with sha512
        /// </summary>
        /// <returns>Hashed password with salt</returns>
        public static string GetEncryptedAndSaltedPassword(string password, string passwordSalt)
        {
            return GetEncryptedPassword($"{passwordSalt}{GetEncryptedPassword(password)}{passwordSalt}");
        }
    }
}
