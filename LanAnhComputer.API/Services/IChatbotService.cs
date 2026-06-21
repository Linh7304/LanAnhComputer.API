using LanAnhComputer.API.Dtos;
using System.Threading.Tasks;

namespace LanAnhComputer.API.Services
{
    public interface IChatbotService
    {
        Task<string> HandleChatAsync(ChatRequestDto request);
    }
}
