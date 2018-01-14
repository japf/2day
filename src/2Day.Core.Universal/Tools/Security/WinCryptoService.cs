using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Chartreuse.Today.Core.Shared.Services;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace Chartreuse.Today.Core.Universal.Tools.Security
{
    public class WinCryptoService : ICryptoService
    {
        private const string PrivateKey = "BwIAAACkAABSU0EyAAQAAAEAAQBpfGlUzMYXvhT1u+++nH4JCEyOk30OMK8k+xu4i1SUl+j5T1HXsGgxAKSOxbOykyF08iil+Edyl4nt9QeKDCPrmQlxVgHd0SBCQO4kCWKqJ1kieKp2aDeKyybC5hV7cOSaFnUp8miBRnkuLe4LnSCCSK0GNdwOFH2JLbl8DbnJ46GeEE73KQz6SeDiH7Ls/X//v1xXDpCjVULUtgyqhxp+TLqQ8X6wrSlSOBd2L7CkTQUA+CrngSKlD1EuLiFDJ/nJ8Obw64UowashD4eoZcAJZgK6caRF4Ehutcs9v4rZfurIvqgcjWRFGFxpmiycocbrwUhQCRIJilyazlvyKAzqgYsgdII3yVFmlzC/FM73LlLQjmfVbLtXlnZfnWpITgV131OaK1rL6Acs/Lx/xqqf32fAsPADIMkeebwwcUJiATnjJeTOJm7QR9VsSk1JTWXGR5VxnrUI7t+2hJjxE0yACYbXorU4i2asVXWikoUIKi+AE2wqmvm9ohW4nyDcacsWFklKFqTQUjWAT3QiJbl9GYI8SA6f8Ys2pcdJUgNCaokCWLf3GBkAWGgSyQ9s+++DmafucQfpGlnQbNQn7LjF4UChAULUNT5bhUQClHyuSpFl9GKWopKFZkkr8u8WQH06Wo4ktNS0NnSg6UzaQYfT3yD5D425w9b0YrqbLGiQju2A0X1JQzJazzFlMfItRvK7CZE1Jfqt65mG8ycvJDideYxDTKjiWOLEc+wHhXIipna0gsZm8OUDXHWlV4z0JAQ=";
        private const string PublicKey = "BgIAAACkAABSU0ExAAQAAAEAAQBpfGlUzMYXvhT1u+++nH4JCEyOk30OMK8k+xu4i1SUl+j5T1HXsGgxAKSOxbOykyF08iil+Edyl4nt9QeKDCPrmQlxVgHd0SBCQO4kCWKqJ1kieKp2aDeKyybC5hV7cOSaFnUp8miBRnkuLe4LnSCCSK0GNdwOFH2JLbl8DbnJ4w==";
        
        public byte[] Encrypt(string input)
        {
            return WinRTEncrypt(PublicKey, input);
        }

        public string Decrypt(byte[] input)
        {
            return WinRTDecrypt(PrivateKey, input);
        }

        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            if (bytes == null)
                return string.Empty;

            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public byte[] RsaEncrypt(string publicKey, string data)
        {
            return WinRTEncrypt(publicKey, data);
        }

        public static Tuple<string, string> WinRTCreateKeyPair()
        {
            AsymmetricKeyAlgorithmProvider asym = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaPkcs1);
            CryptographicKey key = asym.CreateKeyPair(1024);
            
            IBuffer privateKeyBuffer = key.Export(CryptographicPrivateKeyBlobType.Capi1PrivateKey);
            IBuffer publicKeyBuffer = key.ExportPublicKey(CryptographicPublicKeyBlobType.Capi1PublicKey);
            
            byte[] privateKeyBytes;
            byte[] publicKeyBytes;
            
            CryptographicBuffer.CopyToByteArray(privateKeyBuffer, out privateKeyBytes);
            CryptographicBuffer.CopyToByteArray(publicKeyBuffer, out publicKeyBytes);

            string privateKey = Convert.ToBase64String(privateKeyBytes);
            string publicKey = Convert.ToBase64String(publicKeyBytes);

            return new Tuple<string, string>(privateKey, publicKey);
        }

        public static byte[] WinRTEncrypt(string publicKey, string data)
        {
            if (string.IsNullOrEmpty(data))
                return new byte[0];

            IBuffer keyBuffer = CryptographicBuffer.DecodeFromBase64String(publicKey);

            AsymmetricKeyAlgorithmProvider asym = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaPkcs1);
            CryptographicKey key = asym.ImportPublicKey(keyBuffer, CryptographicPublicKeyBlobType.Capi1PublicKey);

            IBuffer plainBuffer = CryptographicBuffer.ConvertStringToBinary(data, BinaryStringEncoding.Utf8);
            IBuffer encryptedBuffer = CryptographicEngine.Encrypt(key, plainBuffer, null);

            byte[] encryptedBytes;
            CryptographicBuffer.CopyToByteArray(encryptedBuffer, out encryptedBytes);

            return encryptedBytes;
        }

        public static string WinRTDecrypt(string privateKey, byte[] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            IBuffer keyBuffer = CryptographicBuffer.DecodeFromBase64String(privateKey);

            AsymmetricKeyAlgorithmProvider asym = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaPkcs1);
            CryptographicKey key = asym.ImportKeyPair(keyBuffer, CryptographicPrivateKeyBlobType.Capi1PrivateKey);
            
            IBuffer plainBuffer = CryptographicEngine.Decrypt(key, data.AsBuffer(), null);

            byte[] plainBytes;
            CryptographicBuffer.CopyToByteArray(plainBuffer, out plainBytes);

            return Encoding.UTF8.GetString(plainBytes, 0, plainBytes.Length);
        }
    }
}
