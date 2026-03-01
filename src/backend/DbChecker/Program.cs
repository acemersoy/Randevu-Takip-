using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RandevuTakip.Api.Data;

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=randevutakip_db;Username=admin;Password=password123");
using var context = new AppDbContext(optionsBuilder.Options);

var admin = context.Admins.Include(a => a.Tenant).FirstOrDefault(a => a.Email == "admin@demo.com");
if (admin != null) {
    Console.WriteLine("FOUND SLUG: " + admin.Tenant.Slug);
} else {
    Console.WriteLine("ADMIN NOT FOUND");
}
