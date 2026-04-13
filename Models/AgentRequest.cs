namespace AgentApi.Models;

public class AgentRequest
{
    public string Message { get; set; } = string.Empty;
    public List<ConversationMessage> History { get; set; } = [];
}

public class ConversationMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
