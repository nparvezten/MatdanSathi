using Microsoft.Extensions.Options;
using MatdanSathi.API.Infrastructure.Security;
using Xunit;

namespace MatdanSathi.API.Tests;

public class CryptographyTests
{
    private readonly CryptographyService _cryptographyService;

    public CryptographyTests()
    {
        var settings = new CryptographySettings
        {
            EncryptionKey = "8sD3K5wGq9L2zR7vP1xB4cM6tJ0eY8uI",
            BlindIndexSalt = "mY-s3cr3t-s4lt-f0r-bl1nd-1ndex-HMAC-256"
        };
        var options = Options.Create(settings);
        _cryptographyService = new CryptographyService(options);
    }

    [Fact]
    public void Encrypt_And_Decrypt_Should_Return_OriginalText()
    {
        // Arrange
        string plainText = "Voter Card ID: ABC1234567";

        // Act
        string cipherText = _cryptographyService.Encrypt(plainText);
        string decryptedText = _cryptographyService.Decrypt(cipherText);

        // Assert
        Assert.NotEqual(plainText, cipherText);
        Assert.Equal(plainText, decryptedText);
    }

    [Fact]
    public void GenerateBlindIndex_Should_Be_Deterministic()
    {
        // Arrange
        string plainText1 = "ABC1234567";
        string plainText2 = "ABC1234567 ";

        // Act
        string hash1 = _cryptographyService.GenerateBlindIndex(plainText1);
        string hash2 = _cryptographyService.GenerateBlindIndex(plainText2);

        // Assert
        Assert.Equal(hash1, hash2);
    }
}
