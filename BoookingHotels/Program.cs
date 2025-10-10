using BoookingHotels.Data;
using BoookingHotels.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using static BoookingHotels.Controllers.AuthController;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// EF Core SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
QuestPDF.Settings.License = LicenseType.Community;
// Authentication - Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";      
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });
builder.Services.AddScoped<AdminActionLogAttribute>();

var app = builder.Build();

// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();   // ph?i ��?t tr??c UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed database với sample data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Kiểm tra kết nối database có sẵn
        if (await context.Database.CanConnectAsync())
        {
            // Cập nhật tên khách sạn cho thực tế hơn  
            // await HotelNameUpdater.UpdateHotelNamesAsync(context); // Already updated!
            
            // Cập nhật tên và giá phòng theo khách sạn
            // await RoomNameUpdater.UpdateRoomNamesAsync(context); // Run once
            // await RoomNameUpdater.UpdateRoomPricesAsync(context); // Run once
            
            // Cập nhật hình ảnh cho hotels và rooms
            // await PhotoUpdater.UpdateAllPhotosAsync(context); // Run once
            
            Console.WriteLine("✅ Database connected successfully!");
        }
        else
        {
            Console.WriteLine("❌ Cannot connect to database!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error seeding database: {ex.Message}");
    }
}

app.Run();
