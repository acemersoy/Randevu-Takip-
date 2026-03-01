using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RandevuTakip.Api.Data;
using RandevuTakip.Api.Services;

namespace RandevuTakip.Api.Controllers;

[ApiController]
[Route("api/{slug}")]
public class PublicController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IBookingService _bookingService;

    public PublicController(AppDbContext context, IBookingService bookingService)
    {
        _context = context;
        _bookingService = bookingService;
    }

    [HttpGet("tenant")]
    public async Task<IActionResult> GetTenant([FromRoute] string slug)
    {
        var tenant = await _context.Tenants
            .Select(t => new
            {
                t.Id,
                t.Slug,
                t.Name,
                t.Industry,
                t.ThemeJson,
                t.BookingFormSchema
            })
            .FirstOrDefaultAsync(t => t.Slug.ToLower() == slug.ToLower());

        if (tenant == null)
            return NotFound(new { Message = $"İşletme ({slug}) bulunamadı." });

        return Ok(tenant);
    }

    [HttpGet("services")]
    public async Task<IActionResult> GetServices([FromRoute] string slug)
    {
        var tenantId = await _context.Tenants
            .Where(t => t.Slug.ToLower() == slug.ToLower())
            .Select(t => t.Id)
            .FirstOrDefaultAsync();

        if (tenantId == Guid.Empty)
            return NotFound(new { Message = $"İşletme ({slug}) bulunamadı." });

        var services = await _context.Services
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Description,
                s.DurationMinutes,
                s.Price
            })
            .ToListAsync();

        return Ok(services);
    }

    [HttpGet("staff")]
    public async Task<IActionResult> GetStaff([FromRoute] string slug, [FromQuery] Guid? serviceId)
    {
        var tenantId = await _context.Tenants
            .Where(t => t.Slug.ToLower() == slug.ToLower())
            .Select(t => t.Id)
            .FirstOrDefaultAsync();

        if (tenantId == Guid.Empty)
            return NotFound();

        var query = _context.Staff
            .Where(s => s.TenantId == tenantId && s.IsActive);

        if (serviceId.HasValue)
        {
            query = query.Where(s => s.StaffServices.Any(ss => ss.ServiceId == serviceId.Value));
        }

        var staffList = await query
            .OrderBy(s => s.Name)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Title,
                s.Bio,
                s.ProfilePictureUrl
            })
            .ToListAsync();

        return Ok(staffList);
    }

    [HttpGet("availability")]
    public async Task<IActionResult> GetAvailability(string slug, [FromQuery] Guid serviceId, [FromQuery] DateTime date, [FromQuery] Guid? staffId)
    {
        var slots = await _bookingService.GetAvailableSlotsAsync(slug, serviceId, date, staffId);
        return Ok(slots);
    }

    [HttpPost("appointments")]
    public async Task<IActionResult> CreateAppointment(string slug, [FromBody] CreateAppointmentRequest request)
    {
        var appointment = await _bookingService.CreateAppointmentAsync(slug, request);
        if (appointment == null)
            return BadRequest(new { Message = "Seçilen slot artık müsait değil veya geçersiz." });

        return Ok(new { appointment.Id, Message = "Randevunuz başarıyla oluşturuldu." });
    }
}
