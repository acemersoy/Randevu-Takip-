using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RandevuTakip.Api.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? smtpJson = null)
    {
        try
        {
            var host = _configuration["Smtp:Host"];
            var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var enableSsl = bool.Parse(_configuration["Smtp:EnableSsl"] ?? "true");
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var fromName = _configuration["Smtp:FromName"] ?? "BookPilot";
            var fromEmail = _configuration["Smtp:FromEmail"];

            if (!string.IsNullOrEmpty(smtpJson) && smtpJson != "{}" && smtpJson != "[]")
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(smtpJson);
                    var r = doc.RootElement;
                    if (r.TryGetProperty("host", out var h) && !string.IsNullOrEmpty(h.GetString())) host = h.GetString();
                    if (r.TryGetProperty("port", out var p) && p.ValueKind == System.Text.Json.JsonValueKind.Number) port = p.GetInt32();
                    if (r.TryGetProperty("enableSsl", out var ssl) && (ssl.ValueKind == System.Text.Json.JsonValueKind.True || ssl.ValueKind == System.Text.Json.JsonValueKind.False)) enableSsl = ssl.GetBoolean();
                    if (r.TryGetProperty("username", out var u) && !string.IsNullOrEmpty(u.GetString())) username = u.GetString();
                    if (r.TryGetProperty("password", out var pwd) && !string.IsNullOrEmpty(pwd.GetString())) password = pwd.GetString();
                    if (r.TryGetProperty("fromName", out var fn) && !string.IsNullOrEmpty(fn.GetString())) fromName = fn.GetString();
                    if (r.TryGetProperty("fromEmail", out var fe) && !string.IsNullOrEmpty(fe.GetString())) fromEmail = fe.GetString();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "SmtpJson parse failed, falling back to global settings");
                }
            }

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fromEmail))
            {
                _logger.LogWarning("SMTP ayarları tam olarak yapılandırılmamış. Mail gönderilmedi: {ToEmail}", toEmail);
                return;
            }

            using var client = new SmtpClient(host, port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Mail başarıyla gönderildi: {ToEmail}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mail gönderilirken bir hata oluştu: {ToEmail}", toEmail);
        }
    }
}
