# WebBanHangOnline

Dự án website bán hàng online được xây dựng bằng ASP.NET Core 8.0

## Yêu cầu hệ thống

- .NET 8.0 SDK
- Linux/macOS/Windows

## Cách chạy dự án

### Phương pháp 1: Sử dụng SQLite (Khuyến nghị cho Development)

1. **Cài đặt .NET 8.0 SDK** (nếu chưa có):
   ```bash
   # Ubuntu/Debian
   wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
   sudo dpkg -i packages-microsoft-prod.deb
   sudo apt-get update
   sudo apt-get install -y dotnet-sdk-8.0
   ```

2. **Chạy dự án bằng script tự động**:
   ```bash
   ./run-dev.sh
   ```

3. **Hoặc chạy thủ công**:
   ```bash
   # Khôi phục packages
   dotnet restore
   
   # Tạo migration (chỉ cần chạy 1 lần)
   dotnet ef migrations add InitialCreate --context ApplicationDbContext
   
   # Chạy migration
   dotnet ef database update --context ApplicationDbContext
   
   # Chạy dự án
   dotnet run --environment Development
   ```

4. **Truy cập ứng dụng**:
   - Website: http://localhost:5000
   - Admin: http://localhost:5000/Admin

### Phương pháp 2: Sử dụng SQL Server (Production)

1. Cài đặt SQL Server
2. Cập nhật connection string trong `appsettings.json`
3. Chạy: `dotnet run --environment Production`

## Tính năng

- ✅ Đăng ký/Đăng nhập người dùng
- ✅ Quản lý sản phẩm và danh mục
- ✅ Giỏ hàng và đặt hàng
- ✅ Giao diện admin
- ✅ Upload và lưu trữ ảnh sản phẩm trong database
- ✅ Monitoring với Prometheus

## Cấu trúc dự án

```
WebBanHangOnline/
├── Areas/Admin/          # Khu vực quản trị
├── Controllers/          # Controllers chính
├── Data/                 # DbContext và Seed data
├── Models/               # Entity models
├── Views/                # Razor views
├── wwwroot/              # Static files
└── Program.cs            # Entry point
```

## Tài khoản mặc định

Sau khi chạy lần đầu, hệ thống sẽ tự động tạo dữ liệu mẫu.

## Lưu ý

- Dự án sử dụng SQLite trong môi trường Development
- SQL Server được sử dụng trong Production
- Ảnh sản phẩm được lưu trữ trực tiếp trong database
- Prometheus metrics được bật tại `/metrics` 