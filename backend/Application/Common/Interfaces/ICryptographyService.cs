namespace MatdanSathi.API.Application.Common.Interfaces;

public interface ICryptographyService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    string GenerateBlindIndex(string plainText);
}
