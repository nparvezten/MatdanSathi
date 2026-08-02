namespace MatdarSathi.API.Infrastructure.Messaging;

public class MessagingSettings
{
    public const string SectionName = "MessagingSettings";

    public string AccountSid { get; set; } = "AC_mock_twilio_account_sid_matdarsathi";
    public string AuthToken { get; set; } = "mock_twilio_auth_token_secret";
    public string FromPhoneNumber { get; set; } = "+18005550199";
    public string WhatsAppFromNumber { get; set; } = "whatsapp:+18005550199";
    public string NotificationChannelEndpoint { get; set; } = "https://api.matdarsathi.org/notifications";
    public bool EnableLiveDelivery { get; set; } = false;
}
