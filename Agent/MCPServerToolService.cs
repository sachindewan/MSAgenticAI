using AgentApi.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
namespace AgentApi.Agent
{
    public class MCPServerToolService
    {
        private readonly ChatClientAgent _agent;
        public MCPServerToolService(IChatClient chatClient)
        {
            McpClient mcpClient = McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri("https://learn.microsoft.com/api/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            })).GetAwaiter().GetResult();
            IList<McpClientTool> mcpTools = mcpClient.ListToolsAsync().GetAwaiter().GetResult();

            _agent = chatClient.AsAIAgent(new ChatClientAgentOptions
            {
                ChatOptions = new ChatOptions
                {
                    ModelId = "qwen2.5:0.5b",
                    Instructions = "You are an Expert in the C# version of Microsoft Agent Framework " +
                              "(use tools to find your knowledge) " +
                              "and assume Azure OpenAI with API Key is used",
                    Tools = mcpTools.Cast<AITool>().ToList(),
                    ToolMode = ChatToolMode.Auto
                }
            });
        }
        public async Task<string> RunAsync(AgentRequest request, CancellationToken cancellationToken = default)
        {
            var session = await _agent.CreateSessionAsync(cancellationToken);

            var response = await _agent.RunAsync(request.Message, session, cancellationToken: cancellationToken);

            return response.Messages
                .Where(m => m.Role == ChatRole.Assistant)
                .Select(m => m.Text)
                .LastOrDefault(string.Empty) ?? string.Empty;
        }
    }
}
