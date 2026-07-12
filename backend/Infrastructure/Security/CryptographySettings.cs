namespace MatdanSathi.API.Infrastructure.Security;

public class CryptographySettings
{
    public const string SectionName = "CryptographySettings";

    public string EncryptionKey { get; set; } = null!;
    public string BlindIndexSalt { get; set; } = null!;
}
