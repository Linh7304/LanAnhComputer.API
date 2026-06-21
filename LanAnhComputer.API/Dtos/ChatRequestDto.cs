namespace LanAnhComputer.API.Dtos
{
    public class ChatRequestDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public List<ChatMessageDto> History { get; set; } = new List<ChatMessageDto>();
    }

    public class ChatMessageDto
    {
        public string Role { get; set; } = string.Empty; // "user" or "model"
        public string Text { get; set; } = string.Empty;
    }
}
