using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RandevuTakip.Api.Models;

public class WorkingHours
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    public Guid? StaffId { get; set; } // Null ise tenant genel çalışma saati

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeSpan OpenTime { get; set; }

    [Required]
    public TimeSpan CloseTime { get; set; }

    [Required]
    public int SlotStepMinutes { get; set; } = 30; // Örn: Her 30 dakikada bir randevu

    public bool IsClosed { get; set; } = false;

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [ForeignKey("StaffId")]
    public virtual Staff? Staff { get; set; }
}

public class Appointment
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public Guid ServiceId { get; set; }

    public Guid? StaffId { get; set; } // Randevu hangi personele alındı

    [Required]
    public DateTime StartAt { get; set; }

    [Required]
    public DateTime EndAt { get; set; }

    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string CustomerPhone { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CustomerEmail { get; set; }

    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

    public string ExtraJson { get; set; } = "{}"; // Dinamik form verileri buraya

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    [ForeignKey("ServiceId")]
    public virtual Service Service { get; set; } = null!;

    [ForeignKey("StaffId")]
    public virtual Staff? Staff { get; set; }
}
