using MongoDB.Bson;
using TuxfamilyScraper.Library.Data;
using TuxfamilyScraper.Library.SimpleAPI;

namespace TuxfamilyScraper.Library.Endpoints;

public class LatestVersionEndpoint : IEndpointDefinition
{
    public void DefineServices(IServiceCollection services)
    {
        services.AddSingleton<ILatestReleaseService, LatestReleaseService>();
        services.AddSingleton<ILatestScraper, LatestScraper>();
    }

    public void DefineEndpoints(WebApplication app)
    {
        app.MapGet("/latest/", async (ILatestReleaseService service) => await service.Get());
    }
}