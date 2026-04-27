using AgentApi.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Threading;

namespace AgentApi.Agent
{
    public class ChatLoopService
    {
        private readonly ChatClientAgent _agent;
        private readonly AgentSession _session;
        public ChatLoopService(IChatClient chatClient)
        {
            _agent = new ChatClientAgent(
                chatClient,
                instructions: "You are a helpful assistant. Interact with user for their queries.",
                name: "ChatAssistant"
            );
            _session = _agent.CreateSessionAsync().GetAwaiter().GetResult();
        }

        public async Task<string> RunAsync(string message, CancellationToken cancellationToken = default)
        {

            var response = await _agent.RunAsync(message, cancellationToken: cancellationToken);


            return response.Messages
                .Where(m => m.Role == ChatRole.Assistant)
                .Select(m => m.Text)
                .LastOrDefault(string.Empty) ?? string.Empty;
        }

        // New: run message using the long-lived session created at construction
        public async Task<string> RunWithSessionAsync(string message, CancellationToken cancellationToken = default)
        {
            var response = await _agent.RunAsync(message, _session, cancellationToken: cancellationToken);

            return response.Messages
                .Where(m => m.Role == ChatRole.Assistant)
                .Select(m => m.Text)
                .LastOrDefault(string.Empty) ?? string.Empty;
        }
    }
}
