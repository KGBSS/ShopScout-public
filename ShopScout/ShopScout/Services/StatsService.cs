using HtmlAgilityPack;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;

namespace ShopScout.Services;

public class SiteStats
{
    public string LastUpdate { get; set; } = "-";
    public string ReportedPeriod { get; set; } = "-";

    // Summary (Human Traffic)
    public string UniqueVisitors { get; set; } = "0";
    public string TotalVisits { get; set; } = "0";
    public string Pages { get; set; } = "0";
    public string Hits { get; set; } = "0";
    public string Bandwidth { get; set; } = "0";

    public string BotHits { get; set; } = "0";
    public string BotBandwidth { get; set; } = "0";

    // Charts
    public string[] DailyLabels { get; set; } = Array.Empty<string>();
    public int[] DailyVisits { get; set; } = Array.Empty<int>();

    public string[] HourlyLabels { get; set; } = Array.Empty<string>();
    public int[] HourlyHits { get; set; } = Array.Empty<int>();

    public string[] MonthlyLabels { get; set; } = Array.Empty<string>();
    public int[] MonthlyVisits { get; set; } = Array.Empty<int>();

    // Lists
    public List<GenericStat> OperatingSystems { get; set; } = new();
    public List<GenericStat> TopPages { get; set; } = new();
    public List<GenericStat> Referers { get; set; } = new();
}

public class GenericStat
{
    public string Name { get; set; } = "";
    public string Count { get; set; } = "";
    public string Percent { get; set; } = "";
    public string ExtraInfo { get; set; } = "";
}

public class StatsService
{
    private readonly HttpClient _http;
    public StatsService(HttpClient http) => _http = http;

    public async Task<SiteStats> GetAwStatsAsync(string baseUrl, int month, int year)
    {
        var stats = new SiteStats();
        try
        {
            var uri = new Uri(baseUrl);
            var query = HttpUtility.ParseQueryString(uri.Query);
            var targetUrl = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}?config={query["config"]}&framename=mainright" +
                            $"&databasebreak=month&month={month}&year={year}&output=main";
            if (!string.IsNullOrEmpty(query["h"])) targetUrl += $"&h={query["h"]}";

            var html = await _http.GetStringAsync(targetUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // 1. METADATA
            stats.LastUpdate = Clean(doc.DocumentNode.SelectSingleNode("//td[b[contains(., 'Last Update')]]/following-sibling::td")?.InnerText ?? "-");
            stats.ReportedPeriod = Clean(doc.DocumentNode.SelectSingleNode("//tr[td[b[contains(., 'Reported period')]]]/td[2]")?.InnerText ?? "-");

            // 2. HUMAN TRAFFIC (Exact XPaths - DO NOT CHANGE)
            stats.UniqueVisitors = GetValue(doc, "/html/body/table[1]//tr[2]/td/table//tr[5]/td[2]/b");
            stats.TotalVisits = GetValue(doc, "/html/body/table[1]//tr[2]/td/table//tr[5]/td[3]/b");
            stats.Pages = GetValue(doc, "/html/body/table[1]//tr[2]/td/table//tr[5]/td[4]/b");
            stats.Hits = GetValue(doc, "/html/body/table[1]//tr[2]/td/table//tr[5]/td[5]/b");
            stats.Bandwidth = GetValue(doc, "/html/body/table[1]//tr[2]/td/table//tr[5]/td[6]/b");

            // 3. BOT TRAFFIC (Exact XPath)
            stats.BotHits = GetValue(doc, "/html/body/table[1]//tr[2]/td/table//tr[6]/td[4]/b");
            stats.BotBandwidth = GetValue(doc, "/html/body/table[1]//tr[2]/td/table//tr[6]/td[5]/b");

            // 4. MONTHLY TRENDS (Full Year Line Chart)
            // Finds the table containing 'Month' and 'Unique visitors' in headers
            var monthlyTable = doc.DocumentNode.SelectSingleNode("/html/body/table[2]/tr[2]/td/table/tr/td/center/table[2]");
            if (monthlyTable != null)
            {
                var mLabels = new List<string>();
                var mVisits = new List<int>();

                // Select all rows inside this table
                var rows = monthlyTable.SelectNodes(".//tr");
                if (rows != null)
                {
                    foreach (var row in rows.Skip(1))
                    {
                        var cols = row.SelectNodes("td");
                        // We need rows with data (6 cols)
                        if (cols != null && cols.Count >= 6)
                        {
                            var rawLabel = Clean(cols[0].InnerText);

                            // Filter out Header row and Total row
                            if (!rawLabel.Contains("Month") && !rawLabel.Contains("Total"))
                            {
                                // "Jan 2026" -> "Jan"
                                mLabels.Add(rawLabel.Split(' ')[0]);
                                mVisits.Add(ParseInt(cols[2].InnerText)); // Col 2 is Number of Visits
                            }
                        }
                    }
                }
                stats.MonthlyLabels = mLabels.ToArray();
                stats.MonthlyVisits = mVisits.ToArray();
            }

            // 5. DAILY HISTORY
            var dailyTable = doc.DocumentNode.SelectSingleNode("//a[@name='daysofmonth']/following::table[1]//table");
            if (dailyTable != null)
            {
                var dLabels = new List<string>();
                var dVisits = new List<int>();
                foreach (var row in dailyTable.SelectNodes(".//tr") ?? new HtmlNodeCollection(null))
                {
                    var cols = row.SelectNodes("td");
                    if (cols != null && cols.Count == 5 && char.IsDigit(Clean(cols[0].InnerText).FirstOrDefault()))
                    {
                        dLabels.Add(Clean(cols[0].InnerText).Split(' ')[0]);
                        dVisits.Add(ParseInt(cols[1].InnerText));
                    }
                }
                stats.DailyLabels = dLabels.ToArray();
                stats.DailyVisits = dVisits.ToArray();
            }

            // 6. HOURLY HISTORY
            var hourlyTable = doc.DocumentNode.SelectSingleNode("//a[@name='hours']/following::table[1]//table");
            if (hourlyTable != null)
            {
                var hLabels = new List<string>();
                var hHits = new List<int>();
                foreach (var row in hourlyTable.SelectNodes(".//tr") ?? new HtmlNodeCollection(null))
                {
                    var cols = row.SelectNodes("td");
                    if (cols != null && cols.Count == 4 && cols[0].InnerText != "Hours")
                    {
                        hLabels.Add(Clean(cols[0].InnerText));
                        hHits.Add(ParseInt(cols[2].InnerText));
                    }
                }
                stats.HourlyLabels = hLabels.ToArray();
                stats.HourlyHits = hHits.ToArray();
            }

            // 7. LISTS (OS, URLs, Referers)
            ScrapeTable(doc, "urls", 5, (cols) => {
                var name = cols[0].InnerText.Trim();
                if (name.StartsWith("/"))
                    stats.TopPages.Add(new GenericStat { Name = Clean(cols[0].InnerText), Count = Clean(cols[1].InnerText), ExtraInfo = Clean(cols[2].InnerText) });
            });

            ScrapeTable(doc, "os", 5, (cols) => {
                stats.OperatingSystems.Add(new GenericStat { Name = Clean(cols[1].InnerText), Count = Clean(cols[4].InnerText), Percent = Clean(cols[5].InnerText) });
            });

            ScrapeReferers(doc, stats);
        }
        catch { /* Log error */ }
        return stats;
    }

