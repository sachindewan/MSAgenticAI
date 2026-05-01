using AgentApi.Agent;
using AgentApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgentApi.Controllers
{
    public class ToolsController : Controller
    {
        private readonly MCPServerToolService _mcpService;
        private readonly WebsearchToolService _websearchTool;

        public ToolsController(MCPServerToolService mcpService, WebsearchToolService websearchTool)
        {
            _mcpService = mcpService;
            _websearchTool = websearchTool;
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

            var agentReq = new AgentRequest { Message = req.Message };

            // route to MCP service when selected
            if (req.ToolId == "mcp")
            {
                var res = await _mcpService.RunAsync(agentReq, cancellationToken);
                return Json(new { result = res });
            }

            // For other tools we don't have services implemented yet — return a placeholder response
            if (req.ToolId == "websearch")
            {
                // Simple simulated response for web search tool
                var simulated = await _websearchTool.RunAsync(agentReq);
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
