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

### Cách 1: Chạy bằng Visual Studio / CLI
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

### Cách 2: Chạy bằng Docker Compose
1. Cài đặt Docker Desktop.
2. Tại thư mục gốc (nơi chứa file `docker-compose.yml`), chạy lệnh:
   ```bash
   docker-compose up -d --build
   ```
3. API sẽ chạy ở cổng `8080` (`http://localhost:8080/swagger`).
4. SQL Server sẽ chạy ở cổng `1433`.

## 🌐 Swagger UI
Sau khi khởi chạy thành công, truy cập Swagger UI để xem toàn bộ danh sách API và test thử:
- **URL:** `http://localhost:<port>/swagger`
- Sử dụng API `/api/Auth/login` để lấy JWT Token và nhập vào ô `Authorize` trên Swagger.

## 🔌 SignalR Hubs
Hỗ trợ realtime không cần reload trang:
- **Table Hub:** `/hubs/table` (cập nhật trạng thái bàn, thời gian)
- **Order Hub:** `/hubs/order` (cập nhật món nước/đồ ăn)
- **Notification Hub:** `/hubs/notification` (thông báo)