    // --- Helpers ---
    private string GetValue(HtmlDocument doc, string xpath) { var n = doc.DocumentNode.SelectSingleNode(xpath); return n != null ? Clean(n.InnerText) : "0"; }
    private void ScrapeTable(HtmlDocument doc, string anchor, int minCols, Action<HtmlNodeCollection> action) { var t = doc.DocumentNode.SelectSingleNode($"//a[@name='{anchor}']/following::table[1]//table"); if (t == null) return; foreach (var r in t.SelectNodes(".//tr") ?? new HtmlNodeCollection(null)) { var c = r.SelectNodes("td"); if (c != null && c.Count >= minCols) try { action(c); } catch { } } }
    private void ScrapeReferers(HtmlDocument doc, SiteStats s) { var t = doc.DocumentNode.SelectSingleNode("//table[contains(., 'Links from an external page')]//table"); if (t == null) return; foreach (var r in t.SelectNodes(".//tr") ?? new HtmlNodeCollection(null)) { var c = r.SelectNodes("td"); if (c?.Count >= 2) { var n = Clean(c[0].InnerText).Replace("- ", ""); if (!string.IsNullOrEmpty(n)) s.Referers.Add(new GenericStat { Name = n, Count = Clean(c[1].InnerText) }); } } }
    private string Clean(string input) { if (string.IsNullOrWhiteSpace(input)) return ""; var d = HttpUtility.HtmlDecode(input); var t = Regex.Replace(d, "<.*?>", " "); var p = Regex.Replace(t, @"\(.*?\)", ""); var r = p.Replace(",", "").Trim(); var m = Regex.Match(r, @"^[\d\.]+\s*([KMG]B)?", RegexOptions.IgnoreCase); return m.Success ? m.Value.Trim() : r; }
    private int ParseInt(string input) { int.TryParse(Clean(input), out int r); return r; }
}