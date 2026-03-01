using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RandevuTakip.Api.Models;

public class Staff
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Title { get; set; } // Örn: Uzman Diş Hekimi

    [MaxLength(100)]
    public string? Email { get; set; }

    public string? Bio { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(50)]
    public string Role { get; set; } = "Staff"; // Admin, Staff vs.

    public Guid? UserId { get; set; } = null; // Optional link to auth system

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;

    // Navigation properties
    public virtual ICollection<StaffService> StaffServices { get; set; } = new List<StaffService>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<WorkingHours> WorkingHours { get; set; } = new List<WorkingHours>();
}

public class StaffService
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid StaffId { get; set; }

    [Required]
    public Guid ServiceId { get; set; }

    [ForeignKey("StaffId")]
    public virtual Staff Staff { get; set; } = null!;

    [ForeignKey("ServiceId")]
    public virtual Service Service { get; set; } = null!;
}
