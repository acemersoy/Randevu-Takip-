using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RandevuTakip.Api.Data;
using RandevuTakip.Api.Models;
using RandevuTakip.Api.Middleware;
using RandevuTakip.Api.Services;

using System.Text.Json.Serialization;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddTransient<IEmailService, SmtpEmailService>();
builder.Services.AddMemoryCache();

// Redis Configuration
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "BookPilot_";
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

SeedDatabase(app);

// --- GEÇİCİ SMTP TEST KODU ---
// using (var scope = app.Services.CreateScope())
// {
//     var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
//     _ = emailService.SendEmailAsync("fodasSmtp@gmail.com", "Sistem Test Maili", "<h1>Merhaba!</h1><p>Bu mail RandevuTakip üzerinden başarıyla atıldı.</p>");
// }
// ----------------------------

app.Run();

static void SeedDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var tid = Guid.Parse("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1");

    // Tenant
    if (!db.Tenants.Any(t => t.Slug == "dentist"))
    {
        db.Tenants.Add(new Tenant
        {
            Id = tid,
            Slug = "dentist",
            Name = "Örnek Diş Kliniği",
            Industry = "Dentist",
            ThemeJson = "{\"primary\": \"#4f46e5\", \"borderRadius\": \"1rem\"}",
            BookingFormSchema = "[{\"name\": \"complaint\", \"type\": \"textarea\", \"label\": \"Şikayetiniz\", \"required\": true}]",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        db.SaveChanges();
    }

    // Admin ve Staff Login Hesapları
    if (!db.Admins.Any(a => a.Email == "admin@demo.com"))
    {
        db.Admins.Add(new Admin
        {
            Id = Guid.NewGuid(),
            TenantId = tid,
            Email = "admin@demo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Owner",
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }

    // Personeller için Kullanıcı (Auth) ID'leri
    var ayseUserId = Guid.Parse("d1e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1");
    var mehmetUserId = Guid.Parse("d2e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a2");
    var selinUserId = Guid.Parse("d3e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a3");

    if (!db.Admins.Any(a => a.Email == "ayse@demo.com"))
    {
        db.Admins.AddRange(
            new Admin { Id = ayseUserId, TenantId = tid, Email = "ayse@demo.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("staff123"), Role = "Staff", CreatedAt = DateTime.UtcNow },
            new Admin { Id = mehmetUserId, TenantId = tid, Email = "mehmet@demo.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("staff123"), Role = "Staff", CreatedAt = DateTime.UtcNow },
            new Admin { Id = selinUserId, TenantId = tid, Email = "selin@demo.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("staff123"), Role = "Staff", CreatedAt = DateTime.UtcNow }
        );
        db.SaveChanges();
    }

    // Working Hours — Pzt–Cum 09-18, Cmt 09-14
    var wdays = new[] {
        (DayOfWeek.Monday,    9, 18),
        (DayOfWeek.Tuesday,   9, 18),
        (DayOfWeek.Wednesday, 9, 18),
        (DayOfWeek.Thursday,  9, 18),
        (DayOfWeek.Friday,    9, 18),
        (DayOfWeek.Saturday,  9, 14),
    };
    foreach (var (dow, oh, ch) in wdays)
    {
        if (!db.WorkingHours.Any(w => w.TenantId == tid && w.DayOfWeek == dow && !w.StaffId.HasValue))
            db.WorkingHours.Add(new WorkingHours
            {
                Id = Guid.NewGuid(),
                TenantId = tid,
                DayOfWeek = dow,
                OpenTime = new TimeSpan(oh, 0, 0),
                CloseTime = new TimeSpan(ch, 0, 0),
                SlotStepMinutes = 30,
                IsClosed = false
            });
    }
    db.SaveChanges();

    // Service IDs
    var s1 = Guid.Parse("b1e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1");
    var s2 = Guid.Parse("b2e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a2");
    var s3 = Guid.Parse("b3e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a3");
    var s4 = Guid.Parse("b4e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a4");
    var s5 = Guid.Parse("b5e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a5");
    var s6 = Guid.Parse("b6e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a6");
    var s7 = Guid.Parse("b7e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a7");

    // All Services (migration seed kaldırıldığı için hepsi burada)
    var allSvcs = new (Guid Id, string Name, string Desc, int Dur, decimal Pr)[]
    {
        (s1, "Genel Muayene",       "İlk kontrol ve ağız sağlığı değerlendirmesi.",  20,  500m),
        (s2, "Kompozit Dolgu",      "Estetik dolgu işlemi.",                          45, 1200m),
        (s3, "Diş Taşı Temizliği",  "Derinlemesine diş taşı ve plak temizliği.",     30,  800m),
        (s4, "Kanal Tedavisi",      "Derin çürük ve enfeksiyon tedavisi.",            60, 3500m),
        (s5, "Diş Beyazlatma",      "Ofis tipi profesyonel diş beyazlatma.",          45, 2500m),
        (s6, "Ortodonti Muayenesi", "Diş sıralaması ve bite değerlendirmesi.",         30,  750m),
        (s7, "Diş Çekimi",          "Lokal anestezi ile basit diş çekimi.",            20,  600m),
    };
    foreach (var svc in allSvcs)
    {
        if (!db.Services.Any(s => s.Id == svc.Id))
            db.Services.Add(new Service
            {
                Id = svc.Id,
                TenantId = tid,
                Name = svc.Name,
                Description = svc.Desc,
                DurationMinutes = svc.Dur,
                Price = svc.Pr,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
    }
    db.SaveChanges();

    // Staff IDs
    var ayse = Guid.Parse("c0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1");
    var mehmet = Guid.Parse("c1e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a2");
    var selin = Guid.Parse("c2e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a3");

    // All Staff (Ayşe migration'dan kaldırıldı, hepsi burada)
    var staffAyse = db.Staff.FirstOrDefault(s => s.Id == ayse);
    if (staffAyse == null)
        db.Staff.Add(new Staff
        {
            Id = ayse,
            TenantId = tid,
            UserId = ayseUserId,
            Name = "Dr. Ayşe Yılmaz",
            Title = "Başhekim / Ortodontist",
            Email = "ayse@demo.com",
            Bio = "15 yıllık deneyimli ortodonti uzmanı.",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    else
        staffAyse.UserId = ayseUserId;

    var staffMehmet = db.Staff.FirstOrDefault(s => s.Id == mehmet);
    if (staffMehmet == null)
        db.Staff.Add(new Staff
        {
            Id = mehmet,
            TenantId = tid,
            UserId = mehmetUserId,
            Name = "Dr. Mehmet Demir",
            Title = "Endodontist / Kanal Uzmanı",
            Email = "mehmet@demo.com",
            Bio = "Kanal tedavisi ve endodonti konusunda 10 yıllık uzman hekim.",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    else
        staffMehmet.UserId = mehmetUserId;

    var staffSelin = db.Staff.FirstOrDefault(s => s.Id == selin);
    if (staffSelin == null)
        db.Staff.Add(new Staff
        {
            Id = selin,
            TenantId = tid,
            UserId = selinUserId,
            Name = "Dt. Selin Çelik",
            Title = "Estetik Diş Uzmanı",
            Email = "selin@demo.com",
            Bio = "Beyazlatma, veneer ve estetik restorasyon uzmanı.",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    else
        staffSelin.UserId = selinUserId;

    db.SaveChanges();

    // Staff-Service Mappings
    var maps = new (Guid Sid, Guid Svcid)[]
    {
        // Dr. Ayşe: Genel + Taş + Ortodonti + Kompozit
        (ayse,   s1), (ayse, s2), (ayse, s3), (ayse, s6),
        // Dr. Mehmet: Kanal + Çekim + Genel
        (mehmet, s4), (mehmet, s7), (mehmet, s1),
        // Dt. Selin: Beyazlatma + Genel + Kompozit
        (selin,  s5), (selin,  s1), (selin,  s2),
    };
    foreach (var m in maps)
    {
        if (!db.StaffServices.Any(ss => ss.StaffId == m.Sid && ss.ServiceId == m.Svcid))
            db.StaffServices.Add(new StaffService { Id = Guid.NewGuid(), StaffId = m.Sid, ServiceId = m.Svcid });
    }
    db.SaveChanges();

    // Sample Appointments
    if (!db.Appointments.Any(a => a.TenantId == tid))
    {
        var today = DateTime.UtcNow.Date;
        db.Appointments.AddRange(new Appointment[]
        {
            new() { Id=Guid.NewGuid(), TenantId=tid, ServiceId=s1, StaffId=ayse,
                StartAt=today.AddDays(-2).AddHours(9), EndAt=today.AddDays(-2).AddHours(9).AddMinutes(20),
                CustomerName="Ahmet Yılmaz", CustomerPhone="05321234567", CustomerEmail="ahmet@gmail.com",
                Status="Completed", ExtraJson="{\"complaint\":\"Dişlerimde ağrı var.\"}", CreatedAt=today.AddDays(-3) },
            new() { Id=Guid.NewGuid(), TenantId=tid, ServiceId=s2, StaffId=ayse,
                StartAt=today.AddDays(-1).AddHours(11), EndAt=today.AddDays(-1).AddHours(11).AddMinutes(45),
                CustomerName="Fatma Kaya", CustomerPhone="05439876543", CustomerEmail="fatma@hotmail.com",
                Status="Completed", ExtraJson="{\"complaint\":\"Çürük dolgu yaptırmak istiyorum.\"}", CreatedAt=today.AddDays(-2) },
            new() { Id=Guid.NewGuid(), TenantId=tid, ServiceId=s4, StaffId=mehmet,
                StartAt=today.AddDays(-1).AddHours(14), EndAt=today.AddDays(-1).AddHours(15),
                CustomerName="Mustafa Şahin", CustomerPhone="05557778899",
                Status="Confirmed", ExtraJson="{\"complaint\":\"Kanal tedavisi için geldim.\"}", CreatedAt=today.AddDays(-2) },
            new() { Id=Guid.NewGuid(), TenantId=tid, ServiceId=s5, StaffId=selin,
                StartAt=today.AddHours(10), EndAt=today.AddHours(10).AddMinutes(45),
                CustomerName="Zeynep Arslan", CustomerPhone="05441122334", CustomerEmail="zeynep@gmail.com",
                Status="Confirmed", ExtraJson="{\"complaint\":\"Dişlerimi beyazlatmak istiyorum.\"}", CreatedAt=today.AddDays(-1) },
            new() { Id=Guid.NewGuid(), TenantId=tid, ServiceId=s3, StaffId=ayse,
                StartAt=today.AddHours(13), EndAt=today.AddHours(13).AddMinutes(30),
                CustomerName="Ali Çelik", CustomerPhone="05339988776",
                Status="Pending", ExtraJson="{\"complaint\":\"Diş taşı temizliği yaptırmak istiyorum.\"}", CreatedAt=today },
            new() { Id=Guid.NewGuid(), TenantId=tid, ServiceId=s1, StaffId=mehmet,
                StartAt=today.AddDays(1).AddHours(9), EndAt=today.AddDays(1).AddHours(9).AddMinutes(20),
                CustomerName="Elif Türk", CustomerPhone="05551234567", CustomerEmail="elif@gmail.com",
                Status="Pending", ExtraJson="{\"complaint\":\"Kontrol muayenesi.\"}", CreatedAt=today },
        });
        db.SaveChanges();
    }
}

