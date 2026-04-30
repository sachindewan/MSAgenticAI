using Microsoft.Extensions.AI;
using OllamaSharp;
using AgentApi.Agent;
using AgentApi.Agent.Tools;
using AgentApi.Data;
using AgentApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IChatClient>(_ =>
{
    IChatClient ollamaClient = new OllamaApiClient(new Uri("http://localhost:11434"), "qwen2.5:0.5b");
    return ollamaClient;
        //.AsBuilder()
        //.UseFunctionInvocation()
        //.Build();
});

builder.Services.AddSingleton<InMemoryStore>();
builder.Services.AddSingleton<UserTool>();
builder.Services.AddSingleton<OrderTool>();
builder.Services.AddSingleton<OllamaAgentService>();
builder.Services.AddSingleton<MedicalImageService>();
builder.Services.AddSingleton<ChatLoopService>();
builder.Services.AddSingleton<MCPServerToolService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add guard middleware before routing so it can intercept the Analyze POST
app.UseMiddleware<MedicalAgentGuardMiddleware>();

app.UseRouting();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
