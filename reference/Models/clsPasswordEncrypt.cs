using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace proCabWeb.Models
{
    public static class clsPasswordEncrypt
    {
        public static string GetHash(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        public static string GetHash64(string password)
        {
            using (MD5 hasher = MD5.Create())    // create hash object
            {
                // Convert to byte array and get hash
                byte[] dbytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(password));
                string sBuilder;
                sBuilder = Convert.ToBase64String(dbytes);
                return sBuilder.ToString();
            }
        }
    }
}