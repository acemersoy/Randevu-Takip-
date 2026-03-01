using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RandevuTakip.Api.Data;
using RandevuTakip.Api.Models;
using System.Security.Claims;
using RandevuTakip.Api.Services;

namespace RandevuTakip.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public AdminController(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    private async Task LogActionAsync(string action, string entityName, string? entityId, string? details = null)
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim)) return;

        var tenantId = Guid.Parse(tenantIdClaim);
        var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value 
                        ?? User.FindFirst(ClaimTypes.Name)?.Value 
                        ?? "unknown";

        _context.AuditLogs.Add(new AuditLog
        {
            TenantId = tenantId,
            AdminEmail = adminEmail,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details
        });
        await _context.SaveChangesAsync();
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointments([FromQuery] DateTime? date)
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        var query = _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Staff)
            .Where(a => a.TenantId == tenantId);

        if (role == "Staff")
        {
            var staffIdStr = User.FindFirst("StaffId")?.Value;
            if (Guid.TryParse(staffIdStr, out var staffId))
            {
                query = query.Where(a => a.StaffId == staffId);
            }
        }

        if (date.HasValue)
        {
            var start = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
            var end = start.AddDays(1);
            query = query.Where(a => a.StartAt >= start && a.StartAt < end);
        }

        var appointments = await query
            .OrderByDescending(a => a.StartAt)
            .Select(a => new
            {
                a.Id,
                a.CustomerName,
                a.CustomerPhone,
                a.CustomerEmail,
                a.StartAt,
                a.EndAt,
                ServiceName = a.Service.Name,
                a.Status,
                a.ExtraJson,
                a.CreatedAt
            })
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpPut("appointments/{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        var query = _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Tenant)
            .Where(a => a.Id == id && a.TenantId == tenantId);

        if (role == "Staff")
        {
            var staffIdStr = User.FindFirst("StaffId")?.Value;
            if (Guid.TryParse(staffIdStr, out var staffId))
            {
                query = query.Where(a => a.StaffId == staffId);
            }
        }

        var appointment = await query.FirstOrDefaultAsync();

        if (appointment == null) return NotFound();

        appointment.Status = request.Status;
        await _context.SaveChangesAsync();
        await LogActionAsync("UpdateAppointmentStatus", "Appointment", appointment.Id.ToString(), $"New Status: {request.Status}");

        if (request.Status == "Confirmed" && !string.IsNullOrEmpty(appointment.CustomerEmail))
        {
            var dateStr = appointment.StartAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
            var subject = $"{appointment.Tenant.Name} - Randevunuz Onaylandı ✅";
            var body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                    <h2 style='color: #059669;'>Harika Haber, {appointment.CustomerName}!</h2>
                    <p><strong>{appointment.Tenant.Name}</strong> işletmesindeki randevunuz onaylanmıştır.</p>
                    <p>Randevu Detaylarınız:</p>
                    <ul>
                        <li><strong>Hizmet:</strong> {appointment.Service.Name}</li>
                        <li><strong>Tarih / Saat:</strong> {dateStr}</li>
                    </ul>
                    <p>Sizi zamanında görmekten mutluluk duyacağız. Lütfen randevu saatinizden 10 dakika önce orada bulunmaya özen gösterin.</p>
                    <hr style='border: 1px solid #eee; my-4;' />
                    <p style='font-size: 12px; color: #888;'>Bu mail otomatik olarak gönderilmiştir. Lütfen cevaplamayınız.</p>
                </div>";

            _ = _emailService.SendEmailAsync(appointment.CustomerEmail, subject, body, appointment.Tenant.SmtpJson);
        }

        return Ok(new { Message = "Durum başarıyla güncellendi." });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        Guid? staffId = null;
        if (role == "Staff")
        {
            var staffIdStr = User.FindFirst("StaffId")?.Value;
            if (Guid.TryParse(staffIdStr, out var parsedId))
                staffId = parsedId;
        }

        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var weekStart = today.AddDays(-(int)today.DayOfWeek + 1); // Pazartesi

        IQueryable<Appointment> baseAppQuery = _context.Appointments.Where(a => a.TenantId == tenantId);
        if (staffId.HasValue) baseAppQuery = baseAppQuery.Where(a => a.StaffId == staffId.Value);

        // Temel sayaçlar
        var todayCount = await baseAppQuery.CountAsync(a => a.StartAt >= today && a.StartAt < today.AddDays(1));
        var pendingCount = await baseAppQuery.CountAsync(a => a.Status == "Pending");
        var totalCount = await baseAppQuery.CountAsync();
        var serviceCount = await _context.Services.CountAsync(s => s.TenantId == tenantId && s.IsActive);

        // Haftalık günlük sayılar (son 7 gün)
        var weeklyData = new List<object>();
        for (int i = 6; i >= 0; i--)
        {
            var day = today.AddDays(-i);
            var dayEnd = day.AddDays(1);
            var count = await baseAppQuery.CountAsync(a => a.StartAt >= day && a.StartAt < dayEnd && a.Status != "Cancelled");
            weeklyData.Add(new { Date = day.ToString("ddd dd/MM"), Count = count });
        }

        // Bu ayki gelir (Confirmed + Completed randevular)
        var monthStart = DateTime.SpecifyKind(new DateTime(today.Year, today.Month, 1), DateTimeKind.Utc);
        var monthRevenue = await baseAppQuery
            .Where(a => a.StartAt >= monthStart && (a.Status == "Confirmed" || a.Status == "Completed"))
            .Include(a => a.Service)
            .SumAsync(a => (decimal?)a.Service.Price) ?? 0;

        // En popüler 5 hizmet
        var popularServices = await baseAppQuery
            .Where(a => a.Status != "Cancelled")
            .Include(a => a.Service)
            .GroupBy(a => new { a.ServiceId, a.Service.Name })
            .Select(g => new { Name = g.Key.Name, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        // Yaklaşan randevular (bugün + yarın, max 8)
        var upcomingEnd = today.AddDays(2);
        var upcoming = await baseAppQuery
            .Where(a => a.StartAt >= DateTime.UtcNow && a.StartAt < upcomingEnd && a.Status != "Cancelled")
            .Include(a => a.Service)
            .Include(a => a.Staff)
            .OrderBy(a => a.StartAt)
            .Take(8)
            .Select(a => new
            {
                a.Id,
                a.CustomerName,
                a.CustomerPhone,
                a.StartAt,
                a.EndAt,
                ServiceName = a.Service.Name,
                StaffName = a.Staff != null ? a.Staff.Name : null,
                a.Status
            })
            .ToListAsync();

        return Ok(new
        {
            TodayCount = todayCount,
            PendingCount = pendingCount,
            TotalCount = totalCount,
            ServiceCount = serviceCount,
            MonthRevenue = monthRevenue,
            WeeklyData = weeklyData,
            PopularServices = popularServices,
            UpcomingAppointments = upcoming
        });
    }

    [Authorize(Roles = "Owner")]
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers()
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);

        // Randevulardan unique müşteri listesini çıkar
        var customers = await _context.Appointments
            .Where(a => a.TenantId == tenantId)
            .GroupBy(a => new { a.CustomerPhone, a.CustomerName, a.CustomerEmail })
            .Select(g => new
            {
                Name = g.Key.CustomerName,
                Phone = g.Key.CustomerPhone,
                Email = g.Key.CustomerEmail,
                TotalAppointments = g.Count(),
                LastAppointment = g.Max(a => a.StartAt),
                // İsteğe bağlı: En son aldığı hizmetin adı da eklenebilir ama Join gerektirir.
                // Basitlik için sadece sayı ve tarih dönüyoruz.
            })
            .OrderByDescending(c => c.LastAppointment)
            .ToListAsync();

        return Ok(customers);
    }

    [Authorize(Roles = "Owner")]
    [HttpGet("reports")]
    public async Task<IActionResult> GetReports()
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);

        // Gelir Özeti (Bu Ay, Geçen Ay, Toplam)
        var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        var thisMonthStart = DateTime.SpecifyKind(new DateTime(today.Year, today.Month, 1), DateTimeKind.Utc);
        var lastMonthStart = thisMonthStart.AddMonths(-1);

        var allCompleted = await _context.Appointments
            .Where(a => a.TenantId == tenantId && (a.Status == "Confirmed" || a.Status == "Completed"))
            .Include(a => a.Service)
            .ToListAsync();

        var thisMonthRevenue = allCompleted.Where(a => a.StartAt >= thisMonthStart).Sum(a => a.Service.Price);
        var lastMonthRevenue = allCompleted.Where(a => a.StartAt >= lastMonthStart && a.StartAt < thisMonthStart).Sum(a => a.Service.Price);
        var totalRevenue = allCompleted.Sum(a => a.Service.Price);

        // Hizmet Bazlı Performans
        var servicePerformance = allCompleted
            .GroupBy(a => new { a.ServiceId, a.Service.Name })
            .Select(g => new
            {
                ServiceName = g.Key.Name,
                AppointmentCount = g.Count(),
                Revenue = g.Sum(a => a.Service.Price)
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        return Ok(new
        {
            ThisMonthRevenue = thisMonthRevenue,
            LastMonthRevenue = lastMonthRevenue,
            TotalRevenue = totalRevenue,
            ServicePerformance = servicePerformance
        });
    }

    // --- SERVICE MANAGEMENT ---

    [HttpGet("services")]
    public async Task<IActionResult> GetServices()
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
        var services = await _context.Services
            .Include(s => s.StaffServices)
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.Name)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Description,
                s.DurationMinutes,
                s.Price,
                s.IsActive,
                StaffIds = s.StaffServices.Select(ss => ss.StaffId).ToList()
            })
            .ToListAsync();

        return Ok(services);
    }

    [Authorize(Roles = "Owner")]
    [HttpPost("services")]
    public async Task<IActionResult> CreateService([FromBody] ServiceRequest request)
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);

        var service = new Service
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            DurationMinutes = request.DurationMinutes,
            Price = request.Price,
            IsActive = true
        };

        _context.Services.Add(service);

        if (request.StaffIds != null && request.StaffIds.Any())
        {
            foreach (var staffId in request.StaffIds)
            {
                _context.StaffServices.Add(new StaffService
                {
                    Id = Guid.NewGuid(),
                    ServiceId = service.Id,
                    StaffId = staffId
                });
            }
        }

        await _context.SaveChangesAsync();
        await LogActionAsync("CreateService", "Service", service.Id.ToString(), $"Name: {service.Name}");

        return Ok(service);
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("services/{id}")]
    public async Task<IActionResult> UpdateService(Guid id, [FromBody] ServiceRequest request)
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

        if (service == null) return NotFound();

        service.Name = request.Name;
        service.Description = request.Description;
        service.DurationMinutes = request.DurationMinutes;
        service.Price = request.Price;
        service.IsActive = request.IsActive;

        var existingStaffServices = _context.StaffServices.Where(ss => ss.ServiceId == id);
        _context.StaffServices.RemoveRange(existingStaffServices);

        if (request.StaffIds != null && request.StaffIds.Any())
        {
            foreach (var staffId in request.StaffIds)
            {
                _context.StaffServices.Add(new StaffService
                {
                    Id = Guid.NewGuid(),
                    ServiceId = service.Id,
                    StaffId = staffId
                });
            }
        }

        await _context.SaveChangesAsync();
        await LogActionAsync("UpdateService", "Service", service.Id.ToString(), $"Name: {service.Name}");

        return Ok(service);
    }

    [Authorize(Roles = "Owner")]
    [HttpDelete("services/{id}")]
    public async Task<IActionResult> DeleteService(Guid id)
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

        if (service == null) return NotFound();

        // Soft delete logic could be here, but let's do hard delete if no appointments exist, or just toggle IsActive
        var hasAppointments = await _context.Appointments.AnyAsync(a => a.ServiceId == id);
        if (hasAppointments)
        {
            service.IsActive = false;
            await _context.SaveChangesAsync();
            await LogActionAsync("SoftDeleteService", "Service", service.Id.ToString(), $"Name: {service.Name}");
            return Ok(new { Message = "Servis aktif randevuları olduğu için sadece pasife alındı." });
        }

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
        await LogActionAsync("HardDeleteService", "Service", service.Id.ToString(), $"Name: {service.Name}");

        return Ok(new { Message = "Servis başarıyla silindi." });
    }

    // --- WORKING HOURS MANAGEMENT ---

    [HttpGet("working-hours")]
    public async Task<IActionResult> GetWorkingHours([FromQuery] Guid? staffId)
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
        var hours = await _context.WorkingHours
            .Where(w => w.TenantId == tenantId && w.StaffId == staffId)
            .OrderBy(w => w.DayOfWeek)
            .ToListAsync();

        return Ok(hours);
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("working-hours")]
    public async Task<IActionResult> UpdateWorkingHours([FromBody] List<WorkingHoursRequest> request, [FromQuery] Guid? staffId)
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);

        foreach (var item in request)
        {
            var dbItem = await _context.WorkingHours
                .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.DayOfWeek == item.DayOfWeek && w.StaffId == staffId);

            if (dbItem != null)
            {
                dbItem.OpenTime = TimeSpan.Parse(item.OpenTime);
                dbItem.CloseTime = TimeSpan.Parse(item.CloseTime);
                dbItem.IsClosed = item.IsClosed;
                dbItem.SlotStepMinutes = item.SlotStepMinutes;
            }
            else
            {
                _context.WorkingHours.Add(new WorkingHours
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    StaffId = staffId,
                    DayOfWeek = item.DayOfWeek,
                    OpenTime = TimeSpan.Parse(item.OpenTime),
                    CloseTime = TimeSpan.Parse(item.CloseTime),
                    IsClosed = item.IsClosed,
                    SlotStepMinutes = item.SlotStepMinutes
                });
            }
        }

        await _context.SaveChangesAsync();
        await LogActionAsync("UpdateWorkingHours", "WorkingHours", staffId?.ToString() ?? "General");
        return Ok(new { Message = "Çalışma saatleri güncellendi." });
    }

    // --- TENANT SETTINGS MANAGEMENT ---

    [Authorize(Roles = "Owner")]
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null) return NotFound();

        return Ok(new
        {
            tenant.Name,
            tenant.Industry,
            tenant.ThemeJson,
            tenant.BookingFormSchema,
            tenant.SmtpJson
        });
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] SettingsRequest request)
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null) return NotFound();

        tenant.Name = request.Name;
        tenant.Industry = request.Industry;
        tenant.ThemeJson = request.ThemeJson;
        tenant.BookingFormSchema = request.BookingFormSchema;
        tenant.SmtpJson = request.SmtpJson;

        await _context.SaveChangesAsync();
        await LogActionAsync("UpdateSettings", "Tenant", tenantId.ToString());
        return Ok(new { Message = "Ayarlar başarıyla güncellendi." });
    }

    // --- STAFF MANAGEMENT ---

    [Authorize(Roles = "Owner")]
    [HttpGet("staff")]
    public async Task<IActionResult> GetStaff()
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
        var staff = await _context.Staff
            .Include(s => s.StaffServices)
            .ThenInclude(ss => ss.Service)
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return Ok(staff.Select(s => new
        {
            s.Id,
            s.Name,
            s.Title,
            s.Email,
            s.Bio,
            s.ProfilePictureUrl,
            s.IsActive,
            Services = s.StaffServices.Select(ss => new { ss.Service.Id, ss.Service.Name })
        }));
    }

    [Authorize(Roles = "Owner")]
    [HttpPost("staff")]
    public async Task<IActionResult> CreateStaff([FromBody] StaffRequest request)
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);

        var staff = new Staff
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Title = request.Title,
            Email = request.Email,
            Bio = request.Bio,
            IsActive = true
        };

        _context.Staff.Add(staff);

        if (request.ServiceIds != null)
        {
            foreach (var serviceId in request.ServiceIds)
            {
                _context.StaffServices.Add(new StaffService
                {
                    Id = Guid.NewGuid(),
                    StaffId = staff.Id,
                    ServiceId = serviceId
                });
            }
        }

        await _context.SaveChangesAsync();
        await LogActionAsync("CreateStaff", "Staff", staff.Id.ToString(), $"Name: {staff.Name}");
        return Ok(staff);
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("staff/{id}")]
    public async Task<IActionResult> UpdateStaff(Guid id, [FromBody] StaffRequest request)
    {
        var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value!);
        var staff = await _context.Staff
            .Include(s => s.StaffServices)
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

        if (staff == null) return NotFound();

        staff.Name = request.Name;
        staff.Title = request.Title;
        staff.Email = request.Email;
        staff.Bio = request.Bio;
        staff.IsActive = request.IsActive;

        // Update services
        var existingServices = _context.StaffServices.Where(ss => ss.StaffId == id);
        _context.StaffServices.RemoveRange(existingServices);

        if (request.ServiceIds != null)
        {
            foreach (var serviceId in request.ServiceIds)
            {
                _context.StaffServices.Add(new StaffService
                {
                    Id = Guid.NewGuid(),
                    StaffId = staff.Id,
                    ServiceId = serviceId
                });
            }
        }

        await _context.SaveChangesAsync();
        await LogActionAsync("UpdateStaff", "Staff", staff.Id.ToString(), $"Name: {staff.Name}");
        return Ok(staff);
    }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class ServiceRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Guid>? StaffIds { get; set; }
}

public class WorkingHoursRequest
{
    public DayOfWeek DayOfWeek { get; set; }
    public string OpenTime { get; set; } = "09:00";
    public string CloseTime { get; set; } = "18:00";
    public bool IsClosed { get; set; }
    public int SlotStepMinutes { get; set; } = 30;
}

public class SettingsRequest
{
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string ThemeJson { get; set; } = "{}";
    public string BookingFormSchema { get; set; } = "[]";
    public string? SmtpJson { get; set; }
}

public class StaffRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? Bio { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Guid>? ServiceIds { get; set; }
}
