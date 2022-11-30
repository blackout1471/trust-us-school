using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace IdentityApi.Helpers
{
    public static class Security
    {
        /// <summary>
        /// Uses Sha256 to compute the hash of the password
        /// </summary>
        /// <returns>Sha256 computed hash</returns>
        private static string ComputeHash(string valueToHash)
        {
            SHA256 algorithm = SHA256.Create();

            byte[] data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(valueToHash));
            string hashed = "";

            for (int i = 0; i <= data.Length - 1; i++)
                hashed += data[i].ToString("x2").ToUpperInvariant();

            return hashed;
        }

        private static string HashPassword(string password, string salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));

            argon2.Salt = Convert.FromBase64String(salt);
            argon2.DegreeOfParallelism = 4;
            argon2.Iterations = 4;
            argon2.MemorySize = 1024 * 32;

            return Convert.ToBase64String(argon2.GetBytes(64));
        }


        /// <summary>
        /// Takes a key and a counter, hashes them, then bitwise
        /// </summary>
        /// <returns>A 20 byte HOTP</returns>
        public static string GetHotp(string key, long counter)
        {
            byte[] keyBytes = Convert.FromBase64String(key);
            byte[] counterInBytes = BitConverter.GetBytes(counter);
            var hmacsha = new HMACSHA512(keyBytes);
            byte[] hmac_result = hmacsha.ComputeHash(counterInBytes);

            //TODO: If we have time, implement and understand bitwise operation to make a 6 digit int instead of a giant ass string
            string hotp = "";
            for (int i = 0; i <= hmac_result.Length - 1; i++)
                hotp += hmac_result[i].ToString("x2").ToUpperInvariant();

            return hotp;
        }

        /// <summary>
        /// Generates a random HMAC key
        /// </summary>
        public static string GetHmacKey()
        {
            var hmac = new HMACSHA512();
            var key = Convert.ToBase64String(hmac.Key);
            return key;
        }
        /// <summary>
        /// Generates random salt based on the salt length
        /// </summary>
        /// <returns>Random generated salt</returns>
        public static string GetSalt(int saltLength = 32)
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(saltLength));
        }

        /// <summary>
        /// Adds salt to the password and computes password with sha512
        /// </summary>
        /// <returns>Hashed password with salt</returns>
        public static string GetEncryptedAndSaltedPassword(string password, string salt, string pepper)
        {
            // Get hashed password with pepper
            var hashedPassword = ComputeHash($"{password}{pepper}");

            return HashPassword(hashedPassword, salt);
        }
    }
}
