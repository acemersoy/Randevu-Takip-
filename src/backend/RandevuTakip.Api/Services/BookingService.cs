using System.Linq;
using Microsoft.EntityFrameworkCore;
using RandevuTakip.Api.Data;
using RandevuTakip.Api.Models;
using StackExchange.Redis;

namespace RandevuTakip.Api.Services;

public interface IBookingService
{
    Task<List<SlotResponse>> GetAvailableSlotsAsync(string slug, Guid serviceId, DateTime date, Guid? staffId = null);
    Task<Appointment?> CreateAppointmentAsync(string slug, CreateAppointmentRequest request);
}

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly INotificationProvider _notificationProvider;
    private readonly GoogleCalendarService _googleCalendarService;
    private readonly ZoomService _zoomService;
    private readonly IConnectionMultiplexer _redis;

    public BookingService(
        AppDbContext context, 
        IEmailService emailService, 
        INotificationProvider notificationProvider,
        GoogleCalendarService googleCalendarService,
        ZoomService zoomService,
        IConnectionMultiplexer redis)
    {
        _context = context;
        _emailService = emailService;
        _notificationProvider = notificationProvider;
        _googleCalendarService = googleCalendarService;
        _zoomService = zoomService;
        _redis = redis;
    }

    public async Task<List<SlotResponse>> GetAvailableSlotsAsync(string slug, Guid serviceId, DateTime date, Guid? staffId = null)
    {
        var cacheKey = $"Avail_{slug}_{serviceId}_{date:yyyyMMdd}_{staffId}";
        var db = _redis.GetDatabase();

        // 1. Cache Check
        var cached = await db.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<SlotResponse>>(cached!) ?? new List<SlotResponse>();
        }

        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Slug.ToLower() == slug.ToLower());
        if (tenant == null) return new List<SlotResponse>();

        var service = await _context.Services.FindAsync(serviceId);
        if (service == null || service.TenantId != tenant.Id) return new List<SlotResponse>();

        // Bu servisi veren personelleri bul
        var qualifiedStaffIds = await _context.StaffServices
            .Where(ss => ss.ServiceId == serviceId)
            .Select(ss => ss.StaffId)
            .ToListAsync();

        if (staffId.HasValue && !qualifiedStaffIds.Contains(staffId.Value))
            return new List<SlotResponse>(); // Seçilen personel bu servisi vermiyor

        var dayOfWeek = date.DayOfWeek;

        // Başlangıç ve bitiş saatlerini belirle (Personel saatleri öncelikli)
        var staffWorkingHours = await _context.WorkingHours
            .Where(w => w.TenantId == tenant.Id && w.DayOfWeek == dayOfWeek && !w.IsClosed)
            .ToListAsync();

        WorkingHours? activeWorkingHours = null;
        if (staffId.HasValue)
        {
            activeWorkingHours = staffWorkingHours.FirstOrDefault(w => w.StaffId == staffId.Value) 
                                 ?? staffWorkingHours.FirstOrDefault(w => !w.StaffId.HasValue);
        }
        else
        {
            activeWorkingHours = staffWorkingHours.FirstOrDefault(w => !w.StaffId.HasValue);
        }

        if (activeWorkingHours == null) return new List<SlotResponse>();

        // Npgsql 6+ DateTimeKind.Unspecified kabul etmiyor — UTC'ye çevir
        var dayUtc = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var startOfDay = dayUtc;
        var endOfDay = dayUtc.AddDays(1);

        // Mevcut randevuları çek
        var appointments = await _context.Appointments
            .Where(a => a.TenantId == tenant.Id && a.StartAt >= startOfDay && a.StartAt < endOfDay && a.Status != "Cancelled")
            .ToListAsync();

        var slots = new List<SlotResponse>();
        
        // Tenant'ın genel açık/kapalı sınırlarını bulmak için tüm personellerin mesailerini birleştirip min OpenTime / max CloseTime alalım
        var defaultHours = staffWorkingHours.FirstOrDefault(w => !w.StaffId.HasValue);
        TimeSpan minOpenTime = defaultHours?.OpenTime ?? new TimeSpan(9, 0, 0);
        TimeSpan maxCloseTime = defaultHours?.CloseTime ?? new TimeSpan(18, 0, 0);

        if (staffWorkingHours.Any(w => w.StaffId.HasValue && !w.IsClosed))
        {
             minOpenTime = staffWorkingHours.Where(w => w.StaffId.HasValue && !w.IsClosed).Min(w => w.OpenTime);
             maxCloseTime = staffWorkingHours.Where(w => w.StaffId.HasValue && !w.IsClosed).Max(w => w.CloseTime);
        }

        var slotStep = activeWorkingHours?.SlotStepMinutes ?? 30;
        var currentTime = minOpenTime;

        while (currentTime.Add(TimeSpan.FromMinutes(service.DurationMinutes)) <= maxCloseTime)
        {
            var slotStart = dayUtc.Add(currentTime);
            var slotEnd = slotStart.AddMinutes(service.DurationMinutes);

            bool isAvailable = false;

            if (staffId.HasValue)
            {
                // Spesifik personel için çakışma kontrolü + O personelin o saatte vardiyası var mı?
                var sHours = staffWorkingHours.FirstOrDefault(w => w.StaffId == staffId.Value);
                // Eğer sHours null ise (personel bazlı ayar girilmemişse), defaultHours kullan
                var hoursToCheck = sHours ?? defaultHours;
                bool inShift = hoursToCheck != null && !hoursToCheck.IsClosed && 
                               (currentTime >= hoursToCheck.OpenTime && currentTime.Add(TimeSpan.FromMinutes(service.DurationMinutes)) <= hoursToCheck.CloseTime);

                isAvailable = inShift && !appointments.Any(a => 
                    a.StaffId == staffId.Value &&
                    ((slotStart >= a.StartAt && slotStart < a.EndAt) || (slotEnd > a.StartAt && slotEnd <= a.EndAt))
                );
            }
            else
            {
                // HERHANGİ bir müsait personel var mı? (Vardiyası o saate uyan ve randevusu olmayan)
                foreach (var sId in qualifiedStaffIds)
                {
                    var sHours = staffWorkingHours.FirstOrDefault(w => w.StaffId == sId);
                    var hoursToCheck = sHours ?? defaultHours;
                    bool inShift = hoursToCheck != null && !hoursToCheck.IsClosed && 
                                   (currentTime >= hoursToCheck.OpenTime && currentTime.Add(TimeSpan.FromMinutes(service.DurationMinutes)) <= hoursToCheck.CloseTime);

                    if (!inShift) continue;

                    bool staffBusy = appointments.Any(a =>
                        a.StaffId == sId &&
                        ((slotStart >= a.StartAt && slotStart < a.EndAt) || (slotEnd > a.StartAt && slotEnd <= a.EndAt))
                    );

                    if (!staffBusy)
                    {
                        isAvailable = true;
                        break;
                    }
                }
            }

            if (isAvailable && slotStart > DateTime.UtcNow)
            {
                slots.Add(new SlotResponse
                {
                    Time = currentTime.ToString(@"hh\:mm"),
                    IsAvailable = true
                });
            }

            currentTime = currentTime.Add(TimeSpan.FromMinutes(slotStep));
        }

        // Cache the result for 60 seconds
        await db.StringSetAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(slots), TimeSpan.FromSeconds(60));

        return slots;
    }

    public async Task<Appointment?> CreateAppointmentAsync(string slug, CreateAppointmentRequest request)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Slug.ToLower() == slug.ToLower());
        if (tenant == null) return null;

        var service = await _context.Services.FindAsync(request.ServiceId);
        if (service == null || service.TenantId != tenant.Id) return null;

        var slotDay = DateTime.SpecifyKind(request.SlotDate.Date, DateTimeKind.Utc);
        var slotTime = TimeSpan.Parse(request.SlotTime);
        var startAt = slotDay.Add(slotTime);
        var endAt = startAt.AddMinutes(service.DurationMinutes);

        // Personel listesini ve çalışma saatlerini getir
        var staffWorkingHours = await _context.WorkingHours
            .Where(w => w.TenantId == tenant.Id && w.DayOfWeek == startAt.DayOfWeek)
            .ToListAsync();

        var defaultHours = staffWorkingHours.FirstOrDefault(w => !w.StaffId.HasValue);

        Guid? assignedStaffId = request.StaffId;

        if (!assignedStaffId.HasValue)
        {
            var qualifiedStaffIds = await _context.StaffServices
                .Where(ss => ss.ServiceId == service.Id)
                .Select(ss => ss.StaffId)
                .ToListAsync();

            // Müsait personelleri bul ve randevu sayılarını say (Load Balancing)
            var availableStaffWithCounts = new List<(Guid StaffId, int AppointmentCount)>();

            foreach (var sId in qualifiedStaffIds)
            {
                // Vardiya kontrolü
                var sHours = staffWorkingHours.FirstOrDefault(w => w.StaffId == sId);
                var hoursToCheck = sHours ?? defaultHours;
                
                bool inShift = hoursToCheck != null && !hoursToCheck.IsClosed && 
                               (slotTime >= hoursToCheck.OpenTime && slotTime.Add(TimeSpan.FromMinutes(service.DurationMinutes)) <= hoursToCheck.CloseTime);
                
                if (!inShift) continue;

                // Meşguliyet kontrolü
                bool staffBusy = await _context.Appointments.AnyAsync(a =>
                    a.StaffId == sId && a.Status != "Cancelled" &&
                    ((startAt >= a.StartAt && startAt < a.EndAt) || (endAt > a.StartAt && endAt <= a.EndAt))
                );

                if (!staffBusy)
                {
                    // O günkü randevu sayısını al
                    int dailyCount = await _context.Appointments.CountAsync(a => 
                        a.StaffId == sId && a.Status != "Cancelled" && 
                        a.StartAt >= slotDay && a.StartAt < slotDay.AddDays(1));
                    
                    availableStaffWithCounts.Add((sId, dailyCount));
                }
            }

            // En az randevusu olanı seç (Load Balancing)
            assignedStaffId = availableStaffWithCounts
                .OrderBy(x => x.AppointmentCount)
                .Select(x => (Guid?)x.StaffId)
                .FirstOrDefault();
        }
        else
        {
            // Seçilen personel müsait mi?
            var sHours = staffWorkingHours.FirstOrDefault(w => w.StaffId == assignedStaffId);
            var hoursToCheck = sHours ?? defaultHours;
            
            bool inShift = hoursToCheck != null && !hoursToCheck.IsClosed && 
                           (slotTime >= hoursToCheck.OpenTime && slotTime.Add(TimeSpan.FromMinutes(service.DurationMinutes)) <= hoursToCheck.CloseTime);
            
            if (!inShift) return null;

            bool isBusy = await _context.Appointments.AnyAsync(a =>
                a.StaffId == assignedStaffId && a.Status != "Cancelled" &&
                ((startAt >= a.StartAt && startAt < a.EndAt) || (endAt > a.StartAt && endAt <= a.EndAt))
            );
            if (isBusy) return null;
        }

        if (!assignedStaffId.HasValue) return null; // Müsait kimse yok

        // REDIS ATOMIC SLOT LOCKING
        var redisDb = _redis.GetDatabase();
        var lockKey = $"SlotLock_{tenant.Id}_{assignedStaffId}_{startAt:yyyyMMddHHmm}";
        var lockValue = Guid.NewGuid().ToString();
        
        // 30 saniyelik kilit al
        bool lockAcquired = await redisDb.LockTakeAsync(lockKey, lockValue, TimeSpan.FromSeconds(30));
        
        if (!lockAcquired) return null;

        try 
        {
            // Kilidi aldıktan sonra meşguliyet kontrolünü TEKRARLA
            bool isStillBusy = await _context.Appointments.AnyAsync(a =>
                a.StaffId == assignedStaffId && a.Status != "Cancelled" &&
                ((startAt >= a.StartAt && startAt < a.EndAt) || (endAt > a.StartAt && endAt <= a.EndAt))
            );

            if (isStillBusy) return null;

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                ServiceId = service.Id,
                StaffId = assignedStaffId,
                StartAt = startAt,
                EndAt = endAt,
                CustomerName = request.CustomerName,
                CustomerPhone = request.CustomerPhone,
                CustomerEmail = request.CustomerEmail,
                ExtraJson = request.ExtraJson,
                Status = "Pending"
            };

            _context.Appointments.Add(appointment);
            
            // --- External Integrations ---

            // 1. Zoom Meeting (if online service)
            if (service.IsOnline)
            {
                var zoomResult = await _zoomService.CreateMeetingAsync(
                    $"{service.Name}: {appointment.CustomerName}",
                    startAt,
                    service.DurationMinutes,
                    tenant.ZoomConfigJson
                );

                if (zoomResult != null)
                {
                    appointment.ZoomMeetingUrl = zoomResult.JoinUrl;
                    appointment.ZoomMeetingId = zoomResult.MeetingId;
                }
            }

            await _context.SaveChangesAsync();

            // 2. Google Takvim Senkronizasyonu
            _ = _googleCalendarService.SyncAppointmentAsync(
                $"{request.CustomerName} - {service.Name}",
                startAt,
                endAt,
                $"Müşteri Notu: {request.ExtraJson} {(appointment.ZoomMeetingUrl != null ? "\nZoom Link: " + appointment.ZoomMeetingUrl : "")}",
                tenant.GoogleCalendarConfigJson
            );

            // --- Notifications ---

            // 3. Müşteriye bildirim e-postası
            if (!string.IsNullOrEmpty(request.CustomerEmail))
            {
                var dateStr = startAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
                var zoomHtml = appointment.ZoomMeetingUrl != null 
                    ? $"<p><strong>Zoom Toplantı Linki:</strong> <a href='{appointment.ZoomMeetingUrl}'>{appointment.ZoomMeetingUrl}</a></p>" 
                    : "";

                var subject = $"{tenant.Name} - Randevu Talebiniz Alındı";
                var body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                        <h2 style='color: #4f46e5;'>Merhaba, {request.CustomerName}</h2>
                        <p><strong>{tenant.Name}</strong> işletmesinden randevu talebiniz başarıyla oluşturuldu.</p>
                        <p>Randevu Detaylarınız:</p>
                        <ul>
                            <li><strong>Hizmet:</strong> {service.Name}</li>
                            <li><strong>Tarih / Saat:</strong> {dateStr}</li>
                        </ul>
                        {zoomHtml}
                        <p>Randevunuz işletme yöneticileri tarafından onaylandıktan sonra size tekrar bilgi vereceğiz.</p>
                        <hr style='border: 1px solid #eee; my-4;' />
                        <p style='font-size: 12px; color: #888;'>Bu mail otomatik olarak gönderilmiştir. Lütfen cevaplamayınız.</p>
                    </div>";

                _ = _emailService.SendEmailAsync(request.CustomerEmail, subject, body, tenant.SmtpJson);
            }

            // 4. SMS / WhatsApp Bildirimi (Müşteriye)
            if (!string.IsNullOrEmpty(request.CustomerPhone))
            {
                var zoomSuffix = appointment.ZoomMeetingUrl != null ? $" Zoom: {appointment.ZoomMeetingUrl}" : "";
                var smsMessage = $"{tenant.Name}: Randevunuz alındı ({startAt.ToLocalTime():dd.MM.yyyy HH:mm}).{zoomSuffix} Onay bekliyor.";
                
                // SMS gönder
                _ = _notificationProvider.SendSmsAsync(request.CustomerPhone, smsMessage, tenant.NotificationConfigJson);
                
                // WhatsApp gönder (opsiyonel, konfigürasyona bağlı olarak burada da tetiklenebilir)
                _ = _notificationProvider.SendWhatsAppAsync(request.CustomerPhone, smsMessage, tenant.NotificationConfigJson);
            }

            // 5. Admin bilgilendirme
            var owners = await _context.Admins
                .Where(a => a.TenantId == tenant.Id && a.Role == "Owner")
                .Select(a => a.Email)
                .ToListAsync();

            foreach (var ownerEmail in owners)
            {
                if (!string.IsNullOrEmpty(ownerEmail))
                {
                    var dateStr = startAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
                    var adminSubject = $"YENİ RANDEVU: {service.Name}";
                    var adminBody = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                            <h2 style='color: #4f46e5;'>Yeni Randevu Talebi</h2>
                            <p>İşletmenize yeni bir randevu talebi ulaştı.</p>
                            <ul>
                                <li><strong>Müşteri:</strong> {request.CustomerName} ({request.CustomerPhone})</li>
                                <li><strong>Hizmet:</strong> {service.Name}</li>
                                <li><strong>Tarih / Saat:</strong> {dateStr}</li>
                            </ul>
                            <p>Detayları incelemek ve onaylamak için Admin paneline giriş yapabilirsiniz.</p>
                            <hr style='border: 1px solid #eee; my-4;' />
                            <p style='font-size: 12px; color: #888;'>Bu mail BookPilot tarafından otomatik olarak gönderilmiştir.</p>
                        </div>";
                    _ = _emailService.SendEmailAsync(ownerEmail, adminSubject, adminBody, tenant.SmtpJson);
                }
            }

            // Sadece bu randevuya özel Cache temizlemesi yapabiliriz (opsiyonel ama sağlıklı)
            await redisDb.KeyDeleteAsync($"Avail_{slug}_{service.Id}_{slotDay:yyyyMMdd}_{assignedStaffId}");
            await redisDb.KeyDeleteAsync($"Avail_{slug}_{service.Id}_{slotDay:yyyyMMdd}_null");

            return appointment;
        }
        finally 
        {
            await redisDb.LockReleaseAsync(lockKey, lockValue);
        }
    }
}

public class SlotResponse
{
    public string Time { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}

public class CreateAppointmentRequest
{
    public Guid ServiceId { get; set; }
    public Guid? StaffId { get; set; }
    public DateTime SlotDate { get; set; }
    public string SlotTime { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string ExtraJson { get; set; } = "{}";
}

