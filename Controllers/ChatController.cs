using AgentApi.Agent;
using AgentApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgentApi.Controllers;

public class ChatController : Controller
{
    private readonly ChatLoopService _chatLoop;
    public ChatController(ChatLoopService chatLoop) => _chatLoop = chatLoop;

    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] AgentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Message))
            return BadRequest(new { error = "Message is required" });

        var response = await _chatLoop.RunAsync(request.Message, cancellationToken);
        return Json(new { response });
    }

    [HttpPost]
    public async Task<IActionResult> ChatWithSession([FromBody] AgentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Message))
            return BadRequest(new { error = "Message is required" });

        var response = await _chatLoop.RunWithSessionAsync(request.Message, cancellationToken);
        return Json(new { response });
    }
}
