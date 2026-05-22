using System;
using System.Collections.Generic;
using System.Linq;
using BilliardManagement.Models.Models;
using BilliardManagement.Models.Enums;
using BCrypt.Net;

namespace BilliardManagement.Data
{
    public static class DbSeeder
    {
        public static void Seed(BilliardManagementDbContext context)
        {
            // Clean up old plain text 'staff' user if exists
            var oldStaff = context.Users.FirstOrDefault(u => u.Username == "staff");
            if (oldStaff != null)
            {
                context.Users.Remove(oldStaff);
                context.SaveChanges();
            }

            // 1. Seed Users
            var adminUser = context.Users.FirstOrDefault(u => u.Username == "admin");
            if (adminUser == null)
            {
                adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Quản trị viên",
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(adminUser);
            }
            else if (!adminUser.PasswordHash.StartsWith("$2"))
            {
                adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");
                context.Users.Update(adminUser);
            }

            var staff01 = context.Users.FirstOrDefault(u => u.Username == "staff01");
            if (staff01 == null)
            {
                staff01 = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Nhân viên 01",
                    Username = "staff01",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Role = UserRole.Staff,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(staff01);
            }
            else if (!staff01.PasswordHash.StartsWith("$2"))
            {
                staff01.PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");
                context.Users.Update(staff01);
            }

            var staff02 = context.Users.FirstOrDefault(u => u.Username == "staff02");
            if (staff02 == null)
            {
                staff02 = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Nhân viên 02",
                    Username = "staff02",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Role = UserRole.Staff,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(staff02);
            }
            else if (!staff02.PasswordHash.StartsWith("$2"))
            {
                staff02.PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");
                context.Users.Update(staff02);
            }
            context.SaveChanges();

            // 2. Seed BilliardTables
            if (!context.BilliardTables.Any())
            {
                var tables = new List<BilliardTable>
                {
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Bàn 01 (Pool 8)", TableType = "Pool 8 Ball", HourlyRate = 60000, Status = TableStatus.Available, IsActive = true },
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Bàn 02 (Pool 8)", TableType = "Pool 8 Ball", HourlyRate = 60000, Status = TableStatus.Available, IsActive = true },
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Bàn 03 (Pool 9)", TableType = "Pool 9 Ball", HourlyRate = 70000, Status = TableStatus.Reserved, IsActive = true },
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Bàn 04 (Pool 9)", TableType = "Pool 9 Ball", HourlyRate = 70000, Status = TableStatus.Maintenance, IsActive = true },
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Bàn 05 (Snooker)", TableType = "Snooker", HourlyRate = 90000, Status = TableStatus.Available, IsActive = true },
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Bàn 06 (Snooker)", TableType = "Snooker", HourlyRate = 90000, Status = TableStatus.Reserved, IsActive = true },
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Bàn 07 (VIP)", TableType = "VIP", HourlyRate = 120000, Status = TableStatus.Available, IsActive = true },
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Bàn 08 (VIP)", TableType = "VIP", HourlyRate = 120000, Status = TableStatus.Maintenance, IsActive = true },
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Bàn 09 (Pool 8)", TableType = "Pool 8 Ball", HourlyRate = 60000, Status = TableStatus.Reserved, IsActive = true },
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Bàn 10 (VIP)", TableType = "VIP", HourlyRate = 120000, Status = TableStatus.Available, IsActive = true }
                };
                tables.AddRange(new[]
                {
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Bàn 11", TableType = "VIP", HourlyRate = 120000, Status = TableStatus.Available, Description = "Bàn VIP tầng 2", IsActive = true },
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Bàn 12", TableType = "Pool 8 Ball", HourlyRate = 65000, Status = TableStatus.Available, Description = "Góc sảnh chính", IsActive = true }
                });
                context.BilliardTables.AddRange(tables);
                context.SaveChanges();
            }
            else if (!context.BilliardTables.Any(t => t.TableName == "Bàn 11"))
            {
                context.BilliardTables.AddRange(
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Bàn 11", TableType = "VIP", HourlyRate = 120000, Status = TableStatus.Available, Description = "Bàn VIP tầng 2", IsActive = true },
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Bàn 12", TableType = "Pool 8 Ball", HourlyRate = 65000, Status = TableStatus.Available, Description = "Góc sảnh chính", IsActive = true });
                context.SaveChanges();
            }
            
            if (!context.BilliardTables.Any(t => t.TableName == "Bàn VIP 01"))
            {
                context.BilliardTables.AddRange(
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Bàn VIP 01", TableType = "VIP", HourlyRate = 150000, Status = TableStatus.Available, Description = "Bàn VIP cách âm", IsActive = true },
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Bàn Tournament 01", TableType = "Bàn thi đấu", HourlyRate = 200000, Status = TableStatus.Available, Description = "Bàn chuẩn thi đấu", IsActive = true },
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Phòng Couple 01", TableType = "Phòng đôi", HourlyRate = 180000, Status = TableStatus.Available, Description = "Phòng riêng cho 2 người", IsActive = true },
                    new BilliardTable { Id = Guid.NewGuid(), TableName = "Bàn Carom 01", TableType = "Carom", HourlyRate = 80000, Status = TableStatus.Available, Description = "Bàn Carom tiêu chuẩn", IsActive = true });
                context.SaveChanges();
            }

            var table1 = context.BilliardTables.First(t => t.TableName == "Bàn 01 (Pool 8)");
            var table2 = context.BilliardTables.First(t => t.TableName == "Bàn 02 (Pool 8)");
            var table3 = context.BilliardTables.First(t => t.TableName == "Bàn 03 (Pool 9)");
            var table7 = context.BilliardTables.First(t => t.TableName == "Bàn 07 (VIP)");

            // 3. Seed Categories
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Id = Guid.NewGuid(), CategoryName = "Nước ngọt", Description = "Các loại nước ngọt đóng lon" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "Cafe", Description = "Cafe pha máy và pha phin" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "Bia", Description = "Bia lon và chai phục vụ khách" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "Đồ ăn vặt", Description = "Đồ ăn vặt, thức ăn nhanh" },
                    new Category { Id = Guid.NewGuid(), CategoryName = "Trà sữa", Description = "Các loại trà sữa và trà hoa quả" }
                };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            var catNuocNgot = context.Categories.First(c => c.CategoryName == "Nước ngọt");
            var catCafe = context.Categories.First(c => c.CategoryName == "Cafe");
            var catBia = context.Categories.First(c => c.CategoryName == "Bia");
            var catSnack = context.Categories.First(c => c.CategoryName == "Đồ ăn vặt");
            var catTraSua = context.Categories.First(c => c.CategoryName == "Trà sữa");

