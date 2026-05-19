using Microsoft.AspNetCore.Http;
namespace LanAnhComputer.API.Dtos
{
    public class ProductCreateDto
    {
        public string ProductName { get; set; }

        public decimal Price { get; set; }

        public IFormFile? Image { get; set; }
    }
}
