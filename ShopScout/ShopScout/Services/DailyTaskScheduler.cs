using ShopScout.Data;

namespace ShopScout.Services;

public class DailyTaskScheduler : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailyTaskScheduler> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _folderPath;
    private readonly string _filePath;

    private readonly TimeSpan _runTime = new TimeSpan(5, 0, 0);

    public DailyTaskScheduler(
        IBackgroundTaskQueue taskQueue,
        ILogger<DailyTaskScheduler> logger,
        IServiceScopeFactory scopeFactory,
        IHostEnvironment env)
    {
        _taskQueue = taskQueue;
        _logger = logger;
        _scopeFactory = scopeFactory;
        // Path to the file that tracks the last execution date
        _folderPath = Path.Combine(env.ContentRootPath, "App_Data");
        _filePath = Path.Combine(_folderPath, "last_run.txt");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily Scheduler starting. Target time: {time}", _runTime);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var targetTimeToday = now.Date.Add(_runTime);
            DateTime lastRunDate = GetLastRunDateFromFile();

            // Check if we need to run NOW (Time passed AND not run today yet)
            if (now >= targetTimeToday && lastRunDate.Date != now.Date)
            {
                _logger.LogInformation("Triggering daily task at {now}", now);

                lastRunDate = now.Date;

                await _taskQueue.QueueBackgroundWorkItemAsync(async token =>
                {
                    _logger.LogInformation("Daily product price fetch starting...");

                    try
                    {
                        using IServiceScope scope = _scopeFactory.CreateScope();
                        var arfigyeloFetchService = scope.ServiceProvider.GetRequiredService<ArfigyeloFetchService>();

                        await arfigyeloFetchService.FetchAsync();

                        SaveLastRunDateToFile(now.Date);
                        _logger.LogInformation("Daily product price fetch successful!");
                    }
                    catch (Exception ex)
                    {
                        lastRunDate = now.Date.Subtract(TimeSpan.FromDays(2));
                        _logger.LogError(ex, "Daily product price fetch failed!");
                    }
                });
            }

            // Calculate the delay until the next occurrence
            var nextRun = now.TimeOfDay < _runTime
                ? now.Date.Add(_runTime)
                : now.Date.AddDays(1).Add(_runTime);

            var timeUntilNextRun = nextRun - DateTime.Now;

            var actualDelay = timeUntilNextRun < TimeSpan.FromMinutes(30)
                ? timeUntilNextRun
                : TimeSpan.FromMinutes(30);

            if (actualDelay.TotalMilliseconds > 0)
            {
                _logger.LogDebug("Scheduler sleeping for {delay}", actualDelay);
                await Task.Delay(actualDelay, stoppingToken);
            }
        }
    }

    private DateTime GetLastRunDateFromFile()
    {
        EnsureSaveFolderExists();
        if (!File.Exists(_filePath)) return DateTime.MinValue;
        try
        {
            string content = File.ReadAllText(_filePath);
            return DateTime.TryParse(content, out var date) ? date.Date : DateTime.MinValue;
        }
        catch { return DateTime.MinValue; }
    }

    private void SaveLastRunDateToFile(DateTime date)
    {
        EnsureSaveFolderExists();
        try { File.WriteAllText(_filePath, date.ToString("yyyy-MM-dd")); }
        catch (Exception ex) { _logger.LogError(ex, "Failed to save last run date."); }
    }

    private void EnsureSaveFolderExists()
    {
        Directory.CreateDirectory(_folderPath);
    }
}