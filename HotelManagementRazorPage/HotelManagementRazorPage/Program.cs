using BusinessObjects;
using BusinessObjects.Entities;
using HotelManagementRazorPage.Hubs;
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
builder.Services.AddSignalR();


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
app.MapHub<HotelHub>("/hotelHub");

// ── SEED ADMIN ACCOUNT ──────────────────────────────────────────────

await SeedAdminAsync(app);
await SeedStaffAsync(app);

app.Run();

// ─────────────────────────────────────────────────────────────────────
static async Task SeedStaffAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    const string staffRole = "Staff";

    // 1. Tạo role "Staff" nếu chưa có
    if (!await roleManager.RoleExistsAsync(staffRole))
        await roleManager.CreateAsync(new IdentityRole(staffRole));

    // 2. Tạo danh sách các user Staff
    var staffUsers = new List<(string UserName, string Email, string FullName, string Password)>
    {
        ("staff", "staff@muongthanh.com", "Nhân viên 1", "Staff123@"),
        ("staff1", "staff1@muongthanh.com", "Nhân viên 2", "Staff123@"),
        ("staff2", "staff2@muongthanh.com", "Nhân viên 3", "Staff123@")
    };

    foreach (var s in staffUsers)
    {
        var user = await userManager.FindByNameAsync(s.UserName);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = s.UserName,
                Email = s.Email,
                EmailConfirmed = true,
                FullName = s.FullName
            };
            var result = await userManager.CreateAsync(user, s.Password);
            if (!result.Succeeded) continue;
        }

        // 3. Gán role Staff nếu chưa có
        if (!await userManager.IsInRoleAsync(user, staffRole))
            await userManager.AddToRoleAsync(user, staffRole);
    }
}

// ─────────────────────────────────────────────────────────────────────
static async Task SeedAdminAsync(WebApplication app)

{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // ── Seed Admin ──
    const string adminRole = "Admin";
    if (!await roleManager.RoleExistsAsync(adminRole))
        await roleManager.CreateAsync(new IdentityRole(adminRole));

    var adminUser = await userManager.FindByNameAsync("Admin");
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "Admin",
            Email = "admin@muongthanh.com",
            EmailConfirmed = true,
            FullName = "Quản trị viên"
        };
        var result = await userManager.CreateAsync(adminUser, "Admin123@");
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Không thể tạo tài khoản Admin: {errors}");
        }
    }
    if (!await userManager.IsInRoleAsync(adminUser, adminRole))
        await userManager.AddToRoleAsync(adminUser, adminRole);

    // ── Seed Manager ──
    const string managerRole = "Manager";
    if (!await roleManager.RoleExistsAsync(managerRole))
        await roleManager.CreateAsync(new IdentityRole(managerRole));

    var managerUser = await userManager.FindByNameAsync("Manager");
    if (managerUser == null)
    {
        managerUser = new ApplicationUser
        {
            UserName = "Manager",
            Email = "manager@muongthanh.com",
            EmailConfirmed = true,
            FullName = "Quản lý vận hành"
        };
        var result = await userManager.CreateAsync(managerUser, "Manager123@");
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Không thể tạo tài khoản Manager: {errors}");
        }
    }
    if (!await userManager.IsInRoleAsync(managerUser, managerRole))
        await userManager.AddToRoleAsync(managerUser, managerRole);
}
