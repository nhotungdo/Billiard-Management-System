using Microsoft.Extensions.DependencyInjection;
using BilliardManagement.Business.Interfaces;
using BilliardManagement.Business.Services;
using BilliardManagement.Data.Repositories.Interfaces;
using BilliardManagement.Data.Repositories;
using BilliardManagement.Business.Mappings;
using FluentValidation;

namespace BilliardManagement.Business
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITableService, TableService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IBillingService, BillingService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IShiftService, ShiftService>();
            services.AddScoped<IRevenueService, RevenueService>();

            services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            return services;
        }
    }
}
