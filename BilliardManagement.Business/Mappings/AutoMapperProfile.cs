using AutoMapper;
using BilliardManagement.Models.Models;
using BilliardManagement.Business.DTOs;

namespace BilliardManagement.Business.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();

            CreateMap<BilliardTable, TableDto>()
                .ForMember(dest => dest.PricePerHour, opt => opt.MapFrom(src => src.HourlyRate));
            CreateMap<TableDto, BilliardTable>()
                .ForMember(dest => dest.HourlyRate, opt => opt.MapFrom(src => src.PricePerHour));

            CreateMap<CreateTableDto, BilliardTable>()
                .ForMember(dest => dest.HourlyRate, opt => opt.MapFrom(src => src.PricePerHour))
                .ForMember(dest => dest.Status, opt => opt.Ignore());

            CreateMap<TableSession, SessionDto>().ReverseMap();

            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.StockQuantity))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src =>
                    src.Category != null ? src.Category.CategoryName : string.Empty));
            CreateMap<ProductDto, Product>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.Stock))
                .ForMember(dest => dest.Category, opt => opt.Ignore());
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.Stock))
                .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore());

            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.TableSessionId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.OrderedBy));
            CreateMap<OrderDto, Order>()
                .ForMember(dest => dest.TableSessionId, opt => opt.MapFrom(src => src.SessionId))
                .ForMember(dest => dest.OrderedBy, opt => opt.MapFrom(src => src.UserId));

            CreateMap<OrderItem, OrderItemDto>().ReverseMap();

            CreateMap<Invoice, BillDto>()
                .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.TableSessionId))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.TotalAmount));
            CreateMap<BillDto, Invoice>()
                .ForMember(dest => dest.TableSessionId, opt => opt.MapFrom(src => src.SessionId))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Total));

            CreateMap<Shift, ShiftDto>()
                .ForMember(dest => dest.CheckIn, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.CheckOut, opt => opt.MapFrom(src => src.EndTime))
                .ForMember(dest => dest.Revenue, opt => opt.MapFrom(src => src.TotalRevenue));
            CreateMap<ShiftDto, Shift>()
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.CheckIn))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.CheckOut))
                .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.Revenue));
        }
    }
}
