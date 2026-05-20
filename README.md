# Billiard Management System API

Hệ thống Quản lý Quán Bida được xây dựng bằng ASP.NET Core Web API (.NET 8) theo kiến trúc Clean Architecture.

## 🚀 Công nghệ sử dụng
- **Framework:** ASP.NET Core 8 Web API
- **ORM:** Entity Framework Core
- **Database:** SQL Server
- **Real-time:** SignalR
- **Mappers & Validation:** AutoMapper, FluentValidation
- **Authentication:** JWT Token
- **Architecture:** Clean Architecture, Repository Pattern, Unit of Work

## 📂 Cấu trúc dự án
- `BilliardManagement.API`: Controllers, Middlewares, SignalR Hubs, Config.
- `BilliardManagement.Business`: Services, DTOs, Mappings, Validators, Interfaces.
- `BilliardManagement.Data`: DbContext, Repositories, Migrations.
- `BilliardManagement.Models`: Domain Entities, Enums.
- `BilliardManagement.Common`: Constants, Helpers, API Responses, Exceptions.

## 🛠️ Hướng dẫn cài đặt và chạy

### ⚠️ QUAN TRỌNG: Khắc phục lỗi kết nối
Nếu gặp lỗi **"No connection could be made because the target machine actively refused it (localhost:5001)"**:
- **Nguyên nhân:** Web app đang cố kết nối đến API nhưng API chưa chạy
- **Giải pháp:** Xem file `START-PROJECTS.md` hoặc `TROUBLESHOOTING.md`
- **Nhanh nhất:** Chạy file `start-projects.bat` để khởi động cả API và Web

### Cách 1: Chạy cả API và Web (Khuyến nghị)
**Option A - Sử dụng script tự động:**
```bash
# Windows
start-projects.bat

# PowerShell
.\start-projects.ps1
```

**Option B - Visual Studio:**
1. Mở `BilliardManagementSystem.sln`
2. Right-click Solution → "Configure Startup Projects"
3. Chọn "Multiple startup projects"
4. Set cả `BilliardManagement.API` và `BilliardManagement.Web` thành "Start"
5. Nhấn F5

**Option C - Hai Terminal riêng biệt:**
```bash
# Terminal 1 - API
cd BilliardManagement.API
dotnet run

# Terminal 2 - Web (chờ API khởi động xong)
cd BilliardManagement.Web
dotnet run
```

### Cách 2: Chỉ chạy API
1. Đảm bảo đã cài đặt .NET 8 SDK và SQL Server.
2. Clone dự án về máy.
3. Cập nhật chuỗi kết nối `DBDefault` trong `appsettings.json` (thư mục API).
4. Mở Terminal tại thư mục gốc, chạy lệnh Migration:
   ```bash
   dotnet ef database update --project BilliardManagement.Data --startup-project BilliardManagement.API
   ```
5. Chạy project:
   ```bash
   dotnet run --project BilliardManagement.API
   ```

### Cách 3: Chạy bằng Docker Compose
1. Cài đặt Docker Desktop.
2. Tại thư mục gốc (nơi chứa file `docker-compose.yml`), chạy lệnh:
   ```bash
   docker-compose up -d --build
   ```
3. API sẽ chạy ở cổng `8080` (`http://localhost:8080/swagger`).
4. SQL Server sẽ chạy ở cổng `1433`.

## 🌐 Swagger UI
Sau khi khởi chạy thành công, truy cập Swagger UI để xem toàn bộ danh sách API và test thử:
- **API Swagger:** `https://localhost:5001/swagger`
- **Web Application:** `https://localhost:5068`
- Sử dụng API `/api/Auth/login` để lấy JWT Token và nhập vào ô `Authorize` trên Swagger.

## 📝 Tài liệu bổ sung
- **START-PROJECTS.md** - Hướng dẫn chi tiết cách khởi động dự án
- **TROUBLESHOOTING.md** - Giải quyết các lỗi thường gặp
- **start-projects.bat** - Script tự động khởi động cả API và Web

## 🔌 SignalR Hubs
Hỗ trợ realtime không cần reload trang:
- **Table Hub:** `/hubs/table` (cập nhật trạng thái bàn, thời gian)
- **Order Hub:** `/hubs/order` (cập nhật món nước/đồ ăn)
- **Notification Hub:** `/hubs/notification` (thông báo)
