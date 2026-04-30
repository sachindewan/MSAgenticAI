using AgentApi.Agent;
using AgentApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgentApi.Controllers
{
    public class ToolsController : Controller
    {
        private readonly MCPServerToolService _mcpService;

        public ToolsController(MCPServerToolService mcpService)
        {
            _mcpService = mcpService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Invoke([FromBody] ToolRequest req, CancellationToken cancellationToken)
        {
            if (req is null || string.IsNullOrWhiteSpace(req.ToolId) || string.IsNullOrWhiteSpace(req.Message))
                return BadRequest(new { error = "ToolId and Message are required" });

            // route to MCP service when selected
            if (req.ToolId == "mcp")
            {
                var agentReq = new AgentRequest { Message = req.Message };
                var res = await _mcpService.RunAsync(agentReq, cancellationToken);
                return Json(new { result = res });
            }

            // For other tools we don't have services implemented yet — return a placeholder response
            if (req.ToolId == "websearch")
            {
                // Simple simulated response for web search tool
                var simulated = $"[Simulated Web Search] Results for: {req.Message}";
                return Json(new { result = simulated });
            }

            if (req.ToolId == "code")
            {
                var simulated = $"[Simulated Code Interceptor] Analysis for: {req.Message}";
                return Json(new { result = simulated });
            }

            return BadRequest(new { error = "Unknown tool" });
        }

        public class ToolRequest
        {
            public string ToolId { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }
    }
}
