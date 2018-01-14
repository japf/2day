using System.Text;

namespace Chartreuse.Today.Core.Shared.Tools.Security
{
    public static class MD
    {
        public static string Encrypt(string input)
        {
            byte[] originalBytes = Encoding.UTF8.GetBytes(input);
            byte[] encodedBytes = new MD5CryptoServiceProvider().ComputeHash(originalBytes);
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < encodedBytes.Length; i++)
            {
                result.Append(encodedBytes[i].ToString("x2"));
            }
            return result.ToString();
        }
    }
}
