using Hangfire;
using TuxfamilyScraper.Library.Models;

namespace TuxfamilyScraper.Library.Data;

public class FileSizeScraperService : IFileSizeScraperService
{
    private readonly ITuxfamilyVersionService _versionService;
    private readonly IFilesizeQueueService _filesizeQueue;
    private readonly ILogger _logger;
    private readonly HttpClient _client;
    private readonly SemaphoreSlim _semaphore;

    public FileSizeScraperService(ITuxfamilyVersionService versionService, IFilesizeQueueService filesizeQueue,
        ILogger<FileSizeScraperService> logger)
    {
        _versionService = versionService;
        _filesizeQueue = filesizeQueue;
        _logger = logger;
        _client = new HttpClient(new SocketsHttpHandler
        {
            MaxConnectionsPerServer = 10
        });
        _semaphore = new SemaphoreSlim(10);
    }

    private async Task<int> GetFileSize(string url)
    {
        try
        {
            await _semaphore.WaitAsync();
            var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Failed to get size for {url}, Code: {response.StatusCode}");
                return 0;
            }

            if (response.Content.Headers.ContentLength is null)
            {
                _logger.LogInformation($"Failed to get size for {url}, ContentLength is null.");
                return 0;
            }

            return (int)response.Content.Headers.ContentLength;
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"Failed to fetch filesize, {ex.Message}");
            return 0;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task ScrapeFileSizes()
    {
        _logger.LogInformation("Starting file size scraping...");
        var count = await _filesizeQueue.Count();
        if (count == 0)
        {
            _logger.LogInformation("No queue items found.");
            return;
        }
        
        var queue = await _filesizeQueue.GetAll();
        var queued = new List<Task>();
        
        _logger.LogInformation($"Processing {count} urls filesize information...");

        foreach (var item in queue)
        {
            queued.Add(Task.Run(async () =>
            {
                item.Size = await GetFileSize(item.Url);
                await _filesizeQueue.Update(item);
            }));
        }

        while (queued.Count > 0)
        {
            if (queued[0].IsCompletedSuccessfully)
            {
                queued.Remove(queued[0]);
                continue;
            }

            Thread.Sleep(500);
        }

        foreach (var version in await _versionService.Get())
        {
            var sizes = await _filesizeQueue.GetByVersion(version);
            var remove = new List<FilesizeQueue>();
            if (sizes.Count == 0) continue;
            foreach (var item in sizes)
            {
                if (item.Size == 0) continue;
                var vurl = item.ClassLocation.Count == 2
                    ? version[item.ClassLocation[0], item.ClassLocation[1]]
                    : version[item.ClassLocation[0]];
                if (vurl is null) continue;
                vurl.Size = item.Size;
                remove.Add(item);
            }

            await _versionService.Update(version.Id, version);
            await _filesizeQueue.BulkRemove(remove);
        }

        var ncount = await _filesizeQueue.Count();
        _logger.LogInformation($"Filesize Scraping completed.  Found {count - ncount} out of {count} filesize information.");
        if (ncount > 0)
        {
            BackgroundJob.Schedule(
                () => ScrapeFileSizes(),
                TimeSpan.FromMinutes(1));
            _logger.LogInformation("Re-Scheduled Filesize Scrape run.");
        }
    }
}