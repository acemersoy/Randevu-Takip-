using System.Threading.Tasks;

namespace RandevuTakip.Api.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? smtpJson = null);
}
