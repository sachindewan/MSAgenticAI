using AgentApi.Agent;
using AgentApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgentApi.Controllers;

public class HomeController(OllamaAgentService agentService) : Controller
{
    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] AgentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { error = "Message is required" });

        var response = await agentService.RunAsync(request, cancellationToken);
        return Json(new { response });
    }
}
