namespace Chartreuse.Today.Core.Shared.Services
{
    public interface ICryptoService
    {
        byte[] Encrypt(string input);
        string Decrypt(byte[] input);

        byte[] RsaEncrypt(string publicKey, string data);
    }
}
