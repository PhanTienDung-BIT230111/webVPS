#!/bin/bash

echo "🚀 Khởi động dự án WebBanHangOnline với SQL Server..."

# Kiểm tra xem .NET có được cài đặt chưa
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK chưa được cài đặt. Vui lòng cài đặt .NET 8.0 SDK trước."
    echo "Hướng dẫn cài đặt: https://dotnet.microsoft.com/download"
    exit 1
fi

# Kiểm tra phiên bản .NET
echo "📋 Phiên bản .NET: $(dotnet --version)"

# Khôi phục các package
echo "📦 Khôi phục các package..."
dotnet restore

# Chạy migration
echo "🔄 Chạy migration..."
dotnet ef database update --context ApplicationDbContext

# Chạy dự án
echo "🌐 Khởi động ứng dụng..."
echo "📍 Truy cập: http://localhost:5000"
echo "📍 Admin: http://localhost:5000/Admin"
echo ""
echo "⏹️  Nhấn Ctrl+C để dừng ứng dụng"
echo ""

dotnet run --environment Production 