            // 4. Seed Products
            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    // Nước ngọt
                    new Product { Id = Guid.NewGuid(), CategoryId = catNuocNgot.Id, ProductName = "Coca Cola", Price = 15000, StockQuantity = 100, IsAvailable = true },
                    new Product { Id = Guid.NewGuid(), CategoryId = catNuocNgot.Id, ProductName = "Pepsi", Price = 15000, StockQuantity = 100, IsAvailable = true },
                    new Product { Id = Guid.NewGuid(), CategoryId = catNuocNgot.Id, ProductName = "Sting", Price = 17000, StockQuantity = 100, IsAvailable = true },
                    new Product { Id = Guid.NewGuid(), CategoryId = catNuocNgot.Id, ProductName = "Red Bull", Price = 20000, StockQuantity = 100, IsAvailable = true },

                    // Cafe
                    new Product { Id = Guid.NewGuid(), CategoryId = catCafe.Id, ProductName = "Cafe Đen", Price = 18000, StockQuantity = 80, IsAvailable = true },
                    new Product { Id = Guid.NewGuid(), CategoryId = catCafe.Id, ProductName = "Cafe Sữa", Price = 22000, StockQuantity = 80, IsAvailable = true },
                    new Product { Id = Guid.NewGuid(), CategoryId = catCafe.Id, ProductName = "Bạc Xỉu", Price = 25000, StockQuantity = 80, IsAvailable = true },

