using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace RandevuTakip.Api.Services;

public class ZoomService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ZoomService> _logger;

    public ZoomService(HttpClient httpClient, ILogger<ZoomService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ZoomMeetingResult?> CreateMeetingAsync(string topic, DateTime startTime, int durationMinutes, string? configJson)
    {
        if (string.IsNullOrEmpty(configJson))
        {
            _logger.LogWarning("Zoom Service: No configuration provided.");
            return null;
        }

        try
        {
            var config = JsonConvert.DeserializeObject<ZoomConfig>(configJson);
            if (config == null || string.IsNullOrEmpty(config.ClientId) || string.IsNullOrEmpty(config.ClientSecret) || string.IsNullOrEmpty(config.AccountId))
            {
                _logger.LogWarning("Zoom Service: Invalid configuration.");
                return null;
            }

            var accessToken = await GetAccessTokenAsync(config);
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Zoom Service: Failed to get access token.");
                return null;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.zoom.us/v2/users/me/meetings");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var meetingData = new
            {
                topic = topic,
                type = 2, // Scheduled meeting
                start_time = startTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                duration = durationMinutes,
                timezone = "UTC",
                settings = new
                {
                    host_video = true,
                    participant_video = true,
                    join_before_host = false,
                    mute_upon_entry = true,
                    waiting_room = true
                }
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(meetingData), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Zoom Service: Error creating meeting. Status: {Status}, Error: {Error}", response.StatusCode, error);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var meetingResponse = JsonConvert.DeserializeObject<ZoomMeetingResponse>(responseContent);

            _logger.LogInformation("Zoom Service: Meeting created. URL: {Url}", meetingResponse?.JoinUrl);

            return new ZoomMeetingResult
            {
                MeetingId = meetingResponse?.Id.ToString(),
                JoinUrl = meetingResponse?.JoinUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Zoom Service: Exception occurred while creating meeting.");
            return null;
        }
    }

    private async Task<string?> GetAccessTokenAsync(ZoomConfig config)
    {
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.ClientId}:{config.ClientSecret}"));
        var request = new HttpRequestMessage(HttpMethod.Post, $"https://zoom.us/oauth/token?grant_type=account_credentials&account_id={config.AccountId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
        return tokenResponse?["access_token"];
    }

    private class ZoomConfig
    {
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? AccountId { get; set; }
    }

    private class ZoomMeetingResponse
    {
        public long Id { get; set; }
        [JsonProperty("join_url")]
        public string? JoinUrl { get; set; }
    }
}

public class ZoomMeetingResult
{
    public string? MeetingId { get; set; }
    public string? JoinUrl { get; set; }
}
