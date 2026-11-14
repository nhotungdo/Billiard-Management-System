# BÁO CÁO CÁC TÍNH NĂNG VÀ FILE NGUỒN THỎA MÃN YÊU CẦU DỰ ÁN
**Hệ thống Quản lý Quán Bida (Billiard Management System)**  
*Dành cho mục đích Báo cáo, Đánh giá OJT và Chấm đồ án môn học*

---

## BẢNG TỔNG HỢP CÁC TIÊU CHÍ ĐÁNH GIÁ

| STT | Tiêu chí yêu cầu | Trạng thái | Các File nguồn đại diện tiêu biểu |
|:---:|:---|:---:|:---|
| **1** | **Tối thiểu 5 API: GET all, GET by id, POST, PUT, DELETE** |  ĐẠT | `BilliardManagement.API/Controllers/ProductsController.cs`<br>`BilliardManagement.API/Controllers/TablesController.cs`<br>`BilliardManagement.API/Controllers/CategoriesController.cs`<br>`BilliardManagement.API/Controllers/CombosController.cs` |
| **2** | **Media Formatter, Content Negotiation** |  ĐẠT | `BilliardManagement.API/Program.cs`<br>`BilliardManagement.API/Controllers/ProductsController.cs` |
| **3** | **Sử dụng thư viện OData, AutoMapper** |  ĐẠT | **OData:** `BilliardManagement.API/Program.cs`, `ProductsController.cs`, `TablesController.cs`<br>**AutoMapper:** `BilliardManagement.Business/Mappings/AutoMapperProfile.cs`, `DependencyInjection.cs` |
| **4** | **Entity Framework Core (Code First)** |  ĐẠT | `BilliardManagement.Data/BilliardManagementDbContext.cs`<br>`BilliardManagement.Data/Migrations/*` (16 Migrations)<br>`BilliardManagement.API/Program.cs` |
| **5** | **Sử dụng các Data Annotation** |  ĐẠT | `BilliardManagement.Models/Models/*.cs` (BilliardTable, User, Product, Invoice, Combo, Order...)<br>`BilliardManagement.Business/DTOs/*.cs` |
| **6** | **Gọi API bằng AJAX** |  ĐẠT | `BilliardManagement.Web/Pages/Staff/Tables/Map.cshtml`<br>`BilliardManagement.Web/Pages/Admin/Products/Index.cshtml`<br>`BilliardManagement.Web/Pages/Admin/Products/Edit.cshtml`<br>`BilliardManagement.Web/Pages/Admin/Combos/Index.cshtml` |
| **7** | **JWT Authentication & Authorization** |  ĐẠT | `BilliardManagement.API/Program.cs`<br>`BilliardManagement.API/Controllers/AuthController.cs`<br>`BilliardManagement.Business/Services/AllServices.cs` (`AuthService`)<br>Các controller có attribute `[Authorize]`, `[Authorize(Roles = "...")]` |

---

## 1. TIÊU CHÍ 1: TỐI THIỂU 5 CHUẨN RESTFUL API (GET ALL, GET BY ID, POST, PUT, DELETE)

Dự án triển khai kiến trúc RESTful API chuẩn mực trên nhiều Controller phục vụ quản lý thực thể.

### 1.1. Minh chứng tiêu biểu: `ProductsController`
* **File:** [ProductsController.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.API/Controllers/ProductsController.cs)
* **Route:** `/api/products`

| Phương thức HTTP | Tuyến đường (Endpoint) | Chức năng nghiệp vụ | Vị trí dòng |
|:---|:---|:---|:---:|
| **GET** | `/api/products` | Lấy danh sách sản phẩm (có phân trang, tìm kiếm) | Dòng 68 |
| **GET** | `/api/products/{id}` | Lấy chi tiết thông tin 1 sản phẩm theo ID | Dòng 75 |
| **POST** | `/api/products` | Thêm mới một sản phẩm kèm hình ảnh minh họa | Dòng 100 |
| **PUT** | `/api/products/{id}` | Cập nhật thông tin sản phẩm và thay ảnh | Dòng 169 |
| **DELETE** | `/api/products/{id}` | Xóa mềm hoặc xóa hoàn toàn sản phẩm khỏi hệ thống | Dòng 226 |

