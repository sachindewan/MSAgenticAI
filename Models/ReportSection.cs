namespace AgentApi.Models;

public class ReportSection
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsRedFlag { get; set; }

    public static List<ReportSection> Parse(string text)
    {
        var lines = text.Split('\n');
        var currentTitle = "Analysis";
        var currentContent = new System.Text.StringBuilder();
        var sections = new List<ReportSection>();

        foreach (var line in lines)
        {
            var cleaned = line.Trim().TrimStart('*').TrimEnd('*').Trim();
            if (cleaned.Length > 2 && char.IsDigit(cleaned[0]) && cleaned[1] == '.')
            {
                if (currentContent.Length > 0)
                    sections.Add(Build(currentTitle, currentContent.ToString().Trim()));
                currentTitle = cleaned[2..].Trim();
                currentContent.Clear();
            }
            else
            {
                currentContent.AppendLine(line.Trim());
            }
        }

        if (currentContent.Length > 0)
            sections.Add(Build(currentTitle, currentContent.ToString().Trim()));

        return sections.Where(s => !string.IsNullOrWhiteSpace(s.Content)).ToList();
    }

    private static ReportSection Build(string title, string content) => new()
    {
        Title = title,
        Content = content,
        IsRedFlag = title.Contains("red flag", StringComparison.OrdinalIgnoreCase) ||
                    title.Contains("urgent", StringComparison.OrdinalIgnoreCase)
    };
}
