using AgentApi.Agent;
using Microsoft.AspNetCore.Mvc;

namespace AgentApi.Controllers;

public class RagController : Controller
{
    private readonly IngestDataIntoVectorStoreService _ingestService;

    public RagController(IngestDataIntoVectorStoreService ingestService)
    {
        _ingestService = ingestService;
    }

    public IActionResult Index()
    {
        ViewData["Message"] = TempData["Message"];
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Ingest(CancellationToken cancellationToken)
    {
        await _ingestService.RunSample();
        TempData["Message"] = "Ingest completed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Search(string query, int top = 3)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            TempData["Message"] = "Query is required.";
            return RedirectToAction(nameof(Index));
        }

        var results = await _ingestService.SearchAsync(query, top);
        ViewData["Query"] = query;
        return View("Index", results);
    }
}
