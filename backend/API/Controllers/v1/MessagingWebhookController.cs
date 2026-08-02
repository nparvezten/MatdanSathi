using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Asp.Versioning;
using MatdarSathi.API.Application.Common.Constants;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Application.Voters.Queries.CheckVoterRegistration;
using MatdarSathi.API.Infrastructure.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;

namespace MatdarSathi.API.Controllers.v1;

public record WebhookRequestDto(string? From, string? Body, string? Channel, string? MessageSid);

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/messaging")]
[AllowAnonymous]
[EnableRateLimiting("strict-limit")]
public class MessagingWebhookController : ControllerBase
{
    private readonly IMessagingChannel _messagingChannel;
    private readonly IMediator _mediator;
    private readonly ILogger<MessagingWebhookController> _logger;

    public MessagingWebhookController(
        IMessagingChannel messagingChannel,
        IMediator mediator,
        ILogger<MessagingWebhookController> logger)
    {
        _messagingChannel = messagingChannel;
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> HandleInboundWebhook([FromBody] WebhookRequestDto? bodyDto)
    {
        string sender = string.Empty;
        string body = string.Empty;
        string? channelHeader = null;
        string? messageSid = null;

        if (Request.HasFormContentType)
        {
            var form = await Request.ReadFormAsync(HttpContext.RequestAborted);
            sender = form["From"].ToString();
            body = form["Body"].ToString();
            channelHeader = form["Channel"].ToString();
            messageSid = form["MessageSid"].ToString();
        }
        else if (bodyDto != null)
        {
            sender = bodyDto.From ?? string.Empty;
            body = bodyDto.Body ?? string.Empty;
            channelHeader = bodyDto.Channel;
            messageSid = bodyDto.MessageSid;
        }
        else
        {
            using var reader = new StreamReader(Request.Body);
            body = await reader.ReadToEndAsync(HttpContext.RequestAborted);
            if (Request.Headers.TryGetValue("X-Sender-ID", out var senderHeader))
            {
                sender = senderHeader.ToString();
            }
            if (Request.Headers.TryGetValue("X-Channel-Type", out var cHeader))
            {
                channelHeader = cHeader.ToString();
            }
        }

        var payload = new WebhookPayload(sender, body, channelHeader, messageSid);

        // 1. Parse payload via messaging channel adapter (WhatsApp / SMS / Notification)
        var inboundMessage = await _messagingChannel.ReceiveWebhookAsync(payload, HttpContext.RequestAborted);
        _logger.LogInformation("Processing webhook message from {Sender} via {Channel}", inboundMessage.Sender, inboundMessage.ChannelType);

        if (string.IsNullOrWhiteSpace(inboundMessage.Body))
        {
            var emptyReply = BuildBilingualResponse(
                "Please provide a valid EPIC card number (e.g. ABC1234567).",
                "कृपया वैध मतदार ओळखपत्र क्रमांक पाठवा (उदा. ABC1234567).");

            await _messagingChannel.SendMessageAsync(
                inboundMessage.Sender,
                emptyReply,
                inboundMessage.ChannelType,
                HttpContext.RequestAborted);

            return Ok(new { status = "NoBody", reply = emptyReply });
        }

        // 2. Extract EPIC pattern from message body
        var epicMatch = Regex.Match(inboundMessage.Body.Trim(), EpicRegexConstants.EpicExtractorPattern, RegexOptions.IgnoreCase);
        if (!epicMatch.Success)
        {
            var invalidReply = BuildBilingualResponse(
                $"No valid EPIC number found in '{inboundMessage.Body}'. Please send a valid EPIC number (e.g., ABC1234567).",
                $"'{inboundMessage.Body}' मध्ये कोणताही वैध मतदार ओळखपत्र क्रमांक सापडला नाही. कृपया वैध EPIC क्रमांक पाठवा.");

            await _messagingChannel.SendMessageAsync(
                inboundMessage.Sender,
                invalidReply,
                inboundMessage.ChannelType,
                HttpContext.RequestAborted);

            return Ok(new { status = "InvalidEpicFormat", reply = invalidReply });
        }

        var epicNumber = epicMatch.Value.ToUpperInvariant();

        // 3. Reuse EXISTING voter lookup logic via native Mediator query handler
        var query = new CheckVoterRegistrationQuery(
            EpicNumber: epicNumber,
            VerifierId: $"messaging:{inboundMessage.Sender}",
            VerificationMethod: $"{inboundMessage.ChannelType}_MessagingChannel");

        VoterRegistrationDto lookupResult;
        try
        {
            lookupResult = await _mediator.Send(query, HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during voter check query for EPIC {Epic}", epicNumber);

            var errorReply = BuildBilingualResponse(
                $"Anomaly or error flagged while looking up EPIC {epicNumber}. Please contact your BLO.",
                $"EPIC {epicNumber} च्या शोधादरम्यान त्रुटी/विसंगती आढळली. कृपया तुमच्या BLO शी संपर्क साधा.");

            await _messagingChannel.SendMessageAsync(
                inboundMessage.Sender,
                errorReply,
                inboundMessage.ChannelType,
                HttpContext.RequestAborted);

            return Ok(new { status = "AnomalyFlagged", reply = errorReply });
        }

        // 4. Format bilingual response (English + Marathi) with explicit consent language matching web app standard
        string responseMessage;
        if (lookupResult.IsVerified)
        {
            responseMessage = BuildBilingualResponse(
                $"[FOUND] Voter Registration Active\n" +
                $"Name: {lookupResult.FullName}\n" +
                $"EPIC: {lookupResult.EpicNumber}\n" +
                $"Assembly: AC {lookupResult.AssemblyConstituency}, Part: {lookupResult.PartNumber}, Serial: {lookupResult.SerialNumber}\n" +
                $"Polling Station: {lookupResult.PollingStationName} ({lookupResult.PollingStationLocation})\n" +
                $"BLO: {lookupResult.BloName} ({lookupResult.BloContact})",

                $"[सापडले] मतदार नोंदणी सक्रिय\n" +
                $"नाव: {lookupResult.FullName}\n" +
                $"EPIC: {lookupResult.EpicNumber}\n" +
                $"विधानसभा: AC {lookupResult.AssemblyConstituency}, भाग: {lookupResult.PartNumber}, अनुक्रमांक: {lookupResult.SerialNumber}\n" +
                $"मतदान केंद्र: {lookupResult.PollingStationName} ({lookupResult.PollingStationLocation})\n" +
                $"BLO: {lookupResult.BloName} ({lookupResult.BloContact})");
        }
        else
        {
            responseMessage = BuildBilingualResponse(
                $"[NOT FOUND] EPIC {epicNumber} was not found on the draft electoral roll.",
                $"[सापडले नाही] मसुदा मतदार यादीत EPIC {epicNumber} सापडला नाही.");
        }

        // 5. Send reply back to user via IMessagingChannel
        await _messagingChannel.SendMessageAsync(
            inboundMessage.Sender,
            responseMessage,
            inboundMessage.ChannelType,
            HttpContext.RequestAborted);

        return Ok(new
        {
            status = lookupResult.IsVerified ? "Found" : "NotFound",
            epic = epicNumber,
            sender = inboundMessage.Sender,
            channel = inboundMessage.ChannelType.ToString(),
            reply = responseMessage
        });
    }

    private static string BuildBilingualResponse(string englishText, string marathiText)
    {
        const string consentNotice =
            "--- Privacy & Consent / गोपनीयता आणि संमती ---\n" +
            "By using MatdarSathi messaging lookup, you consent to checking public draft electoral roll records. MatdarSathi encrypts all queries and does not store raw PII.\n" +
            "मतदार साथी संदेश सेवा वापरून, तुम्ही मसुदा मतदार यादी तपासण्यास संमती देता. मतदार साथी सर्व डेटा एनक्रिप्ट करते आणि वैयक्तिक माहिती साठवत नाही.";

        return $"{englishText}\n\n{marathiText}\n\n{consentNotice}";
    }
}
