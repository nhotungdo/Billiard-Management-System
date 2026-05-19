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
            CreateMap<BilliardTable, TableDto>().ReverseMap();
            CreateMap<CreateTableDto, BilliardTable>();
            CreateMap<TableSession, SessionDto>().ReverseMap();
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<CreateProductDto, Product>();
            CreateMap<Order, OrderDto>().ReverseMap();
            CreateMap<OrderItem, OrderItemDto>().ReverseMap();
            CreateMap<Invoice, BillDto>().ReverseMap();
            CreateMap<Shift, ShiftDto>().ReverseMap();
        }
    }
}
