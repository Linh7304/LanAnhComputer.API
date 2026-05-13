using AutoMapper;
using LanAnhComputer.API.Data.Entities;
using LanAnhComputer.API.Dtos;
using LanAnhComputer.Data.Entities;
using LanAnhComputer.Dtos;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LanAnhComputer.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryDto>(); // chuyển từ Category sang CategoryDto
        CreateMap<CategoryUpsertDto, Category>();

        CreateMap<Product, ProductDto>();
        CreateMap<ProductUpsertDto, Product>();

        CreateMap<User, UserDto>();
        CreateMap<UserUpsertDto, User>();

        CreateMap<OrderDetail, OrderDetailDto>();

        CreateMap<Order, OrderDto>();

        CreateMap<ChatbotHistory, ChatbotHistoryDto>();
        CreateMap<ChatbotHistoryCreateDto, ChatbotHistory>();
        CreateMap<CartDto, CartItem>();
    }
}
