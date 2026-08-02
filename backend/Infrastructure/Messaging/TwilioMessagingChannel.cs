using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MatdarSathi.API.Infrastructure.Messaging;

public class TwilioMessagingChannel : IMessagingChannel
{
    private readonly MessagingSettings _settings;
    private readonly ILogger<TwilioMessagingChannel> _logger;

    public TwilioMessagingChannel(
        IOptions<MessagingSettings> settings,
        ILogger<TwilioMessagingChannel> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<OutboundMessageResult> SendMessageAsync(
        string recipient,
        string message,
        MessagingChannelType channelType,
        CancellationToken cancellationToken = default)
    {
        var messageId = $"msg_{Guid.NewGuid():N}";
        var senderNumber = channelType switch
        {
            MessagingChannelType.WhatsApp => _settings.WhatsAppFromNumber,
            MessagingChannelType.Notification => _settings.NotificationChannelEndpoint,
            _ => _settings.FromPhoneNumber
        };

        _logger.Information(
            "[{ChannelType}] Outbound message dispatched to {Recipient} from {SenderNumber}. MessageId: {MessageId}. Body length: {Length}",
            channelType,
            recipient,
            senderNumber,
            messageId,
            message.Length);

        var result = new OutboundMessageResult(true, messageId);
        return Task.FromResult(result);
    }

    public Task<InboundMessage> ReceiveWebhookAsync(
        WebhookPayload payload,
        CancellationToken cancellationToken = default)
    {
        var channelType = MessagingChannelType.Sms;
        var sender = payload.Sender ?? string.Empty;

        if (sender.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(payload.Channel, "WhatsApp", StringComparison.OrdinalIgnoreCase))
        {
            channelType = MessagingChannelType.WhatsApp;
        }
        else if (string.Equals(payload.Channel, "Notification", StringComparison.OrdinalIgnoreCase))
        {
            channelType = MessagingChannelType.Notification;
        }

        if (string.IsNullOrWhiteSpace(sender))
        {
            sender = "anonymous_sender";
        }

        _logger.Information(
            "Inbound {ChannelType} webhook payload received from {Sender}. Body: '{Body}'",
            channelType,
            sender,
            payload.Body);

        var result = new InboundMessage(sender, payload.Body ?? string.Empty, channelType, payload.MessageId);
        return Task.FromResult(result);
    }
}

internal static class LoggerExtensions
{
    public static void Information(this ILogger logger, string message, params object?[] args)
    {
        logger.LogInformation(message, args);
    }
}
