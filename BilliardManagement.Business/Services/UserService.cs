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

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Repository<User>().GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) throw new CustomException("User not found", 404);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> UpdateUserRoleAsync(Guid id, UserRole role)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) throw new CustomException("User not found", 404);

            user.Role = role;
            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }
    }
}
