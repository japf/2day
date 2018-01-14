using System;
using System.Security.Cryptography;
using System.Text;
using Chartreuse.Today.Core.Shared.Services;

namespace Chartreuse.Today.Core.Shared.Tests.Impl
{
    public class TestCryptoService : ICryptoService
    {        
        public byte[] Encrypt(string input)
        {
            return Encoding.Unicode.GetBytes(input);
        }

        public string Decrypt(byte[] input)
        {
            return Encoding.Unicode.GetString(input);
        }

        public byte[] RsaEncrypt(string publicKeyXml, string data)
        {
            // Variables
            CspParameters cspParams = null;
            RSACryptoServiceProvider rsaProvider = null;
            byte[] plainBytes = null;
            byte[] encryptedBytes = null;

            try
            {
                // Select target CSP
                cspParams = new CspParameters();
                cspParams.ProviderType = 1; // PROV_RSA_FULL 
                //cspParams.ProviderName; // CSP name
                rsaProvider = new RSACryptoServiceProvider(cspParams);

                // Import public key
                rsaProvider.FromXmlString(publicKeyXml);

                // Encrypt plain text
                plainBytes = Encoding.Unicode.GetBytes(data);
                encryptedBytes = rsaProvider.Encrypt(plainBytes, false);
            }
            catch (Exception ex)
            {
                // Any errors? Show them
                Console.WriteLine("Exception encrypting file! More info:");
                Console.WriteLine(ex.Message);
            }

            return encryptedBytes;
        }
    }
}
