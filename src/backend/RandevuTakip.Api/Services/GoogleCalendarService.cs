using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Newtonsoft.Json;

namespace RandevuTakip.Api.Services;

public class GoogleCalendarService
{
    private readonly ILogger<GoogleCalendarService> _logger;

    public GoogleCalendarService(ILogger<GoogleCalendarService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SyncAppointmentAsync(string title, DateTime startTime, DateTime endTime, string description, string? configJson = null)
    {
        if (string.IsNullOrEmpty(configJson))
        {
            _logger.LogWarning("Google Calendar Sync: No configuration provided.");
            return false;
        }

        try
        {
            var config = JsonConvert.DeserializeObject<GoogleCalendarConfig>(configJson);
            if (config == null || string.IsNullOrEmpty(config.ClientId) || string.IsNullOrEmpty(config.ClientSecret))
            {
                _logger.LogWarning("Google Calendar Sync: Invalid configuration.");
                return false;
            }

            // Note: In a production environment, we would use a token store (e.g., in the database).
            // For now, we assume the config contains the necessary credentials or tokens.
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = config.ClientId,
                    ClientSecret = config.ClientSecret
                },
                Scopes = new[] { CalendarService.Scope.CalendarEvents }
            });

            TokenResponse? token = null;
            if (!string.IsNullOrEmpty(config.RefreshToken))
            {
                token = new TokenResponse { RefreshToken = config.RefreshToken };
            }
            else if (!string.IsNullOrEmpty(config.AuthCode))
            {
                token = await flow.ExchangeCodeForTokenAsync("user", config.AuthCode, "postmessage", default);
                // In a real app, we would save the RefreshToken back to the config here.
            }

            if (token == null)
            {
                _logger.LogWarning("Google Calendar Sync: No token or auth code available.");
                return false;
            }

            var credential = new UserCredential(flow, "user", token);
            
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "RandevuTakip",
            });

            var ev = new Event
            {
                Summary = title,
                Description = description,
                Start = new EventDateTime { DateTimeDateTimeOffset = startTime },
                End = new EventDateTime { DateTimeDateTimeOffset = endTime },
                Reminders = new Event.RemindersData { UseDefault = true }
            };

            var request = service.Events.Insert(ev, "primary");
            var createdEvent = await request.ExecuteAsync();

            _logger.LogInformation("Google Calendar Sync: Event created. ID: {Id}", createdEvent.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Calendar Sync: Error syncing appointment.");
            return false;
        }
    }

    private class GoogleCalendarConfig
    {
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? AuthCode { get; set; }
        public string? RefreshToken { get; set; }
    }
}
