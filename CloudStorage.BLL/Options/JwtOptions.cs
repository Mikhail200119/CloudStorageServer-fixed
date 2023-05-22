namespace CloudStorage.BLL.Options;

public class JwtOptions
{
    public TimeSpan? ExpirationTime { get; set; }
    public string SecretKey { get; set; }
    public IEnumerable<string> EncryptionAlgorithms { get; set; }
}