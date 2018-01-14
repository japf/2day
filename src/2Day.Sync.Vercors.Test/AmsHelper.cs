using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Chartreuse.Today.Sync.Vercors.Test
{
    public static class AmsHelper
    {
        public static string GetSecurityToken(TimeSpan periodBeforeExpires, string aud, string userId, string masterKey)
        {
            var now = DateTime.UtcNow;
            var utc0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var payload = new
            {
                exp = (int)now.Add(periodBeforeExpires).Subtract(utc0).TotalSeconds,
                iss = "urn:microsoft:windows-azure:zumo",
                ver = 2,
                aud = aud,
                uid = userId
            };

            var keyBytes = Encoding.UTF8.GetBytes(masterKey + "JWTSig");
            var segments = new List<string>();

            //kid changed to a string
            var header = new { alg = "HS256", typ = "JWT", kid = "0" };
            byte[] headerBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header, Formatting.None));
            byte[] payloadBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload, Formatting.None));
            segments.Add(Base64UrlEncode(headerBytes));
            segments.Add(Base64UrlEncode(payloadBytes));
            var stringToSign = string.Join(".", segments.ToArray());
            var bytesToSign = Encoding.UTF8.GetBytes(stringToSign);
            SHA256Managed hash = new SHA256Managed();
            byte[] signingBytes = hash.ComputeHash(keyBytes);
            var sha = new HMACSHA256(signingBytes);
            byte[] signature = sha.ComputeHash(bytesToSign);
            segments.Add(Base64UrlEncode(signature));
            return string.Join(".", segments.ToArray());
        }

        // from JWT spec
        private static string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Split('=')[0]; // Remove any trailing '='s
            output = output.Replace('+', '-'); // 62nd char of encoding
            output = output.Replace('/', '_'); // 63rd char of encoding
            return output;
        }
    }
}
