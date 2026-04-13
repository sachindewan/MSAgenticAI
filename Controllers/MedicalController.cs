using AgentApi.Agent;
using Microsoft.AspNetCore.Mvc;

namespace AgentApi.Controllers;

public class MedicalController(MedicalImageService medicalImageService) : Controller
{
    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> Analyze(IFormFile image, string modality, string? clinicalContext, CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
        {
            ModelState.AddModelError("", "Please upload an image.");
            return View("Index");
        }

        using var ms = new MemoryStream();
        await image.CopyToAsync(ms, cancellationToken);
        var imageBytes = ms.ToArray();
        var mediaType = MedicalImageService.GetMediaType(image.FileName);

        var result = await medicalImageService.AnalyzeAsync(imageBytes, mediaType, modality, clinicalContext, cancellationToken);

        ViewData["Result"] = result;
        ViewData["Modality"] = modality;
        ViewData["FileName"] = image.FileName;
        return View("Index");
    }
}
