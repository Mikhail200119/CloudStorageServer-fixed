namespace CloudStorage.BLL.Services.Interfaces;

public interface IAesEncryptor
{
    Task<byte[]> EncryptAsync(byte[] data);
    Task<byte[]> DecryptAsync(byte[] data);
}