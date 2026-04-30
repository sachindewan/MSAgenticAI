using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using AgentApi.Agent.Tools;
using AgentApi.Models;
using System.Runtime.CompilerServices;

namespace AgentApi.Agent;

public class OllamaAgentService
{
    private readonly ChatClientAgent _agent;

    public OllamaAgentService(IChatClient chatClient, UserTool userTool, OrderTool orderTool)
    {
        var tools = new List<AITool>
        {
            AIFunctionFactory.Create(userTool.GetUser, "get_user", "Get a user by their ID"),
            AIFunctionFactory.Create(userTool.ListUsers, "list_users", "List all users"),
            AIFunctionFactory.Create(userTool.CreateUser, "create_user", "Create a new user"),
            AIFunctionFactory.Create(userTool.DeleteUser, "delete_user", "Delete a user by ID"),
            AIFunctionFactory.Create(orderTool.GetOrder, "get_order", "Get an order by its ID"),
            AIFunctionFactory.Create(orderTool.GetOrdersByUser, "get_orders_by_user", "Get all orders for a specific user"),
            AIFunctionFactory.Create(orderTool.CreateOrder, "create_order", "Create a new order"),
            AIFunctionFactory.Create(orderTool.UpdateOrderStatus, "update_order_status", "Update the status of an order"),
            AIFunctionFactory.Create(orderTool.ListOrders, "list_orders", "List all orders"),
        };

        //_agent = chatClient.AsAIAgent(new ChatClientAgentOptions
        // {
        //     ChatOptions = new ChatOptions
        //     {
        //         Instructions = "You are a helpful assistant for managing users and orders. Use the available tools to fulfill requests. Always confirm actions taken.",
        //         ModelId = "1",
        //         ToolMode = ChatToolMode.Auto,
        //         Tools = tools,
        //     }
        // });

        _agent = new ChatClientAgent(
            chatClient,
            instructions: "You are a helpful assistant for managing users and orders. Use the available tools to fulfill requests. Always confirm actions taken.",
            name: "OrdersAgent",
            tools: tools
        );
    }

    public async Task<string> RunAsync(AgentRequest request, CancellationToken cancellationToken = default)
    {
        var session = await _agent.CreateSessionAsync(cancellationToken);

        // Replay history into session
        foreach (var msg in request.History)
        {
            var role = msg.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase)
                ? ChatRole.Assistant
                : ChatRole.User;
            await _agent.RunAsync(new ChatMessage(role, msg.Content), session, cancellationToken: cancellationToken);
        }

        var response = await _agent.RunAsync(request.Message, session, cancellationToken: cancellationToken);

        return response.Messages
            .Where(m => m.Role == ChatRole.Assistant)
            .Select(m => m.Text)
            .LastOrDefault(string.Empty) ?? string.Empty;
    }

}
