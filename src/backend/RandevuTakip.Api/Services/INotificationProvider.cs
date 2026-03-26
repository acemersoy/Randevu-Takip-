using System.Threading.Tasks;

namespace RandevuTakip.Api.Services;

public interface INotificationProvider
{
    string ProviderName { get; }
    Task<bool> SendSmsAsync(string phoneNumber, string message, string? configJson = null);
    Task<bool> SendWhatsAppAsync(string phoneNumber, string message, string? configJson = null);
}
