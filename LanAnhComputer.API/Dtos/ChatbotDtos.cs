namespace LanAnhComputer.Dtos;

public class ChatbotHistoryDto
{
    public long ChatHistoryId { get; set; }
    public long? UserId { get; set; }
    public string SessionId { get; set; } = null!;
    public string UserMessage { get; set; } = null!;
    public string BotResponse { get; set; } = null!;
    public string? Intent { get; set; }
    public long? ProductIdSuggested { get; set; }
    public int? ResponseTimeMs { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ChatbotHistoryCreateDto
{
    public long? UserId { get; set; }
    public string SessionId { get; set; } = null!;
    public string UserMessage { get; set; } = null!;
    public string BotResponse { get; set; } = null!;
    public string? Intent { get; set; }
    public long? ProductIdSuggested { get; set; }
    public int? ResponseTimeMs { get; set; }
}
