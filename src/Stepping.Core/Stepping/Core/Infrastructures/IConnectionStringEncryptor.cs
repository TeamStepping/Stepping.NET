namespace Stepping.Core.Infrastructures;

public interface IConnectionStringEncryptor
{
    Task<string> EncryptAsync(string connectionString);

    Task<string> DecryptAsync(string ciphertext);
}