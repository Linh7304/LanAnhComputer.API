using AutoMapper;
using LanAnhComputer.API.Data.Entities;
using LanAnhComputer.API.Dtos;
using LanAnhComputer.Data.Entities;
using LanAnhComputer.Dtos;

namespace LanAnhComputer.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryDto>();
        CreateMap<CategoryUpsertDto, Category>();

        CreateMap<Product, ProductDto>();
        CreateMap<ProductImage, ProductImageDto>();
        CreateMap<ProductSpecification, ProductSpecificationDto>();
        CreateMap<ProductReview, ProductReviewDto>()
            .ForMember(dest => dest.UserFullName,
                opt => opt.MapFrom(src => src.User.FullName));

        // QUAN TRỌNG
        CreateMap<ProductUpsertDto, Product>()
            .ForMember(dest => dest.ImageUrl,
                opt => opt.Ignore());

        CreateMap<User, UserDto>();
        CreateMap<UserUpsertDto, User>();

        CreateMap<OrderDetail, OrderDetailDto>()
    .ForMember(dest => dest.ProductName,opt => opt.MapFrom(src => src.Product.ProductName))
    .ForMember(dest => dest.ImageUrl,opt => opt.MapFrom(src => src.Product.ImageUrl));

        CreateMap<Order, OrderDto>();

        CreateMap<ChatbotHistory, ChatbotHistoryDto>();
        CreateMap<ChatbotHistoryCreateDto, ChatbotHistory>();

        CreateMap<CartDto, CartItem>();
    }
}
