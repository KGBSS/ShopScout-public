using Microsoft.Extensions.Hosting;
using ShopScout.Models;
using System.Globalization;

namespace ShopScout.Services;

public class LogService
{
    private readonly IHostEnvironment _env;

    public LogService(IHostEnvironment env) => _env = env;

    public List<DateTime> GetAvailableDates(string prefix)
    {
        var path = Path.Combine(_env.ContentRootPath, "logs");
        if (!Directory.Exists(path)) return new();

        return Directory.GetFiles(path, $"{prefix}*.txt")
            .Select(Path.GetFileNameWithoutExtension)
            .Select(name => name!.Replace(prefix, ""))
            .Select(datePart => DateTime.TryParseExact(datePart, "yyyyMMdd", null, DateTimeStyles.None, out var d) ? d : (DateTime?)null)
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .OrderByDescending(d => d)
            .ToList();
    }

    public async Task<string> ReadRawLogAsync(string fileName)
    {
        var filePath = Path.Combine(_env.ContentRootPath, "logs", fileName);
        if (!File.Exists(filePath)) return "No log data found for this date.";

        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan);
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            return string.IsNullOrWhiteSpace(content) ? "Log file exists but is currently empty." : content;
        }
        catch (IOException)
        {
            return "The file is currently being locked by the system. Please try again in a moment.";
        }
    }

    public List<LogEntry> ParseLogs(string rawContent)
    {
        var entries = new List<LogEntry>();
        var lines = rawContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            try
            {
                var parts = line.Split(new[] { " [" }, StringSplitOptions.None);
                if (parts.Length < 3) continue;

                entries.Add(new LogEntry
                {
                    Timestamp = DateTime.Parse(parts[0].Substring(0, 19)),
                    Level = parts[1].Replace("]", "").Trim(),
                    SourceContext = parts[2].Split(']')[0].Trim(),
                    Message = line.Substring(line.IndexOf("] ]") + 3).Trim()
                });
            }
            catch { continue; }
        }
        return entries;
    }
}