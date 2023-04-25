using TuxfamilyScraper.Library.Data;
using TuxfamilyScraper.Library.SimpleAPI;

namespace TuxfamilyScraper.Library.Endpoints;

public class ScrapeEndpoint : IEndpointDefinition
{
    public void DefineServices(IServiceCollection services)
    {
        services.AddSingleton<IScraperService, ScraperService>();
    }

    public void DefineEndpoints(WebApplication app)
    {
        
    }
}