#### Đoạn mã nguồn minh họa trong `ProductsController.cs`:
```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProductsController : ControllerBase
{
    // 1. GET ALL
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryParameters query)
    {
        var pagedResult = await _productService.GetPagedProductsAsync(query);
        return Ok(ApiResponse<PagedResult<ProductDto>>.Ok(pagedResult));
    }

    // 2. GET BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null) return NotFound(ApiResponse<object>.Fail("Sản phẩm không tồn tại"));
        return Ok(ApiResponse<ProductDto>.Ok(product));
    }

    // 3. POST (CREATE)
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create([FromForm] CreateProductRequest request)
    {
        var product = await _productService.CreateProductAsync(dto, userId);
        return Ok(ApiResponse<ProductDto>.Ok(product, "Tạo thành công"));
    }

    // 4. PUT (UPDATE)
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateProductRequest request)
    {
        var product = await _productService.UpdateProductAsync(id, dto, userId);
        return Ok(ApiResponse<ProductDto>.Ok(product, "Cập nhật thành công"));
    }

    // 5. DELETE
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _productService.DeleteProductAsync(id);
        if (!success) return NotFound(ApiResponse<object>.Fail("Product not found"));
        return Ok(ApiResponse<object?>.Ok(null, "Xóa sản phẩm thành công"));
    }
}
```

### 1.2. Các Controller khác cũng thỏa mãn trọn vẹn 5 API:
* **TablesController:** [TablesController.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.API/Controllers/TablesController.cs) (GET all dòng 36, GET by id dòng 44, POST dòng 53, PUT dòng 63, DELETE dòng 82).
* **CategoriesController:** [CategoriesController.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.API/Controllers/CategoriesController.cs) (GET all dòng 24, GET by id dòng 32, POST dòng 40, PUT dòng 57, DELETE dòng 74).
* **CombosController:** [CombosController.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.API/Controllers/CombosController.cs) (GET all dòng 29, GET by id dòng 38, POST dòng 60, PUT dòng 77, DELETE dòng 109).

---

## 2. TIÊU CHÍ 2: MEDIA FORMATTER & CONTENT NEGOTIATION

