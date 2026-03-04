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
builder.Services.AddScoped<Repositories.Interfaces.IWalletRepository, Repositories.WalletRepository>();
builder.Services.AddScoped<Services.Interfaces.IBookingService, Services.BookingService>();
builder.Services.AddScoped<Services.Interfaces.IWalletService, Services.WalletService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddScoped<Services.Interfaces.IDashboardService, Services.DashboardService>();

// AI Chat Service
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAiChatService, AiChatService>();
builder.Services.AddScoped<ISignalRService, HotelManagementRazorPage.SignalR.SignalRService>();
builder.Services.AddScoped<Repositories.Interfaces.IReviewRepository, Repositories.ReviewRepository>();
builder.Services.AddScoped<Services.Interfaces.IReviewService, Services.ReviewService>();

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
app.MapHub<HotelManagementRazorPage.Hubs.RoomHub>("/roomHub");

await SeedAdminAsync(app);

app.Run();

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

    // ── Seed Staff ──
    const string staffRole = "Staff";
    if (!await roleManager.RoleExistsAsync(staffRole))
        await roleManager.CreateAsync(new IdentityRole(staffRole));

    for (int i = 1; i <= 3; i++)
    {
        string staffName = $"Staff{i}";
        var staffUser = await userManager.FindByNameAsync(staffName);
        if (staffUser == null)
        {
            staffUser = new ApplicationUser
            {
                UserName = staffName,
                Email = $"staff{i}@muongthanh.com",
                EmailConfirmed = true,
                FullName = $"Nhân viên {i}"
            };
            var result = await userManager.CreateAsync(staffUser, "Staff123@");
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Không thể tạo tài khoản {staffName}: {errors}");
            }
        }
        if (!await userManager.IsInRoleAsync(staffUser, staffRole))
            await userManager.AddToRoleAsync(staffUser, staffRole);
    }
}
