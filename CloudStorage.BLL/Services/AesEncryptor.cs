using CloudStorage.BLL.Options;
using CloudStorage.BLL.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace CloudStorage.BLL.Services;

public class AesEncryptor : IAesEncryptor
{
    private readonly FileEncryptionOptions _encryptionOptions;

    public AesEncryptor(IOptions<FileEncryptionOptions> encryptionOptions)
    {
        _encryptionOptions = encryptionOptions.Value;
    }

    public async Task<byte[]> EncryptAsync(byte[] data)
    {
        using var aes = GetAes();

        return aes.EncryptCfb(data, aes.IV);
    }

    public async Task<byte[]> DecryptAsync(byte[] data)
    {
        using var aes = GetAes();

        return aes.DecryptCfb(data, aes.IV);
    }

    private Aes GetAes()
    {
        var iv = new byte[16];
        var aes = Aes.Create();
        //aes.Key = Encoding.UTF8.GetBytes(_encryptionOptions.EncryptionKey);
        aes.Key = _encryptionOptions.EncryptionKey;
        aes.IV = iv;
        
        return aes;
    }
}