using System;
using System.Security.Cryptography;
using System.Text;

namespace CIB.Exchange
{
    internal static class SignHelper
    {
        public static string HmacSha512(string key, string message)
        {
            var keyByte = Encoding.ASCII.GetBytes(key);
            var messageBytes = Encoding.ASCII.GetBytes(message);
            using (var hmacsha512 = new HMACSHA512(keyByte))
            {
                Byte[] result = hmacsha512.ComputeHash(messageBytes);
                return BitConverter.ToString(result).Replace("-", "").ToLower();
            }
        }

        public static string HmacSha256(string key, string message)
        {
            var keyByte = Encoding.ASCII.GetBytes(key);
            var messageBytes = Encoding.ASCII.GetBytes(message);
            using (var hmacsha512 = new HMACSHA256(keyByte))
            {
                Byte[] result = hmacsha512.ComputeHash(messageBytes);
                return BitConverter.ToString(result).Replace("-", "").ToLower();
            }
        }
    }
}
