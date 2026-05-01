using AgentApi.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentApi.Agent
{
    public class WebsearchToolService
    {
        private readonly ChatClientAgent _agent;
        public WebsearchToolService(IChatClient chatClient)
        {
            _agent = chatClient.AsAIAgent(new ChatClientAgentOptions
            {
                ChatOptions = new ChatOptions
                {
                    ModelId = "gemma3:4b-cloud",
                    Instructions = "You are a web search assistant that use the web_search tool to browse upto date news.",
                    Tools = [new HostedWebSearchTool()],
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
