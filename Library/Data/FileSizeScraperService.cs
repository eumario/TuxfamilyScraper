namespace TuxfamilyScraper.Library.Data;

public class FileSizeScraperService : IFileSizeScraperService
{
    private readonly ITuxfamilyVersionService _versionService;
    private readonly ILogger _logger;

    public FileSizeScraperService(ITuxfamilyVersionService versionService, ILogger<FileSizeScraperService> logger)
    {
        _versionService = versionService;
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
        _logger.LogInformation($"Successfully fetched size {response.Content.Headers.ContentLength} for {url}");
        return (int)response.Content.Headers.ContentLength;
    }
    public async Task ScrapeFileSizes()
    {
        _logger.LogInformation("Starting file size scraping...");
    }
}