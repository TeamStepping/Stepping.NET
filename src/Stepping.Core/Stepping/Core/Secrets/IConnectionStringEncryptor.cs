namespace Stepping.Core.Secrets;

public interface IConnectionStringEncryptor
{
    Task<string> EncryptAsync(string connectionString);

    Task<string> DecryptAsync(string ciphertext);
}