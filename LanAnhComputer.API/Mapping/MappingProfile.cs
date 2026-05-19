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

        // QUAN TRỌNG
        CreateMap<ProductUpsertDto, Product>()
            .ForMember(dest => dest.ImageUrl,
                opt => opt.Ignore());

        CreateMap<User, UserDto>();
        CreateMap<UserUpsertDto, User>();

        CreateMap<OrderDetail, OrderDetailDto>();

        CreateMap<Order, OrderDto>();

        CreateMap<ChatbotHistory, ChatbotHistoryDto>();
        CreateMap<ChatbotHistoryCreateDto, ChatbotHistory>();

        CreateMap<CartDto, CartItem>();
    }
}