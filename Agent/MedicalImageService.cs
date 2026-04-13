using System.Text;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace AgentApi.Agent;

public class MedicalImageService
{
    private readonly string _endpoint;
    private readonly string _model;

    public MedicalImageService(IConfiguration config)
    {
        _endpoint = config["Ollama:Endpoint"] ?? "http://localhost:11434";
        _model = config["Ollama:VisionModel"] ?? "gemma3:4b-cloud";
    }

    public async Task<string> AnalyzeAsync(byte[] imageBytes, string mediaType, string modality, string? clinicalContext, CancellationToken cancellationToken = default)
    {
        IChatClient chatClient = new OllamaApiClient(_endpoint, _model);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, BuildSystemInstructions(modality)),
            new(ChatRole.User, new AIContent[]
            {
                new TextContent(BuildUserPrompt(modality, clinicalContext)),
                new DataContent(imageBytes, mediaType)
            })
        };

        var response = await chatClient.GetResponseAsync(messages, new ChatOptions
        {
            Temperature = 0.1f,
            MaxOutputTokens = 2200
        }, cancellationToken);

        return response.Text ?? "No analysis returned.";
    }

    private static string BuildSystemInstructions(string modality) =>
        $"""
        You are a careful multimodal medical image triage assistant running locally.
        Analyze the supplied medical image and produce a structured report.

        Important safety rules:
        - Do not claim to be a licensed doctor.
        - State that the output is decision support and not a diagnosis.
        - If the modality or anatomy is uncertain, say so clearly.
        - Mention image quality limitations and missing clinical context.
        - Highlight urgent red-flag findings separately.
        - Never invent measurements that are not visible.

        Expected modality focus: {modality}.
        Expected output sections:
        1. Study summary
        2. Detected modality and anatomy
        3. Image quality observations
        4. Key visual findings
        5. Clinical impression
        6. Red flags / urgent escalation
        7. Recommended follow-up questions or tests
        8. Patient-friendly explanation
        """;

    private static string BuildUserPrompt(string modality, string? clinicalContext)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Analyze this {modality} medical image in detail.");
        if (!string.IsNullOrWhiteSpace(clinicalContext))
            sb.AppendLine($"Clinical context: {clinicalContext}");
        sb.AppendLine("Use concise but clinically useful language.");
        sb.AppendLine("If this is an ECG, comment on rate/rhythm, intervals if inferable, axis clues, waveform abnormalities, and obvious artifact.");
        sb.AppendLine("If this is an X-ray, comment on projection limitations, bones/soft tissue/lung fields/cardiomediastinal silhouette when applicable.");
        sb.AppendLine("If this is an MRI, comment on likely sequence or plane only if reasonably inferable, symmetry, mass effect, signal abnormalities, and obvious artifacts.");
        sb.AppendLine("End with a short disclaimer recommending confirmation by a radiologist or cardiologist.");
        return sb.ToString();
    }

    public static string GetMediaType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}
