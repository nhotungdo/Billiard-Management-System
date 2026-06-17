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

            CreateMap<TableStatusHistory, TableStatusHistoryDto>()
                .ForMember(dest => dest.TableName, opt => opt.MapFrom(src => src.BilliardTable != null ? src.BilliardTable.TableName : string.Empty))
                .ForMember(dest => dest.OldStatusName, opt => opt.MapFrom(src => src.OldStatus.ToString()))
                .ForMember(dest => dest.NewStatusName, opt => opt.MapFrom(src => src.NewStatus.ToString()))
                .ForMember(dest => dest.ChangedByName, opt => opt.MapFrom(src => src.ChangedByUser != null ? src.ChangedByUser.FullName : string.Empty));

            CreateMap<TableSession, SessionDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : null))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.PhoneNumber : null))
                .ReverseMap();

            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<CreateCategoryDto, Category>();

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
                .ForMember(dest => dest.StaffId, opt => opt.MapFrom(src => src.TableSession != null ? (Guid?)src.TableSession.UserId : null))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.TableName, opt => opt.MapFrom(src => src.TableSession != null && src.TableSession.BilliardTable != null ? src.TableSession.BilliardTable.TableName : string.Empty))
                .ForMember(dest => dest.TableType, opt => opt.MapFrom(src => src.TableSession != null && src.TableSession.BilliardTable != null ? src.TableSession.BilliardTable.TableType : string.Empty))
                .ForMember(dest => dest.StaffName, opt => opt.MapFrom(src => src.TableSession != null && src.TableSession.User != null ? src.TableSession.User.FullName : string.Empty))
                .ForMember(dest => dest.PlayingFee, opt => opt.MapFrom(src => src.TableSession != null ? src.TableSession.TotalPrice : 0))
                .ForMember(dest => dest.ServiceFee, opt => opt.MapFrom(src => src.TableSession != null ? Math.Max(0, src.Subtotal - src.TableSession.TotalPrice) : 0));
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

            CreateMap<Customer, CustomerDto>().ReverseMap();
            CreateMap<CustomerCreateDto, Customer>();
            CreateMap<CustomerUpdateDto, Customer>();
        }
    }
}
