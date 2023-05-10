namespace TuxfamilyScraper.Library.Data;

public class FileSizeScraperService : IFileSizeScraperService
{
    private readonly ITuxfamilyVersionService _versionService;
    private readonly IFilesizeQueueService _filesizeQueue;
    private readonly ILogger _logger;

    public FileSizeScraperService(ITuxfamilyVersionService versionService, IFilesizeQueueService filesizeQueue,
        ILogger<FileSizeScraperService> logger)
    {
        _versionService = versionService;
        _filesizeQueue = filesizeQueue;
        _logger = logger;
    }

    private async Task<int> GetFileSize(string url)
    {
        var client = new HttpClient();
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
        client.Dispose();
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogInformation($"Failed with status {response.StatusCode} for url: {url}");
            return 0;
        }

        if (response.Content.Headers.ContentLength is null)
        {
            _logger.LogInformation($"Failed to fetch size information for url: {url}");
            return 0;
        }
        //_logger.LogInformation($"Successfully fetched size {response.Content.Headers.ContentLength} for {url}");
        return (int)response.Content.Headers.ContentLength;
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
        
        _logger.LogInformation($"Processing {count} urls filesize information...");

        foreach (var item in queue)
        {
            var size = await GetFileSize(item.Url);
            if (size == 0) continue;

            var version = await _versionService.Get(item.Version.Id);

            if (item.ClassLocation.Count == 2)
            {
                var vurl = version[item.ClassLocation[0], item.ClassLocation[1]];
                if (vurl is null) continue;
                vurl.Size = size;
                await _versionService.Update(version.Id, version);
                await _filesizeQueue.Remove(item.Id);
            }
            else
            {
                var vurl = version[item.ClassLocation[0]];
                if (vurl is null) continue;
                vurl.Size = size;
                await _versionService.Update(version.Id, version);
                await _filesizeQueue.Remove(item.Id);
            }
        }

        var ncount = await _filesizeQueue.Count();
        _logger.LogInformation($"Filesize Scraping completed.  Found {count - ncount} out of {count} filesize information.");
    }
}