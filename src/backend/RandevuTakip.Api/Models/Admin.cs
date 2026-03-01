using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RandevuTakip.Api.Models;

public class Admin
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "Staff"; // Owner, Staff

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; } = null!;
}
