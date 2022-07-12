namespace Stepping.Core.Infrastructures;

public class NullConnectionStringEncryptor : IConnectionStringEncryptor
{
    public virtual Task<string> EncryptAsync(string connectionString)
    {
        return Task.FromResult(connectionString);
    }

    public virtual Task<string> DecryptAsync(string ciphertext)
    {
        return Task.FromResult(ciphertext);
    }
}