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
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BilliardManagement.Common.Responses;

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
            var user = await _unitOfWork.Repository<User>().GetFirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) throw new CustomException("Invalid credentials", 401);

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
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
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
        private readonly ILogger<TableService> _logger;

        public TableService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TableService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TableDto> CreateTableAsync(CreateTableDto dto, Guid? createdBy = null)
        {
            if (string.IsNullOrWhiteSpace(dto.TableName))
                throw new CustomException("Tên bàn không được để trống", 400);

            if (dto.PricePerHour <= 0)
                throw new CustomException("Giá theo giờ phải lớn hơn 0", 400);

            var allowedTypes = new[] { 
                "Pool 8 Ball", "Pool 9 Ball", "Snooker", "Carom", 
                "Libre", "English Billiards", "Russian Pyramid", 
                "VIP", "Phòng đôi", "Bàn thi đấu" 
            };
            if (!allowedTypes.Contains(dto.TableType?.Trim(), StringComparer.OrdinalIgnoreCase))
                throw new CustomException("Loại bàn không hợp lệ", 400);

            if (!Enum.TryParse<TableStatus>(dto.Status, true, out var status) || !Enum.IsDefined(typeof(TableStatus), status))
                throw new CustomException("Trạng thái bàn không hợp lệ", 400);

            if (status == TableStatus.Playing)
                throw new CustomException("Không thể tạo bàn mới với trạng thái Đang chơi", 400);

            var existing = await _unitOfWork.Repository<BilliardTable>()
                .AnyAsync(t => t.TableName.ToLower() == dto.TableName.Trim().ToLower());
            if (existing)
                throw new CustomException("Tên bàn đã tồn tại", 400);

            var table = _mapper.Map<BilliardTable>(dto);
            table.TableName = dto.TableName.Trim();
            table.TableType = dto.TableType.Trim();
            table.Status = status;
            table.Description = dto.Description?.Trim();

            await _unitOfWork.Repository<BilliardTable>().AddAsync(table);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Table created: tableId={TableId}, name={TableName}, type={TableType}, createdBy={CreatedBy}",
                table.Id, table.TableName, table.TableType, createdBy);

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

        public async Task<TableDto> UpdateTableStatusAsync(Guid id, TableStatus status, Guid? updatedBy = null)
        {
            var table = await _unitOfWork.Repository<BilliardTable>().GetByIdAsync(id);
            if (table == null) throw new CustomException("Table not found", 404);

            if (!Enum.IsDefined(typeof(TableStatus), status))
                throw new CustomException("Invalid table status", 400);

            var activeSessions = await _unitOfWork.Repository<TableSession>().GetAllAsync(
                s => s.TableId == id && s.Status == SessionStatus.Active && !s.IsFinished);
            var hasActiveSession = activeSessions.Any();
            var oldStatus = table.Status;

            if (status == TableStatus.Playing && !hasActiveSession)
                throw new CustomException("Cannot set Playing without an active session. Start a session first", 400);

            if (status == TableStatus.Available && hasActiveSession)
                throw new CustomException("Cannot set Available while session is active. Please end the session first", 400);

            if (status == TableStatus.Maintenance &&
                (table.Status == TableStatus.Playing || hasActiveSession))
                throw new CustomException("Cannot set Maintenance while table is playing", 400);

            table.Status = status;
            _unitOfWork.Repository<BilliardTable>().Update(table);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Table status updated: tableId={TableId}, oldStatus={OldStatus}, newStatus={NewStatus}, updatedBy={UpdatedBy}",
                id, oldStatus, status, updatedBy);

            return _mapper.Map<TableDto>(table);
        }

        public async Task<TableDto> UpdateTableAsync(Guid id, CreateTableDto dto, Guid? updatedBy = null)
        {
            if (string.IsNullOrWhiteSpace(dto.TableName))
                throw new CustomException("Tên bàn không được để trống", 400);

            if (dto.PricePerHour <= 0)
                throw new CustomException("Giá tiền mỗi giờ phải lớn hơn 0", 400);

            var allowedTypes = new[] { 
                "Pool 8 Ball", "Pool 9 Ball", "Snooker", "Carom", 
                "Libre", "English Billiards", "Russian Pyramid", 
                "VIP", "Phòng đôi", "Bàn thi đấu" 
            };
            if (!allowedTypes.Contains(dto.TableType?.Trim(), StringComparer.OrdinalIgnoreCase))
                throw new CustomException("Loại bàn không hợp lệ", 400);

            var table = await _unitOfWork.Repository<BilliardTable>().GetByIdAsync(id);
            if (table == null) throw new CustomException("Table not found", 404);

            if (!string.Equals(table.TableName.Trim(), dto.TableName.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var nameExists = await _unitOfWork.Repository<BilliardTable>()
                    .AnyAsync(t => t.Id != id && t.TableName.ToLower() == dto.TableName.Trim().ToLower());
                if (nameExists)
                    throw new CustomException("Tên bàn đã tồn tại", 400);
            }

            table.TableName = dto.TableName.Trim();
            table.TableType = dto.TableType.Trim();
            table.HourlyRate = dto.PricePerHour;
            table.Description = dto.Description?.Trim();

            if (Enum.TryParse<TableStatus>(dto.Status, true, out var newStatus))
            {
                table.Status = newStatus;
            }

            _unitOfWork.Repository<BilliardTable>().Update(table);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Table updated: tableId={TableId}, name={TableName}, updatedBy={UpdatedBy}",
                id, table.TableName, updatedBy);

            return _mapper.Map<TableDto>(table);
        }

        public async Task<bool> DeleteTableAsync(Guid id)
        {
            var table = await _unitOfWork.Repository<BilliardTable>().GetByIdAsync(id);
            if (table == null) return false;

            var activeSessions = await _unitOfWork.Repository<TableSession>().GetAllAsync(
                s => s.TableId == id && !s.IsFinished);
            if (activeSessions.Any())
                throw new CustomException("Không thể xóa bàn đang có phiên chơi hoạt động", 400);

            _unitOfWork.Repository<BilliardTable>().Remove(table);
            var result = await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Table deleted: tableId={TableId}", id);
            return result > 0;
        }

        public async Task<PagedResult<TableDto>> GetPagedTablesAsync(TableQueryParameters query)
        {
            var filters = new List<System.Linq.Expressions.Expression<System.Func<BilliardTable, bool>>>();

            if (query.Status.HasValue)
            {
                var statusEnum = (TableStatus)query.Status.Value;
                filters.Add(t => t.Status == statusEnum);
            }
            if (!string.IsNullOrEmpty(query.TableType))
            {
                filters.Add(t => t.TableType.ToLower().Contains(query.TableType.ToLower()));
            }
            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                filters.Add(t => t.TableName.ToLower().Contains(query.SearchTerm.ToLower()));
            }

            Func<IQueryable<BilliardTable>, IOrderedQueryable<BilliardTable>>? orderBy = null;
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                if (query.SortBy.Equals("PricePerHour", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = q => query.IsDescending ? q.OrderByDescending(t => t.HourlyRate) : q.OrderBy(t => t.HourlyRate);
                }
                else if (query.SortBy.Equals("TableName", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = q => query.IsDescending ? q.OrderByDescending(t => t.TableName) : q.OrderBy(t => t.TableName);
                }
            }
            else
            {
                orderBy = q => q.OrderBy(t => t.TableName);
            }

            var (items, totalCount) = await _unitOfWork.Repository<BilliardTable>().GetPagedAsync(
                filters: filters,
                orderBy: orderBy,
                includeProperties: null,
                page: query.PageNumber,
                pageSize: query.PageSize
            );

            var mappedItems = _mapper.Map<IEnumerable<TableDto>>(items);
            return new PagedResult<TableDto>(mappedItems, query.PageNumber, query.PageSize, totalCount);
        }
    }
}
