using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace iRaidTools
{
    public class PasswordHelper
    {
        private static readonly Random random       = new Random();
        private static readonly object synclock     = new object();
        private static readonly HashAlgorithm SHA   = SHA1.Create();
        private static readonly string characters   = "abdcefghkmnpqrstwxyz"
                                                    + "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
                                                    + "0123456789"
                                                    + "!#%$@";

        public static string ComputeHash(string password, string salt)
        {
            string Hash = StringToSHA1(password + salt);
            for (int x = 0; x < 25; x++)
            {
                string tmp = StringToSHA1(Hash + salt);
                Hash = tmp;
            }

            return Hash;
        }

        /// <summary>
        /// Generates a random string based with length of size.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string RandomString(int size)
        {
            return new String(Enumerable.Repeat(characters, size).Select(s => s[Random(s.Length)]).ToArray());
        }

        /// <summary>
        /// Returns hash for given input string.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        private static string StringToSHA1(string inputString)
        {
            var hash = SHA.ComputeHash(Encoding.UTF8.GetBytes(inputString));

            StringBuilder sb = new StringBuilder();
            foreach (var h in hash)
            {
                sb.Append(h.ToString("X2"));
            }

            return sb.ToString();
        } 

        /// <summary>
        /// Returns a random number between min and max integer.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private static int RandomBetween(int min, int max)
        {
            lock(synclock)
            {
                return random.Next(min, max);
            }
        }

        /// <summary>
        /// Returns a rand
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private static int Random(int num)
        {
            lock (synclock)
            {
                return random.Next(num);
            }
        }
    }
}
