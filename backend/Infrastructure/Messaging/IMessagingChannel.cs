using System.Threading;
using System.Threading.Tasks;

namespace MatdarSathi.API.Infrastructure.Messaging;

public enum MessagingChannelType
{
    Sms,
    WhatsApp,
    Notification
}

public record InboundMessage(
    string Sender,
    string Body,
    MessagingChannelType ChannelType,
    string? MessageId = null);

public record OutboundMessageResult(
    bool Success,
    string? MessageId,
    string? ErrorMessage = null);

public record WebhookPayload(
    string Sender,
    string Body,
    string? Channel = null,
    string? MessageId = null);

public interface IMessagingChannel
{
    Task<OutboundMessageResult> SendMessageAsync(
        string recipient,
        string message,
        MessagingChannelType channelType,
        CancellationToken cancellationToken = default);

    Task<InboundMessage> ReceiveWebhookAsync(
        WebhookPayload payload,
        CancellationToken cancellationToken = default);
}
