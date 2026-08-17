using AutoMapper;
using Rivo.Application.Brands.Dtos;
using Rivo.Application.Categories.Dtos;
using Rivo.Application.Customers.Dtos;
using Rivo.Application.Loyalty.Dtos;
using Rivo.Application.Orders.Dtos;
using Rivo.Application.Payments.Dtos;
using Rivo.Application.Permissions.Dtos;
using Rivo.Application.Products.Dtos;
using Rivo.Application.Returns.Dtos;
using Rivo.Application.Stores.Dtos;
using Rivo.Application.Users.Dtos;
using Rivo.Domain.Entities.Brands;
using Rivo.Domain.Entities.Categories;
using Rivo.Domain.Entities.Customers;
using Rivo.Domain.Entities.Loyalty;
using Rivo.Domain.Entities.Orders;
using Rivo.Domain.Entities.Payments;
using Rivo.Domain.Entities.Permissions;
using Rivo.Domain.Entities.Products;
using Rivo.Domain.Entities.Returns;
using Rivo.Domain.Entities.Stores;
using Rivo.Domain.Entities.Users;

namespace Rivo.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.RoleName, o => o.MapFrom(s => s.Role != null ? s.Role.Name : string.Empty));

        CreateMap<Permission, PermissionDto>();

        CreateMap<Store, StoreDto>();
        CreateMap<Branch, BranchDto>();

        CreateMap<Category, CategoryDto>();
        CreateMap<Brand, BrandDto>();

        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.BrandName, o => o.MapFrom(s => s.Brand != null ? s.Brand.Name : null));
        CreateMap<ProductVariation, ProductVariationDto>();

        CreateMap<Customer, CustomerDto>();

        CreateMap<LoyaltyLevel, LoyaltyLevelDto>();
        CreateMap<LoyaltyCard, LoyaltyCardDto>()
            .ForMember(d => d.LoyaltyLevelName, o => o.Ignore())
            .ForMember(d => d.LoyaltyLevelDiscountPercentage, o => o.Ignore());

        CreateMap<Order, OrderDto>();
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty));

        CreateMap<Payment, PaymentDto>();

        CreateMap<Return, ReturnDto>();
        CreateMap<ReturnItem, ReturnItemDto>();
    }
}
