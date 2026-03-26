using System.ComponentModel.DataAnnotations;

namespace RandevuTakip.Api.Models;

public class Tenant
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Industry { get; set; } = "General";

    public string ThemeJson { get; set; } = "{}";

    public string BookingFormSchema { get; set; } = "[]";

    public string? SmtpJson { get; set; } // { "host": "smtp.gmail.com", "port": 587, "enableSsl": true, "username": "...", "password": "...", "fromName": "...", "fromEmail": "..." }

    public string? NotificationConfigJson { get; set; } // { "twilioSid": "...", "twilioAuthToken": "...", "fromNumber": "..." }

    public string? GoogleCalendarConfigJson { get; set; } // { "clientId": "...", "clientSecret": "...", "authCode": "..." }
 
    public string? ZoomConfigJson { get; set; } // { "clientId": "...", "clientSecret": "...", "accountId": "..." }
 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation property
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
