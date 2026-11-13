# Multi-tenant: triển khai và kiểm chứng

Cập nhật: **13/11/2026**. Các mục của kế hoạch 7 phase đã được triển khai;
checklist tích hợp đã chạy thành công trên SQL Server local.

## Chức năng

- Tenant trực tiếp: StoreId, FK, index và unique index theo store.
- Orders, OrderItems, Invoices, SessionCombos: query filter qua phiên chơi.
- JWT chứa `store_id`; middleware xác minh tài khoản, vai trò, store còn hoạt động.
- Admin/Staff luôn dùng store từ token, bỏ qua header/body StoreId giả.
- SuperAdmin không gửi `X-Store-Id`: đọc tổng hợp toàn hệ thống.
- SuperAdmin gửi `X-Store-Id`: đọc/ghi giới hạn trong store đã chọn. Ghi nghiệp vụ
  yêu cầu header hợp lệ; ID bàn/category/product/session khác store bị từ chối.
- `/Admin/Stores`: quản lý cửa hàng và tạo Admin.
- `/Admin/Users`: tạo Admin, gán tài khoản hiện có thành Admin vào store.
  Chuyển store bị chặn nếu tài khoản có lịch sử phiên, order, ca hoặc thay đổi trạng thái bàn.
  Token cũ bị từ chối ở request tiếp theo sau khi đổi role/store; cần đăng nhập lại.
- Dashboard và doanh thu hiện có được tái sử dụng cho SuperAdmin để tổng hợp tất cả store.
- Sidebar Admin/Staff hiển thị tên cửa hàng.
- SignalR yêu cầu JWT: gửi vào group store tương ứng và group giám sát SuperAdmin.

### Liên kết với tài khoản SuperAdmin

User query filter cho phép tài khoản SuperAdmin làm actor trong navigation bắt buộc
(ví dụ `Invoice → TableSession → User`). Nếu loại actor này khỏi join, báo cáo có thể
bỏ sót hóa đơn của phiên do SuperAdmin mở. API danh sách/chi tiết người dùng vẫn có
điều kiện store riêng; Admin không đọc được tài khoản SuperAdmin qua các endpoint này.

## Seed

API đọc mật khẩu khởi tạo từ `Seed:SuperAdminPassword` (`Seed__SuperAdminPassword`
khi dùng biến môi trường). Giá trị cấu hình chỉ dùng khi tạo tài khoản hoặc nâng cấp
hash cũ; không ghi đè mật khẩu đã hash của tài khoản hiện có.

`Seed:SampleData=false` chỉ seed store mặc định và các tài khoản, bỏ qua dữ liệu mẫu
bàn, sản phẩm, phiên, hóa đơn. Mặc định giữ luồng seed dữ liệu mẫu hiện có.

## Chạy lại kiểm thử

Yêu cầu: .NET SDK, Node.js 22+ có Fetch/WebSocket, SQL Server theo connection string
trong API appsettings và tài khoản SQL có quyền tạo/xóa database kiểm thử.

Tại thư mục solution:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\verify-multitenant.ps1"
```

Kiểm tra cả luồng seed dữ liệu mẫu trên database mới:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\verify-multitenant.ps1" -IncludeSampleData
```

Runner build solution, tạo database có tên ngẫu nhiên `billiard_verify_*`, khởi động API
và Web trên cổng 18080/18081, chạy test, dừng ứng dụng và xóa database trong `finally`.
Có thể đổi cổng bằng `-ApiPort` và `-WebPort`. Connection string database ứng dụng
được override chỉ trong process test; không thêm dữ liệu test vào database đang sử dụng.

## Kết quả thực tế

Cả chế độ seed tài khoản và chế độ `-IncludeSampleData` đã **PASS**:

- Tạo 12 store mới, 120 bàn có tên trùng nhau giữa các store.
- Số điện thoại giống nhau ở hai store được chấp nhận; trùng trong cùng store bị từ chối.
- Admin store B không truy cập được bàn/khách hàng store A.
- StoreId giả trong body và header của Admin không thay đổi tenant thực tế.
- Gán Admin thành công, token cũ bị từ chối; chuyển tài khoản có lịch sử bị chặn.
- Tạo category/product/combo cùng tên/mã ở hai store bằng SuperAdmin thành công.
- Combo/order không được tham chiếu sản phẩm store khác; phiên không được mở cho bàn store khác.
- Doanh thu store chỉ tính hóa đơn của store, gồm cả phiên do SuperAdmin mở.
- Ba WebSocket SignalR thật: store A chỉ nhận sự kiện A, store B chỉ nhận sự kiện B,
  SuperAdmin nhận cả hai. Test cả thao tác ghi từ SuperAdmin và Admin.
- SignalR negotiate ẩn danh trả 401.
- Web form tạo Store, tạo/gán Admin; dashboard/doanh thu SuperAdmin; chặn Admin vào Users;
  sidebar Staff hiển thị đúng tên store.
- Kết thúc phiên và tạo hóa đơn thành công.

### Benchmark cục bộ

Sau warmup, 30 request tuần tự mỗi endpoint; dữ liệu test mới: **12 store, 122 bàn,
12 phiên đang hoạt động** (ngoài store mặc định và dữ liệu mẫu nếu bật).

| Chế độ | Endpoint | Median | p95 |
|---|---|---:|---:|
| Chỉ seed tài khoản | `/api/tables` | 3,4 ms | 4,3 ms |
| Chỉ seed tài khoản | `/api/table-sessions` | 4,9 ms | 7,4 ms |
| Có dữ liệu mẫu | `/api/tables` | 7,4 ms | 17,8 ms |
| Có dữ liệu mẫu | `/api/table-sessions` | 7,2 ms | 8,8 ms |

Đây là smoke benchmark trên môi trường local, không phải load test đồng thời hoặc SLA production.

## Kiểm tra bổ sung

- `dotnet build BilliardManagementSystem.sln --nologo -v q`: thành công.
- `dotnet ef migrations has-pending-model-changes --project BilliardManagement.Data --startup-project BilliardManagement.API`:
  không có thay đổi model chưa được migrate.
- Migration multi-tenant đã apply vào database local từ giai đoạn trước.
- `db/database.sql` sinh từ migrations, gồm lịch sử migration và store mặc định;
  đã chạy thành công trên database tạm ở giai đoạn trước.
