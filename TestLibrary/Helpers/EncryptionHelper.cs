using System.Security.Cryptography;
using System.Text;

namespace TestLibrary.Helpers
{
    public static class EncryptionHelper
    {
        public static string GetHash(this string str)
        {
            var bytes = new UTF8Encoding().GetBytes(str);
            var hashBytes = MD5.Create()
                               .ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
