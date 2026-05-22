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
    }
}
