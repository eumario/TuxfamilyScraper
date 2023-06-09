using TuxfamilyScraper.Library.Data;
using TuxfamilyScraper.Library.Models;
using TuxfamilyScraper.Library.SimpleAPI;

namespace TuxfamilyScraper.Library.Endpoints;

public class ScrapeEndpoint : IEndpointDefinition
{
    public void DefineServices(IServiceCollection services)
    {
        services.AddSingleton<IFilesizeQueueService, FilesizeQueueService>();
        services.AddSingleton<IScraperService, ScraperService>();
        services.AddSingleton<IFileSizeScraperService, FileSizeScraperService>();
    }

    public void DefineEndpoints(WebApplication app)
    {
        
    }
}