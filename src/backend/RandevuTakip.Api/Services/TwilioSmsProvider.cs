using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Newtonsoft.Json;

namespace RandevuTakip.Api.Services;

public class TwilioSmsProvider : INotificationProvider
{
    private readonly ILogger<TwilioSmsProvider> _logger;

    public TwilioSmsProvider(ILogger<TwilioSmsProvider> logger)
    {
        _logger = logger;
    }

    public string ProviderName => "Twilio";

    public async Task<bool> SendSmsAsync(string phoneNumber, string message, string? configJson = null)
    {
        if (string.IsNullOrEmpty(configJson))
        {
            _logger.LogWarning("Twilio SMS: No configuration provided.");
            return false;
        }

        try
        {
            var config = JsonConvert.DeserializeObject<TwilioConfig>(configJson);
            if (config == null || string.IsNullOrEmpty(config.TwilioSid) || string.IsNullOrEmpty(config.TwilioAuthToken))
            {
                _logger.LogWarning("Twilio SMS: Invalid configuration.");
                return false;
            }

            TwilioClient.Init(config.TwilioSid, config.TwilioAuthToken);

            var messageOptions = new CreateMessageOptions(new PhoneNumber(phoneNumber))
            {
                From = new PhoneNumber(config.FromNumber),
                Body = message
            };

            var msg = await MessageResource.CreateAsync(messageOptions);
            _logger.LogInformation("Twilio SMS: Message sent successfully. SID: {Id}", msg.Sid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Twilio SMS: Error sending message to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    public async Task<bool> SendWhatsAppAsync(string phoneNumber, string message, string? configJson = null)
    {
        if (string.IsNullOrEmpty(configJson))
        {
            _logger.LogWarning("Twilio WhatsApp: No configuration provided.");
            return false;
        }

        try
        {
            var config = JsonConvert.DeserializeObject<TwilioConfig>(configJson);
            if (config == null || string.IsNullOrEmpty(config.TwilioSid) || string.IsNullOrEmpty(config.TwilioAuthToken))
            {
                _logger.LogWarning("Twilio WhatsApp: Invalid configuration.");
                return false;
            }

            TwilioClient.Init(config.TwilioSid, config.TwilioAuthToken);

            // Twilio WhatsApp numbers must be prefixed with 'whatsapp:'
            var to = phoneNumber.StartsWith("whatsapp:") ? phoneNumber : "whatsapp:" + phoneNumber;
            var from = config.FromNumber!.StartsWith("whatsapp:") ? config.FromNumber : "whatsapp:" + config.FromNumber;

            var messageOptions = new CreateMessageOptions(new PhoneNumber(to))
            {
                From = new PhoneNumber(from),
                Body = message
            };

            var msg = await MessageResource.CreateAsync(messageOptions);
            _logger.LogInformation("Twilio WhatsApp: Message sent successfully. SID: {Id}", msg.Sid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Twilio WhatsApp: Error sending message to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    private class TwilioConfig
    {
        public string? TwilioSid { get; set; }
        public string? TwilioAuthToken { get; set; }
        public string? FromNumber { get; set; }
    }
}
