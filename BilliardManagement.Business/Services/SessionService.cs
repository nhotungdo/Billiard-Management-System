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
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SessionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        // Khi một người chơi bắt đầu chơi, tạo một phiên mới với thời gian bắt đầu và lưu vào cơ sở dữ liệu
        public async Task<SessionDto> StartSessionAsync(Guid tableId, Guid userId)
        {
            var table = await _unitOfWork.Repository<BilliardTable>().GetByIdAsync(tableId);
            if (table == null) throw new CustomException("Table not found", 404);
            if (table.Status != TableStatus.Empty) throw new CustomException("Table is not empty", 400);

            var session = new TableSession
            {
                TableId = tableId,
                UserId = userId,
                StartTime = DateTime.UtcNow,
                Status = SessionStatus.Active
            };

            table.Status = TableStatus.Playing;
            
            await _unitOfWork.Repository<TableSession>().AddAsync(session);
            _unitOfWork.Repository<BilliardTable>().Update(table);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SessionDto>(session);
        }
        // Khi người chơi kết thúc phiên, cập nhật thời gian kết thúc, tính toán tổng tiền dựa trên thời gian chơi và giá giờ của bàn, sau đó lưu vào cơ sở dữ liệu
        public async Task<SessionDto> EndSessionAsync(Guid sessionId)
        {
            var session = await _unitOfWork.Repository<TableSession>().GetByIdAsync(sessionId);
            if (session == null) throw new CustomException("Session not found", 404);
            if (session.Status != SessionStatus.Active) throw new CustomException("Session is not active", 400);

            var table = await _unitOfWork.Repository<BilliardTable>().GetByIdAsync(session.TableId);
            if (table == null) throw new CustomException("Table not found", 404);

            session.EndTime = DateTime.UtcNow;
            session.Status = SessionStatus.Finished;
            
            // Calculate duration and price
            var duration = session.EndTime.Value - session.StartTime;
            session.DurationMinutes = (int)Math.Ceiling(duration.TotalMinutes / 5.0) * 5; // Round to 5 mins
            session.TotalPrice = (session.DurationMinutes.Value / 60m) * table.HourlyRate;

            table.Status = TableStatus.Empty;

            _unitOfWork.Repository<TableSession>().Update(session);
            _unitOfWork.Repository<BilliardTable>().Update(table);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SessionDto>(session);
        }
        // Cung cấp API để lấy danh sách các phiên đang hoạt động, bao gồm thông tin về bàn, người chơi và thời gian bắt đầu
        public async Task<IEnumerable<SessionDto>> GetActiveSessionsAsync()
        {
            var sessions = await _unitOfWork.Repository<TableSession>().GetAllAsync(s => s.Status == SessionStatus.Active);
            return _mapper.Map<IEnumerable<SessionDto>>(sessions);
        }
    }
}
