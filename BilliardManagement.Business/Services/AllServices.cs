using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BilliardManagement.Business.DTOs;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Data.Repositories.Interfaces;
using BilliardManagement.Models.Enums;
using BilliardManagement.Models.Models;
using BilliardManagement.Common.Exceptions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BilliardManagement.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _config = config;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _unitOfWork.Repository<User>().GetFirstOrDefaultAsync(u => u.Username == dto.Username && u.PasswordHash == dto.Password);
            if (user == null) throw new CustomException("Invalid credentials", 401);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["JwtSettings:Secret"] ?? "SuperSecretKeyForBilliardManagementSystem12345");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthResponseDto { Token = tokenHandler.WriteToken(token), User = _mapper.Map<UserDto>(user) };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existing = await _unitOfWork.Repository<User>().GetFirstOrDefaultAsync(u => u.Username == dto.Username);
            if (existing != null) throw new CustomException("Username already exists", 400);

            var user = new User
            {
                FullName = dto.FullName,
                Username = dto.Username,
                PasswordHash = dto.Password, // Should be hashed in real prod
                PhoneNumber = dto.PhoneNumber,
                Role = UserRole.Staff
            };
            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return await LoginAsync(new LoginDto { Username = dto.Username, Password = dto.Password });
        }
    }

    public class TableService : ITableService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TableService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TableDto> CreateTableAsync(CreateTableDto dto)
        {
            var table = _mapper.Map<BilliardTable>(dto);
            table.Status = TableStatus.Empty;
            await _unitOfWork.Repository<BilliardTable>().AddAsync(table);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<TableDto>(table);
        }

        public async Task<IEnumerable<TableDto>> GetAllTablesAsync()
        {
            var tables = await _unitOfWork.Repository<BilliardTable>().GetAllAsync();
            return _mapper.Map<IEnumerable<TableDto>>(tables);
        }

        public async Task<TableDto> GetTableByIdAsync(Guid id)
        {
            var table = await _unitOfWork.Repository<BilliardTable>().GetByIdAsync(id);
            if (table == null) throw new CustomException("Table not found", 404);
            return _mapper.Map<TableDto>(table);
        }

        public async Task<TableDto> UpdateTableStatusAsync(Guid id, TableStatus status)
        {
            var table = await _unitOfWork.Repository<BilliardTable>().GetByIdAsync(id);
            if (table == null) throw new CustomException("Table not found", 404);

            table.Status = status;
            _unitOfWork.Repository<BilliardTable>().Update(table);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<TableDto>(table);
        }
    }
}
