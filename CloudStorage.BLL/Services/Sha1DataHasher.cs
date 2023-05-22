using System.Security.Cryptography;
using CloudStorage.BLL.Services.Interfaces;

namespace CloudStorage.BLL.Services;

public class Sha1DataHasher : IDataHasher
{
    public string HashData(byte[] data) => string.Concat(SHA1.HashData(data).Select(byteElement => byteElement.ToString("X2")));
    
    public string HashStreamData(Stream data) =>
        string.Concat(SHA256.Create()
            .ComputeHash(data));
}