                    // Bia
                    new Product { Id = Guid.NewGuid(), CategoryId = catBia.Id, ProductName = "Heineken", Price = 28000, StockQuantity = 120, IsAvailable = true },
                    new Product { Id = Guid.NewGuid(), CategoryId = catBia.Id, ProductName = "Tiger", Price = 24000, StockQuantity = 150, IsAvailable = true },

                    // Snack
                    new Product { Id = Guid.NewGuid(), CategoryId = catSnack.Id, ProductName = "Khoai Tây Chiên", Price = 30000, StockQuantity = 50, IsAvailable = true },
                    new Product { Id = Guid.NewGuid(), CategoryId = catSnack.Id, ProductName = "Cá Viên Chiên", Price = 35000, StockQuantity = 50, IsAvailable = true },
                    new Product { Id = Guid.NewGuid(), CategoryId = catSnack.Id, ProductName = "Mì Ly", Price = 15000, StockQuantity = 200, IsAvailable = true },

                    // Trà sữa
                    new Product { Id = Guid.NewGuid(), CategoryId = catTraSua.Id, ProductName = "Trà Sữa", Price = 30000, StockQuantity = 60, IsAvailable = true },
                    new Product { Id = Guid.NewGuid(), CategoryId = catTraSua.Id, ProductName = "Trà Đào", Price = 28000, StockQuantity = 60, IsAvailable = true }
                };
                products.AddRange(new[]
                {
                    new Product { Id = Guid.NewGuid(), CategoryId = catNuocNgot.Id, ProductName = "7 Up", Price = 15000, StockQuantity = 80, IsAvailable = true, Description = "Nước ngọt có gas" },
                    new Product { Id = Guid.NewGuid(), CategoryId = catBia.Id, ProductName = "Saigon Special", Price = 22000, StockQuantity = 100, IsAvailable = true },
                    new Product { Id = Guid.NewGuid(), CategoryId = catTraSua.Id, ProductName = "Trà Sữa Trân Châu", Price = 35000, StockQuantity = 50, IsAvailable = true, Description = "Size M" },
                    new Product { Id = Guid.NewGuid(), CategoryId = catSnack.Id, ProductName = "Bánh Gạo", Price = 12000, StockQuantity = 60, IsAvailable = true }
                });
                context.Products.AddRange(products);
                context.SaveChanges();
            }
            else if (!context.Products.Any(p => p.ProductName == "7 Up"))
            {
                var catNn = context.Categories.First(c => c.CategoryName == "Nước ngọt");
                var catBi = context.Categories.First(c => c.CategoryName == "Bia");
                var catTs = context.Categories.First(c => c.CategoryName == "Trà sữa");
                var catSn = context.Categories.First(c => c.CategoryName == "Đồ ăn vặt");
                context.Products.AddRange(
                    new Product { Id = Guid.NewGuid(), CategoryId = catNn.Id, ProductName = "7 Up", Price = 15000, StockQuantity = 80, IsAvailable = true },
                    new Product { Id = Guid.NewGuid(), CategoryId = catBi.Id, ProductName = "Saigon Special", Price = 22000, StockQuantity = 100, IsAvailable = true },
                    new Product { Id = Guid.NewGuid(), CategoryId = catTs.Id, ProductName = "Trà Sữa Trân Châu", Price = 35000, StockQuantity = 50, IsAvailable = true },
                    new Product { Id = Guid.NewGuid(), CategoryId = catSn.Id, ProductName = "Bánh Gạo", Price = 12000, StockQuantity = 60, IsAvailable = true });
                context.SaveChanges();
            }

            var prodCoca = context.Products.First(p => p.ProductName == "Coca Cola");
            var prodSting = context.Products.First(p => p.ProductName == "Sting");
            var prodHeineken = context.Products.First(p => p.ProductName == "Heineken");
            var prodCaVien = context.Products.First(p => p.ProductName == "Cá Viên Chiên");
            var prodTraSua = context.Products.First(p => p.ProductName == "Trà Sữa");

            // 5. Seed Sessions, Orders, OrderItems, Invoices (Historical & Active)
            if (!context.TableSessions.Any())
            {
                var now = DateTime.UtcNow;

                var table5 = context.BilliardTables.First(t => t.TableName == "Bàn 05 (Snooker)");

                // --- 5.1. Active Session 1 (Table 1) — còn ~75 phút ---
                var start1 = now.AddMinutes(-45);
                var hours1 = 2;
                var sessionActive1 = new TableSession
                {
                    Id = Guid.NewGuid(),
                    TableId = table1.Id,
                    UserId = staff01.Id,
                    StartTime = start1,
                    EndTime = start1.AddHours(hours1),
                    DurationHours = hours1,
                    DurationMinutes = hours1 * 60,
                    RemainingMinutes = Math.Max(0, (int)(start1.AddHours(hours1) - now).TotalMinutes),
                    TotalPrice = table1.HourlyRate * hours1,
                    IsFinished = false,
                    Status = SessionStatus.Active,
                    CreatedAt = start1
                };
                table1.Status = TableStatus.Playing;
                context.TableSessions.Add(sessionActive1);

                // Order for Active Session 1
                var orderActive1 = new Order
                {
                    Id = Guid.NewGuid(),
                    TableSessionId = sessionActive1.Id,
                    OrderedBy = staff01.Id,
                    OrderTime = now.AddMinutes(-30),
                    Status = OrderStatus.Pending,
                    TotalAmount = 2 * prodCoca.Price + prodCaVien.Price
                };
                orderActive1.OrderItems.Add(new OrderItem { Id = Guid.NewGuid(), OrderId = orderActive1.Id, ProductId = prodCoca.Id, Quantity = 2, UnitPrice = prodCoca.Price, TotalPrice = 2 * prodCoca.Price });
                orderActive1.OrderItems.Add(new OrderItem { Id = Guid.NewGuid(), OrderId = orderActive1.Id, ProductId = prodCaVien.Id, Quantity = 1, UnitPrice = prodCaVien.Price, TotalPrice = prodCaVien.Price });
                context.Orders.Add(orderActive1);

                // --- 5.2. Active Session 2 (Table 7 - VIP) — còn ~10 phút (vàng) ---
                var start2 = now.AddHours(-2).AddMinutes(-50);
                var hours2 = 3;
                var sessionActive2 = new TableSession
                {
                    Id = Guid.NewGuid(),
                    TableId = table7.Id,
                    UserId = staff01.Id,
                    StartTime = start2,
                    EndTime = start2.AddHours(hours2),
                    DurationHours = hours2,
                    DurationMinutes = hours2 * 60,
                    RemainingMinutes = Math.Max(0, (int)(start2.AddHours(hours2) - now).TotalMinutes),
                    TotalPrice = table7.HourlyRate * hours2,
                    IsFinished = false,
                    Status = SessionStatus.Active,
                    CreatedAt = start2
                };
                table7.Status = TableStatus.Playing;
                context.TableSessions.Add(sessionActive2);

                // --- 5.2b. Active Session 3 (Table 5) — sắp hết giờ ---
                var start3 = now.AddMinutes(-55);
                var hours3 = 1;
                var sessionActive3 = new TableSession
                {
                    Id = Guid.NewGuid(),
                    TableId = table5.Id,
                    UserId = staff02.Id,
                    StartTime = start3,
                    EndTime = start3.AddHours(hours3),
                    DurationHours = hours3,
                    DurationMinutes = hours3 * 60,
                    RemainingMinutes = Math.Max(0, (int)(start3.AddHours(hours3) - now).TotalMinutes),
                    TotalPrice = table5.HourlyRate * hours3,
                    IsFinished = false,
                    Status = SessionStatus.Active,
                    CreatedAt = start3
                };
                table5.Status = TableStatus.Playing;
                context.TableSessions.Add(sessionActive3);

                // Order for Active Session 2
                var orderActive2 = new Order
                {
                    Id = Guid.NewGuid(),
                    TableSessionId = sessionActive2.Id,
                    OrderedBy = staff01.Id,
                    OrderTime = now.AddHours(-1),
                    Status = OrderStatus.Pending,
                    TotalAmount = 4 * prodHeineken.Price + 2 * prodCaVien.Price
                };
                orderActive2.OrderItems.Add(new OrderItem { Id = Guid.NewGuid(), OrderId = orderActive2.Id, ProductId = prodHeineken.Id, Quantity = 4, UnitPrice = prodHeineken.Price, TotalPrice = 4 * prodHeineken.Price });
                orderActive2.OrderItems.Add(new OrderItem { Id = Guid.NewGuid(), OrderId = orderActive2.Id, ProductId = prodCaVien.Id, Quantity = 2, UnitPrice = prodCaVien.Price, TotalPrice = 2 * prodCaVien.Price });
                context.Orders.Add(orderActive2);


                // --- 5.3. Revenue Seed - Past 5 Days ---
                for (int d = 5; d >= 0; d--)
                {
                    var day = now.AddDays(-d);
                    
                    // Morning shift session
                    var sessionTimeStart = new DateTime(day.Year, day.Month, day.Day, 9, 0, 0, DateTimeKind.Utc);
                    var sessionTimeEnd = sessionTimeStart.AddHours(2.5); // 150 minutes

                    var tableToUse = (d % 2 == 0) ? table2 : table3;
                    var pastHours = 3;
                    var sessionPast = new TableSession
                    {
                        Id = Guid.NewGuid(),
                        TableId = tableToUse.Id,
                        UserId = staff01.Id,
                        StartTime = sessionTimeStart,
                        EndTime = sessionTimeEnd,
                        DurationHours = pastHours,
                        DurationMinutes = 150,
                        RemainingMinutes = 0,
                        TotalPrice = Math.Round((decimal)(150.0 / 60.0) * tableToUse.HourlyRate),
                        IsFinished = true,
                        Status = SessionStatus.Finished,
                        CreatedAt = sessionTimeStart
                    };
                    context.TableSessions.Add(sessionPast);

                    // Order
                    var orderPast = new Order
                    {
                        Id = Guid.NewGuid(),
                        TableSessionId = sessionPast.Id,
                        OrderedBy = staff01.Id,
                        OrderTime = sessionTimeStart.AddHours(1),
                        Status = OrderStatus.Completed,
                        TotalAmount = 3 * prodSting.Price + 1 * prodTraSua.Price
                    };
                    orderPast.OrderItems.Add(new OrderItem { Id = Guid.NewGuid(), OrderId = orderPast.Id, ProductId = prodSting.Id, Quantity = 3, UnitPrice = prodSting.Price, TotalPrice = 3 * prodSting.Price });
                    orderPast.OrderItems.Add(new OrderItem { Id = Guid.NewGuid(), OrderId = orderPast.Id, ProductId = prodTraSua.Id, Quantity = 1, UnitPrice = prodTraSua.Price, TotalPrice = prodTraSua.Price });
                    context.Orders.Add(orderPast);

                    // Invoice
                    var subtotal = sessionPast.TotalPrice + orderPast.TotalAmount;
                    var discount = d % 3 == 0 ? 10000 : 0;
                    var total = subtotal - discount;

                    var invoicePast = new Invoice
                    {
                        Id = Guid.NewGuid(),
                        TableSessionId = sessionPast.Id,
                        OrderId = orderPast.Id,
                        Subtotal = subtotal,
                        Discount = discount,
                        TotalAmount = total,
                        PaymentMethod = (d % 2 == 0) ? PaymentMethod.Cash : PaymentMethod.Transfer,
                        CreatedAt = sessionTimeEnd
                    };
                    context.Invoices.Add(invoicePast);

                    // Afternoon VIP shift session
                    var sessionVipStart = new DateTime(day.Year, day.Month, day.Day, 15, 30, 0, DateTimeKind.Utc);
                    var sessionVipEnd = sessionVipStart.AddHours(3); // 180 minutes

                    var sessionPastVip = new TableSession
                    {
                        Id = Guid.NewGuid(),
                        TableId = table7.Id,
                        UserId = staff01.Id,
                        StartTime = sessionVipStart,
                        EndTime = sessionVipEnd,
                        DurationHours = 3,
                        DurationMinutes = 180,
                        RemainingMinutes = 0,
                        TotalPrice = 3 * table7.HourlyRate,
                        IsFinished = true,
                        Status = SessionStatus.Finished,
                        CreatedAt = sessionVipStart
                    };
                    context.TableSessions.Add(sessionPastVip);

                    // VIP Order
                    var orderPastVip = new Order
                    {
                        Id = Guid.NewGuid(),
                        TableSessionId = sessionPastVip.Id,
                        OrderedBy = staff01.Id,
                        OrderTime = sessionVipStart.AddHours(1),
                        Status = OrderStatus.Completed,
                        TotalAmount = 6 * prodHeineken.Price + 2 * prodCaVien.Price
                    };
                    orderPastVip.OrderItems.Add(new OrderItem { Id = Guid.NewGuid(), OrderId = orderPastVip.Id, ProductId = prodHeineken.Id, Quantity = 6, UnitPrice = prodHeineken.Price, TotalPrice = 6 * prodHeineken.Price });
                    orderPastVip.OrderItems.Add(new OrderItem { Id = Guid.NewGuid(), OrderId = orderPastVip.Id, ProductId = prodCaVien.Id, Quantity = 2, UnitPrice = prodCaVien.Price, TotalPrice = 2 * prodCaVien.Price });
                    context.Orders.Add(orderPastVip);

                    // VIP Invoice
                    var subtotalVip = sessionPastVip.TotalPrice + orderPastVip.TotalAmount;
                    var discountVip = 20000;
                    var totalVip = subtotalVip - discountVip;

                    var invoicePastVip = new Invoice
                    {
                        Id = Guid.NewGuid(),
                        TableSessionId = sessionPastVip.Id,
                        OrderId = orderPastVip.Id,
                        Subtotal = subtotalVip,
                        Discount = discountVip,
                        TotalAmount = totalVip,
                        PaymentMethod = PaymentMethod.Transfer,
                        CreatedAt = sessionVipEnd
                    };
                    context.Invoices.Add(invoicePastVip);
                }

                context.SaveChanges();
            }
            else
            {
                UpgradeActiveSessions(context);
            }
        }

        private static void UpgradeActiveSessions(BilliardManagementDbContext context)
        {
            var activeSessions = context.TableSessions
                .Where(s => s.Status == SessionStatus.Active && !s.IsFinished)
                .ToList();
            if (!activeSessions.Any()) return;

            var now = DateTime.UtcNow;
            foreach (var session in activeSessions)
            {
                var table = context.BilliardTables.Find(session.TableId);
                if (table == null) continue;

                if (session.DurationHours <= 0)
                    session.DurationHours = 2;

                if (!session.EndTime.HasValue)
                    session.EndTime = session.StartTime.AddHours(session.DurationHours);

                if (session.TotalPrice <= 0)
                    session.TotalPrice = table.HourlyRate * session.DurationHours;

                session.DurationMinutes = session.DurationHours * 60;
                session.RemainingMinutes = Math.Max(0, (int)(session.EndTime.Value - now).TotalMinutes);
                table.Status = TableStatus.Playing;
            }
            context.SaveChanges();
        }
    }
}
