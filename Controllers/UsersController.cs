using AgentApi.Agent;
using AgentApi.Data;
using AgentApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgentApi.Controllers;

public class UsersController(OllamaAgentService agentService, InMemoryStore store) : Controller
{
    public IActionResult Index() => View(store.Users);

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(string name, string email, CancellationToken cancellationToken)
    {
        await agentService.RunAsync(new AgentRequest
        {
            Message = $"Create a new user with name \"{name}\" and email \"{email}\""
        }, cancellationToken);

        TempData["Message"] = $"User {name} created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Orders(string userId)
    {
        var orders = store.Orders.Where(o => o.UserId == userId).ToList();
        ViewData["UserId"] = userId;
        return View(orders);
    }

    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await agentService.RunAsync(new AgentRequest
        {
            Message = $"Delete user with id \"{id}\""
        }, cancellationToken);

        TempData["Message"] = $"User {id} deleted.";
        return RedirectToAction(nameof(Index));
    }
}
