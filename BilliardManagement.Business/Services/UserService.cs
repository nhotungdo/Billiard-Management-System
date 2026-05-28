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

namespace BilliardManagement.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        // Constructor
        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // Methods
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Repository<User>().GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        // Lấy thông tin người dùng theo ID
        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) throw new CustomException("User not found", 404);
            return _mapper.Map<UserDto>(user);
        }

        // Cập nhật vai trò người dùng
        public async Task<UserDto> UpdateUserRoleAsync(Guid id, UserRole role)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) throw new CustomException("User not found", 404);

            user.Role = role;
            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        // Xóa 1 nhân viên
        public async Task<UserDeletionResultDto> DeleteUserAsync(Guid id)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) throw new CustomException("User not found", 404);

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
