using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebBanHangOnline.Data;
using WebBanHangOnline.Models;
using Microsoft.AspNetCore.Http;
using Prometheus; // Thêm thư viện Prometheus để thu thập metrics

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:80");



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Kiểm tra môi trường để quyết định sử dụng database nào
if (builder.Environment.IsDevelopment())
{
    // Sử dụng SQLite trong môi trường Development
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
}
else
{
    // Sử dụng SQL Server trong môi trường Production
    var saPassword = builder.Configuration["SA_PASSWORD"];
    // Nếu mật khẩu tồn tại, thêm nó vào chuỗi kết nối.
    // Cấu hình này giúp chuỗi kết nối hoạt động cả trong Docker Compose và K8s
    if (!string.IsNullOrEmpty(saPassword))
    {
        connectionString = $"{connectionString};Password={saPassword}";
    }

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
        {
            // Thêm chính sách thử lại khi kết nối thất bại, rất hữu ích trong K8s khi DB có thể mất một lúc để khởi động
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));
}

// 2. Cấu hình Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Cấu hình đường dẫn cho các trang của Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// 4. Thêm dịch vụ cho Controller và View
builder.Services.AddControllersWithViews();

// 5. Cấu hình Session để lưu giỏ hàng
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    // Cấu hình SecurePolicy và SameSiteMode để tương thích với LoadBalancer/Ingress Controller
    // trong Kubernetes, nơi HTTPS được xử lý ở tầng ngoài.
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
});

var app = builder.Build();

// Thêm cấu hình cho Prometheus
// Các dòng này sẽ kích hoạt endpoint /metrics để Prometheus có thể scrape dữ liệu
app.UseRouting(); // Cần có UseRouting() trước UseHttpMetrics()
app.UseHttpMetrics(); // Thu thập các metrics về HTTP request (thời gian, số lượng)

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

// Đảm bảo UseRouting() nằm trước cả UseSession() và UseAuthentication()
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Thực hiện di chuyển cơ sở dữ liệu và seed dữ liệu khi khởi động
// Điều này giúp đảm bảo schema DB luôn được cập nhật trong mọi môi trường
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        int retryCount = 0;
        const int maxRetries = 10;
        while (retryCount < maxRetries)
        {
            try
            {
                await dbContext.Database.MigrateAsync();
                Console.WriteLine("Database migration applied successfully.");
                break; // Thoát vòng lặp nếu thành công
            }
            catch (Exception ex)
            {
                retryCount++;
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, $"Failed to migrate database, retrying... ({retryCount}/{maxRetries})");
                await Task.Delay(TimeSpan.FromSeconds(5)); // Đợi 5 giây trước khi thử lại
                if (retryCount >= maxRetries)
                {
                    logger.LogError(ex, "Exceeded max retries for database migration.");
                    throw; // Ném lỗi nếu đã thử lại quá nhiều lần
                }
            }
        }
        Console.WriteLine(">>> Running SeedData.Initialize...");

        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB or migrating.");
    }
}

// Map các route
app.MapControllerRoute(
    name: "Admin",
    pattern: "{area:exists}/{controller=Admin}/{action=Dashboard}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Tạo endpoint /metrics cho Prometheus.
// Chú ý: Dòng này phải đứng sau các app.UseRouting() và app.MapControllerRoute()
app.MapMetrics();

app.Run();