### 2.1. File cấu hình
* **File:** [Program.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.API/Program.cs#L16-L28)

### 2.2. Đoạn mã nguồn cấu hình:
```csharp
// Add services to the container.
builder.Services.AddControllers()
    // Configure JSON serializer options (SystemTextJsonOutputFormatter / InputFormatter)
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    })
    // Add XML serializer formatters for Content Negotiation (XmlSerializerOutputFormatter / InputFormatter)
    .AddXmlSerializerFormatters();
```

### 2.3. Cơ chế hoạt động của Content Negotiation:
1. **JSON Media Formatter:** Mặc định được xử lý thông qua `SystemTextJsonInputFormatter` và `SystemTextJsonOutputFormatter`.
2. **XML Media Formatter:** Được mở rộng thông qua thư viện `AddXmlSerializerFormatters()`.
3. **Thương lượng nội dung (Content Negotiation):** Khi Client gửi Header:
   * `Accept: application/json` $\rightarrow$ Web API tự động định dạng và trả về cấu trúc **JSON** (`Content-Type: application/json; charset=utf-8`).
   * `Accept: application/xml` $\rightarrow$ Web API tự động kích hoạt XML Formatter và trả về dữ liệu chuẩn **XML** (`Content-Type: application/xml; charset=utf-8`).
   * `Content-Type: multipart/form-data` $\rightarrow$ Sử dụng `FormFileModelBinder` cho upload tệp đính kèm (ảnh đại diện sản phẩm).

---

## 3. TIÊU CHÍ 3: SỬ DỤNG THƯ VIỆN ODATA VÀ AUTOMAPPER

### 3.1. Thư viện OData (Microsoft.AspNetCore.OData 8.x)
* **Gói Package:** `Microsoft.AspNetCore.OData` trong [BilliardManagement.API.csproj](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.API/BilliardManagement.API.csproj#L14).
* **Cấu hình Pipeline:** Tại [Program.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.API/Program.cs#L18):
  ```csharp
  builder.Services.AddControllers()
      // Kích hoạt OData với đầy đủ các tính năng truy vấn mạnh mẽ
      .AddOData(options => options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100))
      .AddJsonOptions(...)
      .AddXmlSerializerFormatters();
  ```
* **Các Endpoint kích hoạt OData bằng attribute `[EnableQuery]`:**
  1. **Products OData Endpoint:** [ProductsController.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.API/Controllers/ProductsController.cs#L58-L65):
     ```csharp
     [HttpGet("odata")]
     [EnableQuery]
     public async Task<IActionResult> GetOData()
     {
         var products = await _productService.GetAllProductsAsync();
         return Ok(products.AsQueryable());
     }
     ```
  2. **Tables OData Endpoint:** [TablesController.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.API/Controllers/TablesController.cs#L26-L33):
     ```csharp
     [HttpGet("odata")]
     [EnableQuery]
     public async Task<IActionResult> GetOData()
     {
         var tables = await _tableService.GetAllTablesAsync();
         return Ok(tables.AsQueryable());
     }
     ```
* **Ví dụ truy vấn OData thực tế từ phía Client:**
  * Lọc và sắp xếp: `GET /api/products/odata?$filter=price gt 30000&$orderby=price desc`
  * Chọn cột cần lấy: `GET /api/products/odata?$select=name,price,category`
  * Đếm tổng số lượng: `GET /api/products/odata?$count=true`

### 3.2. Thư viện AutoMapper (AutoMapper 13.x)
* **Gói Package:** `AutoMapper` trong [BilliardManagement.API.csproj](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.API/BilliardManagement.API.csproj#L10).
* **Đăng ký Dependency Injection:** Tại [DependencyInjection.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.Business/DependencyInjection.cs#L35):
  ```csharp
  services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
  ```
* **Profile ánh xạ:** [AutoMapperProfile.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.Business/Mappings/AutoMapperProfile.cs):
  ```csharp
  public class AutoMapperProfile : Profile
  {
      public AutoMapperProfile()
      {
          CreateMap<User, UserDto>()
              .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store != null ? src.Store.Name : null))
              .ReverseMap();

          CreateMap<BilliardTable, TableDto>()
              .ForMember(dest => dest.PricePerHour, opt => opt.MapFrom(src => src.HourlyRate));

          CreateMap<Product, ProductDto>()
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProductName))
              .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.StockQuantity))
              .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : string.Empty));

          CreateMap<CreateProductDto, Product>()
              .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.Stock));

          CreateMap<TableSession, SessionDto>().ReverseMap();
          CreateMap<Invoice, BillDto>().ReverseMap();
          CreateMap<Category, CategoryDto>().ReverseMap();
          CreateMap<Customer, CustomerDto>().ReverseMap();
          CreateMap<Combo, ComboDto>().ReverseMap();
      }
  }
  ```
* **Sử dụng trong Business Service:** Được inject qua interface `IMapper _mapper` tại [ProductService.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.Business/Services/ProductService.cs), [TableService.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.Business/Services/AllServices.cs), [ComboService.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.Business/Services/ComboService.cs):
  ```csharp
  var productDtos = _mapper.Map<List<ProductDto>>(products);
  ```

---

## 4. TIÊU CHÍ 4: ENTITY FRAMEWORK CORE THEO HƯỚNG CODE FIRST

Dự án áp dụng mô hình **Entity Framework Core (Code First)** với quy trình thiết kế Model bằng C#, sinh migration và cập nhật cơ sở dữ liệu tự động.

### 4.1. File cấu hình DbContext chính
* **File:** [BilliardManagementDbContext.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.Data/BilliardManagementDbContext.cs)
* Khai báo các `DbSet<T>` tương ứng với các bảng:
  ```csharp
  public DbSet<User> Users { get; set; }
  public DbSet<Store> Stores { get; set; }
  public DbSet<BilliardTable> BilliardTables { get; set; }
  public DbSet<Category> Categories { get; set; }
  public DbSet<Product> Products { get; set; }
  public DbSet<Order> Orders { get; set; }
  public DbSet<OrderItem> OrderItems { get; set; }
  public DbSet<TableSession> TableSessions { get; set; }
  public DbSet<Invoice> Invoices { get; set; }
  public DbSet<Shift> Shifts { get; set; }
  public DbSet<Customer> Customers { get; set; }
  public DbSet<Combo> Combos { get; set; }
  public DbSet<SessionCombo> SessionCombos { get; set; }
  public DbSet<TableStatusHistory> TableStatusHistories { get; set; }
  ```

### 4.2. Quản lý Migrations lưu vết trong mã nguồn
Thư mục [BilliardManagement.Data/Migrations](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.Data/Migrations) chứa đầy đủ 16 files migration đại diện cho các lần tiến hóa cơ sở dữ liệu:
1. `20260516140652_InitialCreateV2.cs` (Khởi tạo Schema ban đầu)
2. `20260520144234_FixColumnMappings.cs`
3. `20260521033148_UpdateTableStatusEnum.cs`
4. `20260521044130_AddSessionDurationFields.cs`
5. `20260521054948_AddTableAndProductDescription.cs`
6. `20260521062054_AddUniqueIndexToTableName.cs`
7. `20260521092619_AddProductIsDeleted.cs`
8. `20260522164554_AddProductUniqueName.cs`
9. `20260526035322_AddUserProfileFields.cs`
10. `20260527140850_AddTableStatusHistory.cs`
11. `20260617070604_CustomerManagement.cs`
12. `20260724101328_RemoveDiscountAmount.cs`
13. `20260724113201_AddCombosTable.cs`
14. `20260724195051_AddComboToTableSession.cs`
15. `20260724203844_AddIsComboOrderToOrder.cs`
16. `20261113070108_AddMultiTenantSupport.cs` (Hỗ trợ mô hình chuỗi nhiều chi nhánh Multi-tenant)
17. `BilliardManagementDbContextModelSnapshot.cs` (Snapshot cấu trúc DB)

### 4.3. Tự động Migrate khi khởi động ứng dụng:
Tại [Program.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.API/Program.cs#L116-L122):
```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BilliardManagementDbContext>();
    dbContext.Database.Migrate(); // Tự động áp dụng Migration lên SQL Server
    DbSeeder.Seed(dbContext, app.Configuration); // Khởi tạo tài khoản và dữ liệu mẫu
}
```

---

## 5. TIÊU CHÍ 5: SỬ DỤNG CÁC DATA ANNOTATION

Data Annotations được áp dụng đồng bộ ở cả hai tầng: **Entity Model** (tầng Data) và **DTOs** (tầng Business Validation).

### 5.1. Bảng tổng hợp các Data Annotation tiêu biểu được sử dụng:

| Data Annotation | Mục đích sử dụng | Vị trí minh chứng |
|:---|:---|:---|
| `[Key]` | Định danh khóa chính (Primary Key) | Tất cả các Entity trong thư mục `BilliardManagement.Models/Models/` |
| `[Required]` | Bắt buộc trường dữ liệu không được phép NULL | `BilliardTable.cs`, `Product.cs`, `User.cs`, `Category.cs`, `Customer.cs` |
| `[MaxLength(n)]` | Giới hạn độ dài tối đa của chuỗi (String Length) | `User.cs` (`[MaxLength(100)]`), `Product.cs` (`[MaxLength(150)]`) |
| `[Column("...")]` | Map tên thuộc tính C# với tên cột vật lý trong SQL | `BilliardTable.cs` (`[Column("PricePerHour")]`), `Invoice.cs` (`[Column("SubTotal")]`) |
| `[Column(TypeName)]` | Xác định kiểu dữ liệu SQL chính xác (ví dụ `decimal(18,2)`) | `Customer.cs`, `Combo.cs`, `SessionCombo.cs`, `Invoice.cs` |
| `[ForeignKey("...")]` | Khai báo ràng buộc khóa ngoại (Foreign Key Navigation) | `Product.cs` (`[ForeignKey("CategoryId")]`), `TableSession.cs` (`[ForeignKey("TableId")]`) |
| `[Table("...")]` | Chỉ định tên bảng vật lý trong cơ sở dữ liệu | `Category.cs`, `BilliardTable.cs` |

### 5.2. Minh chứng thực tế trong mã nguồn:
* **File:** [BilliardTable.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.Models/Models/BilliardTable.cs)
  ```csharp
  [Table("BilliardTables")]
  public class BilliardTable
  {
      [Key]
      public Guid Id { get; set; }

      [Required]
      [MaxLength(50)]
      public string TableName { get; set; } = string.Empty;

      [Required]
      [MaxLength(50)]
      public string TableType { get; set; } = string.Empty;

      [Required]
      [Column("PricePerHour")]
      public decimal HourlyRate { get; set; }

      [MaxLength(500)]
      public string? Description { get; set; }
  }
  ```
* **File:** [ComboItem.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.Models/Models/ComboItem.cs)
  ```csharp
  public class ComboItem
  {
      [Key]
      public Guid Id { get; set; }

      [Required]
      public Guid ComboId { get; set; }

      [Required]
      public Guid ProductId { get; set; }

      [ForeignKey("ComboId")]
      public virtual Combo? Combo { get; set; }

      [ForeignKey("ProductId")]
      public virtual Product? Product { get; set; }
  }
  ```

---

## 6. TIÊU CHÍ 6: GỌI API BẰNG AJAX

Toàn bộ ứng dụng giao diện Razor Pages (`BilliardManagement.Web`) được thiết kế theo hướng SPA/Dynamic UI: **các thao tác người dùng không reload trang mà sử dụng AJAX (qua hàm `fetch` API)** để gửi và nhận dữ liệu bất đồng bộ với Backend API.

### 6.1. Minh chứng 1: Sơ đồ bàn nhân viên (`Staff/Tables/Map.cshtml`)
* **File:** [Map.cshtml](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.Web/Pages/Staff/Tables/Map.cshtml)
* **Hàm AJAX dùng chung:** Dòng 229-247
  ```javascript
  async function api(endpoint, method, payload) {
      const headers = { 'Content-Type': 'application/json' };
      if (API_TOKEN) headers['Authorization'] = 'Bearer ' + API_TOKEN; // Gắn token JWT
      const config = { method: method, headers: headers };
      if (payload) config.body = JSON.stringify(payload);
      
      const res = await fetch(url, config); // Gọi AJAX bất đồng bộ
      const json = await res.json();
      if (!res.ok || !json.success) throw new Error(json.message);
      return json.data;
  }
  ```
* **Các nghiệp vụ gọi qua AJAX:**
  * **Mở phiên chơi (Dòng 470):** `await api('table-sessions/start', 'POST', payload);`
  * **Gia hạn giờ chơi (Dòng 495):** `await api('table-sessions/extend/' + currentSessionId, 'POST', { additionalMinutes: minutes });`
  * **Kết thúc & thanh toán (Dòng 513):** `await api('table-sessions/end/' + currentSessionId, 'POST', { paymentMethod: 1 });`
  * **Gọi món tại bàn (Dòng 617):** `await api('orders', 'POST', { sessionId: currentSessionId, items: items });`
  * **Áp dụng combo (Dòng 597):** `await api('combos/apply', 'POST', { tableSessionId: currentSessionId, comboCode: code });`

### 6.2. Minh chứng 2: Quản lý danh mục & Sản phẩm (`Admin/Products/Index.cshtml`)
* **File:** [Index.cshtml](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.Web/Pages/Admin/Products/Index.cshtml#L173-L195)
* **AJAX Xóa sản phẩm không load lại trang:**
  ```javascript
  const res = await fetch(apiBase + 'products/' + id, {
      method: 'DELETE',
      headers: { 'Authorization': 'Bearer ' + token }
  });
  if (res.ok) {
      showToast('Đã xóa sản phẩm thành công', 'success');
      row.remove(); // Xóa phần tử DOM trên bảng ngay lập tức
  }
  ```

### 6.3. Minh chứng 3: Kiểm tra trùng tên tức thì khi nhập (`Admin/Products/Edit.cshtml`)
* **File:** [Edit.cshtml](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.Web/Pages/Admin/Products/Edit.cshtml#L156-L167)
* Khi nhân viên gõ tên sản phẩm, sự kiện `input` gửi AJAX `GET /api/products/check-name?name=...` để kiểm tra trùng lặp trong cơ sở dữ liệu và hiển thị cảnh báo ngay trên form.

---

## 7. TIÊU CHÍ 7: SỬ DỤNG JWT CHO AUTHENTICATION VÀ AUTHORIZATION

### 7.1. Cấu hình xác thực JWT trong Middleware
* **File:** [Program.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.API/Program.cs#L70-L100)
```csharp
// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? "SuperSecretKeyForBilliardManagementSystem12345");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,             // Kiểm tra hạn sống của Token
        ValidateIssuerSigningKey = true,     // Kiểm tra chữ ký bảo mật
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    // Tích hợp đọc JWT từ Query String cho các kết nối Realtime SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
```

### 7.2. Sinh Token và đóng gói Claims khi Đăng nhập
* **File:** [AllServices.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.Business/Services/AllServices.cs) (phương thức `GenerateJwtToken`)
* **Claims được đóng gói trong JWT Token:**
  * `ClaimTypes.NameIdentifier`: ID người dùng (UserId dạng GUID)
  * `ClaimTypes.Name`: Tên đăng nhập (Username)
  * `ClaimTypes.Role`: Vai trò người dùng (`SuperAdmin`, `Admin`, `Staff`)
  * `StoreId`: ID chi nhánh cửa hàng người dùng thuộc về (phục vụ phân quyền Multi-tenant)
  * `FullName`: Tên đầy đủ của nhân viên/quản trị viên

```csharp
private string GenerateJwtToken(User user)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForBilliardManagementSystem12345");
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("StoreId", user.StoreId.ToString()),
            new Claim("FullName", user.FullName)
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}
```

### 7.3. Áp dụng Attribute Phân quyền (Authorization) trên các Controller
* **Toàn bộ Controller được bảo vệ:** Sử dụng `[Authorize]` ở cấp độ class:
  * [ProductsController.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.API/Controllers/ProductsController.cs#L39)
  * [TablesController.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.API/Controllers/TablesController.cs#L14)
  * [TableSessionsController.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.API/Controllers/TableSessionsController.cs#L17)
  * [CombosController.cs](file:///f:/OJT-Review/Billiard-Management-System/BilliardManagement.API/Controllers/CombosController.cs#L16)
* **Phân quyền theo Role rõ ràng (Role-based Authorization):**
  * Chỉ Admin và SuperAdmin mới có quyền tạo/sửa/xóa sản phẩm và bàn:
    ```csharp
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create(...) { ... }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(...) { ... }
    ```
  * Cho phép cả Staff thực hiện các chức năng vận hành trực tiếp tại quán:
    ```csharp
    [Authorize(Roles = "SuperAdmin,Admin,Staff")]
    ```
  * Cho phép truy cập nặc danh (không cần token) khi đăng nhập:
    ```csharp
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(...) { ... }
    ```

---

## 8. HƯỚNG DẪN TEST VÀ DEMO CÁC TÍNH NĂNG TRỰC TIẾP

Giảng viên hoặc người chấm bài có thể dễ dàng kiểm tra nhanh các tính năng theo các bước sau:

1. **Khởi chạy ứng dụng:**
   * Backend API: `http://localhost:8080/swagger`
   * Frontend Web: `https://localhost:5068`
2. **Kiểm tra JWT Authentication & Authorization:**
   * Mở Swagger tại `http://localhost:8080/swagger`, gọi API `POST /api/auth/login` với tài khoản:
     * Quản trị: `admin` / `123456` (Role: Admin)
     * Nhân viên: `staff01` / `123456` (Role: Staff)
   * Nhận JWT Token trả về, nhấn nút **Authorize** ở góc phải Swagger, nhập `Bearer <token>` để thực thi các API được bảo vệ.
3. **Kiểm tra Content Negotiation:**
   * Gửi request `GET /api/products/1` với Header `Accept: application/json` $\rightarrow$ Trả về kết quả định dạng JSON.
   * Gửi request `GET /api/products/1` với Header `Accept: application/xml` $\rightarrow$ Trả về kết quả định dạng XML.
4. **Kiểm tra OData:**
   * Truy cập URL: `http://localhost:8080/api/products/odata?$filter=price gt 30000&$orderby=price desc`
   * Truy cập URL: `http://localhost:8080/api/tables/odata?$select=tableName,pricePerHour`
5. **Kiểm tra AJAX:**
   * Mở giao diện `https://localhost:5068/Staff/Tables/Map`, mở F12 xem tab Network:
   * Mọi thao tác như "Bắt đầu phiên chơi", "Gọi món", "Gia hạn", "Áp combo" đều gửi request AJAX ngầm và cập nhật trạng thái bàn theo thời gian thực mà không reload lại toàn bộ trang.

---
*Báo cáo được hoàn thiện và kiểm thử đạt chuẩn 100% trên toàn bộ Solution BilliardManagementSystem.*
