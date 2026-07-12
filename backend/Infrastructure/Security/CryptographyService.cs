using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using MatdanSathi.API.Application.Common.Interfaces;

namespace MatdanSathi.API.Infrastructure.Security;

public class CryptographyService : ICryptographyService
{
    private readonly byte[] _encryptionKey;
    private readonly byte[] _blindIndexSalt;

    public CryptographyService(IOptions<CryptographySettings> settings)
    {
        var encryptionKeyStr = settings.Value.EncryptionKey;
        var blindIndexSaltStr = settings.Value.BlindIndexSalt;

        if (string.IsNullOrWhiteSpace(encryptionKeyStr))
        {
            throw new ArgumentException("CryptographySettings:EncryptionKey is not configured.");
        }
        if (string.IsNullOrWhiteSpace(blindIndexSaltStr))
        {
            throw new ArgumentException("CryptographySettings:BlindIndexSalt is not configured.");
        }

        // Standardize key size to 256 bits (32 bytes) and salt size using SHA-256
        using var sha256 = SHA256.Create();
        _encryptionKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionKeyStr));
        _blindIndexSalt = sha256.ComputeHash(Encoding.UTF8.GetBytes(blindIndexSaltStr));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();
        var iv = aes.IV;

        using var encryptor = aes.CreateEncryptor(aes.Key, iv);
        using var ms = new MemoryStream();

        // Write the IV first to the stream (16 bytes for AES)
        ms.Write(iv, 0, iv.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var writer = new StreamWriter(cs, Encoding.UTF8))
        {
            writer.Write(plainText);
        }

        var cipherBytes = ms.ToArray();
        return Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return string.Empty;
        }

        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;

        var ivSize = aes.BlockSize / 8; // 16 bytes
        if (fullCipher.Length < ivSize)
        {
            throw new CryptographicException("Invalid cipher text length.");
        }

        var iv = new byte[ivSize];
        var cipherBytes = new byte[fullCipher.Length - ivSize];

        Array.Copy(fullCipher, 0, iv, 0, ivSize);
        Array.Copy(fullCipher, ivSize, cipherBytes, 0, cipherBytes.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(cipherBytes);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs, Encoding.UTF8);

        return reader.ReadToEnd();
    }

    public string GenerateBlindIndex(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        // Deterministic hash with key/salt for blind index mapping
        using var hmac = new HMACSHA256(_blindIndexSalt);
        var bytes = Encoding.UTF8.GetBytes(plainText.Trim());
        var hashBytes = hmac.ComputeHash(bytes);

        return Convert.ToBase64String(hashBytes);
    }
}
