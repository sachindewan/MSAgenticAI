using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace AgentApi.Middleware;

public class MedicalAgentGuardMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly string[] ForbiddenPatterns = new[]
    {
        "create user",
        "delete user",
        "drop table",
        "update order",
        "shutdown",
        "reboot",
        "rm -rf",
        "sudo",
        "exec ",
        "execute",
        "run command",
        "system:",
        "assistant:",
        "ignore previous",
        "openai",
        "api_key",
        "token",
        "password",
        "curl",
        "http",
        "https",
        "<script",
        "get_user",
        "create_order",
        "delete_user",
        "list_users",
        "delete",
        "drop",
        "insert",
        "select",
        "grant",
        "revoke"
    };

    public MedicalAgentGuardMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only inspect the Medical Analyze endpoint POST requests
        if (context.Request.Path.Equals("/Medical/Analyze", StringComparison.OrdinalIgnoreCase)
            && string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
        {
            // Enable buffering so we can read the request body and let the downstream handlers read it again
            context.Request.EnableBuffering();

            try
            {
                var form = await context.Request.ReadFormAsync();

                var clinicalContext = form["clinicalContext"].ToString() ?? string.Empty;
                var modality = form["modality"].ToString() ?? string.Empty;

                // also check filename and other form fields
                var fileName = form.Files.FirstOrDefault()?.FileName ?? string.Empty;

                var combined = (clinicalContext + " " + modality + " " + fileName).ToLowerInvariant();

                if (IsManipulation(combined))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = "application/json";
                    var payload = JsonSerializer.Serialize(new { error = "Prompt rejected: contains instructions or content outside of medical image analysis." });
                    await context.Response.WriteAsync(payload);
                    return;
                }
            }
            catch
            {
                // If we can't parse the form for some reason, reject to be safe
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                var payload = JsonSerializer.Serialize(new { error = "Invalid request format." });
                await context.Response.WriteAsync(payload);
                return;
            }
            finally
            {
                // Rewind the request body so the MVC pipeline can read the form again
                try
                {
                    context.Request.Body.Position = 0;
                }
                catch
                {
                    // ignore if not seekable
                }
            }
        }

        await _next(context);
    }

    private static bool IsManipulation(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        foreach (var pattern in ForbiddenPatterns)
        {
            if (text.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        // Reject if the text contains suspicious control sequences or markup
        if (text.Contains("{{") || text.Contains("}}") || text.Contains("<%") || text.Contains("<script"))
            return true;

        // Heuristic: reject if text contains excessive punctuation that looks like an instruction sequence
        var punctuationCount = text.Count(ch => char.IsPunctuation(ch));
        if (punctuationCount > 50)
            return true;

        return false;
    }
}
