using AgentApi.Agent;
using AgentApi.Data;
using AgentApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgentApi.Controllers;

public class OrdersController(OllamaAgentService agentService, InMemoryStore store) : Controller
{
    public IActionResult Index() => View(store.Orders);

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(string userId, string items, decimal total, CancellationToken cancellationToken)
    {
        var itemList = items.Split(',').Select(i => i.Trim()).ToList();
        await agentService.RunAsync(new AgentRequest
        {
            Message = $"Create an order for user \"{userId}\" with items {string.Join(", ", itemList)} and total {total}"
        }, cancellationToken);

        TempData["Message"] = "Order created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult UpdateStatus(string id)
    {
        var order = store.Orders.FirstOrDefault(o => o.Id == id);
        if (order is null) return NotFound();
        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(string id, string status, CancellationToken cancellationToken)
    {
        await agentService.RunAsync(new AgentRequest
        {
            Message = $"Update the status of order \"{id}\" to \"{status}\""
        }, cancellationToken);

        TempData["Message"] = $"Order {id} updated to {status}.";
        return RedirectToAction(nameof(Index));
    }
}
