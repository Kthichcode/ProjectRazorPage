using BusinessObjects;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.Interfaces;
using Services;
using Services.Interfaces;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IRoomImageRepository, RoomImageRepository>();
builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();

// Admin services
builder.Services.AddScoped<Repositories.Interfaces.IBookingRepository, Repositories.BookingRepository>();
builder.Services.AddScoped<Repositories.Interfaces.IPaymentRepository, Repositories.PaymentRepository>();
builder.Services.AddScoped<Services.Interfaces.IBookingService, Services.BookingService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddScoped<Services.Interfaces.IDashboardService, Services.DashboardService>();

// AI Chat Service
builder.Services.AddScoped<IAiChatService, AiChatService>();

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// ── SEED ADMIN ACCOUNT ──────────────────────────────────────────────
await SeedAdminAsync(app);

app.Run();

// ─────────────────────────────────────────────────────────────────────
static async Task SeedAdminAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    const string adminRole = "Admin";
    const string adminUser = "Admin";
    const string adminEmail = "admin@muongthanh.com";
    const string adminPass = "Admin123@";

    // 1. Tạo role "Admin" nếu chưa có
    if (!await roleManager.RoleExistsAsync(adminRole))
        await roleManager.CreateAsync(new IdentityRole(adminRole));

    // 2. Tạo user Admin nếu chưa có
    var user = await userManager.FindByNameAsync(adminUser);
    if (user == null)
    {
        user = new ApplicationUser
        {
            UserName = adminUser,
            Email = adminEmail,
            EmailConfirmed = true,
            FullName = "Quản trị viên"
        };
        var result = await userManager.CreateAsync(user, adminPass);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Không thể tạo tài khoản Admin: {errors}");
        }
    }

    // 3. Gán role Admin nếu chưa có
    if (!await userManager.IsInRoleAsync(user, adminRole))
        await userManager.AddToRoleAsync(user, adminRole);
}
