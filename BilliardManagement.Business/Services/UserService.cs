using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Data.Repositories.Interfaces;
using BilliardManagement.Models.Enums;
using BilliardManagement.Models.Models;
using BilliardManagement.Common.Exceptions;
using BilliardManagement.Common.Tenant;
using BilliardManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BilliardManagement.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly BilliardManagementDbContext _context;
        private readonly ITenantContext _tenantContext;

        // Constructor
        public UserService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            BilliardManagementDbContext context,
            ITenantContext tenantContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _tenantContext = tenantContext;
        }

        // Methods
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            // SuperAdmin actors remain visible to required navigation joins; root user
            // endpoints still expose only accounts belonging to the caller's store.
            var users = await _context.Users.Include(u => u.Store)
                .Where(u => (_tenantContext.IsSuperAdmin && _tenantContext.StoreId == Guid.Empty) ||
                    u.StoreId == _tenantContext.StoreId).ToListAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            if (!Enum.IsDefined(typeof(UserRole), dto.Role))
                throw new CustomException("Vai trò không hợp lệ", 400);
            if (string.IsNullOrWhiteSpace(dto.FullName))
                throw new CustomException("Họ tên không được để trống", 400);
            if (string.IsNullOrWhiteSpace(dto.Username))
                throw new CustomException("Tên đăng nhập không được để trống", 400);
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                throw new CustomException("Mật khẩu phải có ít nhất 6 ký tự", 400);

            if (!_tenantContext.IsResolved)
                throw new CustomException("Không xác định được ngữ cảnh cửa hàng", 403);

            var role = dto.Role;
            Guid? storeId;

            if (_tenantContext.IsSuperAdmin)
            {
                if (role == UserRole.SuperAdmin)
                {
                    storeId = null;
                }
                else
                {
                    if (!dto.StoreId.HasValue || dto.StoreId == Guid.Empty)
                        throw new CustomException("SuperAdmin phải chọn cửa hàng khi tạo Admin/Staff", 400);
                    storeId = dto.StoreId;
                }
            }
            else
            {
                if (role != UserRole.Staff)
                    throw new CustomException("Admin chỉ được tạo tài khoản Staff cho cửa hàng của mình", 403);

                // Never trust StoreId sent by a non-SuperAdmin client.
                storeId = _tenantContext.StoreId;
            }

            if (storeId.HasValue)
            {
                var storeExists = await _context.Stores.AnyAsync(s => s.Id == storeId.Value && s.IsActive);
                if (!storeExists)
                    throw new CustomException("Cửa hàng không tồn tại hoặc đã ngừng hoạt động", 400);
            }

            // Username is used by login so it must stay unique across the whole system.
            var usernameExists = await _context.Users
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Username == dto.Username);
            if (usernameExists)
                throw new CustomException("Tên đăng nhập đã tồn tại", 400);

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Username = dto.Username.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                Email = dto.Email?.Trim(),
                Role = role,
                StoreId = storeId
            };

            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var created = await _context.Users
                .IgnoreQueryFilters()
                .Include(u => u.Store)
                .FirstAsync(u => u.Id == user.Id);

            return _mapper.Map<UserDto>(created);
        }

        // Lấy thông tin người dùng theo ID
        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users.Include(u => u.Store)
                .FirstOrDefaultAsync(u => u.Id == id &&
                    ((_tenantContext.IsSuperAdmin && _tenantContext.StoreId == Guid.Empty) ||
                     u.StoreId == _tenantContext.StoreId));
            if (user == null) throw new CustomException("User not found", 404);
            return _mapper.Map<UserDto>(user);
        }

        // Cập nhật vai trò người dùng
        public async Task<UserDto> UpdateUserRoleAsync(Guid id, UserRole role)
        {
            if (!Enum.IsDefined(typeof(UserRole), role))
                throw new CustomException("Vai trò không hợp lệ", 400);
            var user = await _unitOfWork.Repository<User>().GetFirstOrDefaultAsync(u => u.Id == id, "Store");
            if (user == null) throw new CustomException("User not found", 404);

            if (!_tenantContext.IsSuperAdmin)
            {
                if (role != UserRole.Staff)
                    throw new CustomException("Admin chỉ được gán vai trò Staff trong cửa hàng của mình", 403);
                if (user.Role != UserRole.Staff)
                    throw new CustomException("Admin không được thay đổi vai trò của Admin hoặc SuperAdmin", 403);
                if (user.StoreId != _tenantContext.StoreId)
                    throw new CustomException("Không có quyền cập nhật người dùng của cửa hàng khác", 403);
            }

            if (role == UserRole.SuperAdmin)
            {
                user.StoreId = null;
            }
            else if (!user.StoreId.HasValue)
            {
                if (!_tenantContext.IsSuperAdmin || _tenantContext.StoreId == Guid.Empty)
                    throw new CustomException("Người dùng phải thuộc một cửa hàng", 400);
                user.StoreId = _tenantContext.StoreId;
            }

            user.Role = role;
            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> AssignAdminStoreAsync(Guid id, Guid storeId)
        {
            if (!_tenantContext.IsSuperAdmin)
                throw new CustomException("Chỉ SuperAdmin được gán Admin vào cửa hàng", 403);

            var user = await _context.Users.IgnoreQueryFilters().Include(u => u.Store)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) throw new CustomException("Không tìm thấy người dùng", 404);
            if (user.Role == UserRole.SuperAdmin)
                throw new CustomException("SuperAdmin không thuộc cửa hàng", 400);
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == storeId && s.IsActive);
            if (store == null) throw new CustomException("Cửa hàng không tồn tại hoặc đã ngừng hoạt động", 400);

            // Existing operational history must remain attached to its original store.
            if (user.StoreId != storeId &&
                (await _context.TableSessions.IgnoreQueryFilters().AnyAsync(s => s.UserId == id) ||
                 await _context.Orders.IgnoreQueryFilters().AnyAsync(o => o.OrderedBy == id) ||
                 await _context.Shifts.IgnoreQueryFilters().AnyAsync(s => s.UserId == id) ||
                 await _context.TableStatusHistories.IgnoreQueryFilters().AnyAsync(h => h.ChangedById == id)))
                throw new CustomException("Tài khoản đã có lịch sử nghiệp vụ; hãy tạo Admin mới cho cửa hàng đích", 400);

            user.StoreId = store.Id;
            user.Store = store;
            user.Role = UserRole.Admin;
            await _context.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);
        }

        private async Task<User> FindStoreAdminAsync(Guid id)
        {
            if (!_tenantContext.IsSuperAdmin)
                throw new CustomException("Chỉ SuperAdmin được quản lý Admin cửa hàng", 403);
            var user = await _context.Users.IgnoreQueryFilters().Include(u => u.Store)
                .FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Admin);
            return user ?? throw new CustomException("Không tìm thấy Admin cửa hàng", 404);
        }

        public async Task<UserDto> UpdateStoreAdminAsync(Guid id, UpdateStoreAdminDto dto)
        {
            var user = await FindStoreAdminAsync(id);
            if (string.IsNullOrWhiteSpace(dto.FullName)) throw new CustomException("Vui lòng nhập họ tên", 400);
            user.FullName = dto.FullName.Trim();
            user.PhoneNumber = dto.PhoneNumber?.Trim();
            user.Email = dto.Email?.Trim();
            await _context.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> SetStoreAdminActiveAsync(Guid id, bool isActive)
        {
            var user = await FindStoreAdminAsync(id);
            user.IsActive = isActive;
            await _context.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);
        }

        // Xóa 1 nhân viên
        public async Task<UserDeletionResultDto> DeleteUserAsync(Guid id)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) throw new CustomException("User not found", 404);

            if (!_tenantContext.IsSuperAdmin && user.Role != UserRole.Staff)
                throw new CustomException("Admin chỉ được quản lý tài khoản Staff trong cửa hàng của mình", 403);

            var sessions = await _unitOfWork.Repository<TableSession>().GetAllAsync(s => s.UserId == id);
            int customersServed = sessions.Count();

            var orders = await _unitOfWork.Repository<Order>().GetAllAsync(o => o.OrderedBy == id, includeProperties: "OrderItems");
            int itemsSold = orders.SelectMany(o => o.OrderItems).Sum(oi => oi.Quantity);

            var hasDependencies = sessions.Any() || orders.Any();
            if (hasDependencies)
            {
                user.IsActive = false;
                _unitOfWork.Repository<User>().Update(user);
            }
            else
            {
                _unitOfWork.Repository<User>().Remove(user);
            }
            
            await _unitOfWork.SaveChangesAsync();
 
             return new UserDeletionResultDto
             {
                 CustomersServed = customersServed,
                 ItemsSold = itemsSold
             };
         }
 
         // Cập nhật thông tin cá nhân
         public async Task<UserDto> UpdateProfileAsync(Guid id, UpdateProfileDto dto)
         {
             var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
             if (user == null) throw new CustomException("Không tìm thấy người dùng", 404);
 
             if (string.IsNullOrWhiteSpace(dto.FullName))
                 throw new CustomException("Họ tên không được để trống", 400);
 
             user.FullName = dto.FullName.Trim();
             user.PhoneNumber = dto.PhoneNumber?.Trim();
             user.Email = dto.Email?.Trim();
             if (dto.ProfilePictureUrl != null)
             {
                 user.ProfilePictureUrl = dto.ProfilePictureUrl;
             }
 
             _unitOfWork.Repository<User>().Update(user);
             await _unitOfWork.SaveChangesAsync();
 
             return _mapper.Map<UserDto>(user);
         }
 
         // Đổi mật khẩu
         public async Task<bool> ChangePasswordAsync(Guid id, ChangePasswordDto dto)
         {
             var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
             if (user == null) throw new CustomException("Không tìm thấy người dùng", 404);
 
             if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                 throw new CustomException("Mật khẩu hiện tại không được để trống", 400);
 
             if (string.IsNullOrWhiteSpace(dto.NewPassword))
                 throw new CustomException("Mật khẩu mới không được để trống", 400);
 
             if (dto.NewPassword != dto.ConfirmNewPassword)
                 throw new CustomException("Mật khẩu mới và xác nhận mật khẩu không khớp", 400);
 
             // Verify current password
             bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
             if (!isPasswordValid)
                 throw new CustomException("Mật khẩu hiện tại không chính xác", 400);
 
             // Hash new password
             user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
 
             _unitOfWork.Repository<User>().Update(user);
             await _unitOfWork.SaveChangesAsync();
 
             return true;
         }

         // Reset mật khẩu cho nhân viên
         public async Task<string> ResetUserPasswordAsync(Guid userId, string newPassword)
         {
             var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
             if (user == null) throw new CustomException("Không tìm thấy người dùng", 404);

             if (!_tenantContext.IsSuperAdmin && user.Role != UserRole.Staff)
                 throw new CustomException("Admin chỉ được đặt lại mật khẩu cho Staff trong cửa hàng của mình", 403);

             if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
             {
                 throw new CustomException("Mật khẩu phải chứa ít nhất 8 ký tự.", 400);
             }
             bool hasLetter = false;
             bool hasDigit = false;
             foreach (char c in newPassword)
             {
                 if (char.IsLetter(c)) hasLetter = true;
                 if (char.IsDigit(c)) hasDigit = true;
             }
             if (!hasLetter || !hasDigit)
             {
                 throw new CustomException("Mật khẩu phải chứa cả chữ và số.", 400);
             }

             // Hash new password
             user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

             _unitOfWork.Repository<User>().Update(user);
             await _unitOfWork.SaveChangesAsync();

             return newPassword;
         }
     }
 }
