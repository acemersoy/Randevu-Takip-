using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RandevuTakip.Api.Data;
using RandevuTakip.Api.Models;

namespace RandevuTakip.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var admin = await _context.Admins
            .Include(a => a.Tenant)
            .FirstOrDefaultAsync(a => a.Email == request.Email);

        if (admin == null || !BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
        {
            return Unauthorized(new { Message = "E-posta veya şifre hatalı." });
        }

        Guid? staffId = null;
        if (admin.Role == "Staff")
        {
            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.UserId == admin.Id);
            staffId = staff?.Id;
        }

        var token = GenerateJwtToken(admin, staffId);

        return Ok(new
        {
            Token = token,
            Admin = new
            {
                admin.Email,
                admin.Role,
                TenantSlug = admin.Tenant.Slug,
                admin.Tenant.Name,
                StaffId = staffId
            }
        });
    }

    private string GenerateJwtToken(Admin admin, Guid? staffId)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Email, admin.Email),
            new Claim(ClaimTypes.Role, admin.Role),
            new Claim("TenantId", admin.TenantId.ToString()),
            new Claim("TenantSlug", admin.Tenant.Slug)
        };

        if (staffId.HasValue)
        {
            claims.Add(new Claim("StaffId", staffId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            _config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(_config["Jwt:DurationInMinutes"])),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
