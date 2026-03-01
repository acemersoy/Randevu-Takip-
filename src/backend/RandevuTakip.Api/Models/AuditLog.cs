using System.ComponentModel.DataAnnotations;

namespace RandevuTakip.Api.Models;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid TenantId { get; set; }
    
    public string? AdminEmail { get; set; }
    
    [Required]
    public string Action { get; set; } = string.Empty;
    
    public string? EntityName { get; set; }
    
    public string? EntityId { get; set; }
    
    public string? Details { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
