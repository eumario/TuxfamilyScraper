using TuxfamilyScraper.Library.Data;
using TuxfamilyScraper.Library.SimpleAPI;

namespace TuxfamilyScraper.Library.Endpoints;

public class ListingEndpoint : IEndpointDefinition
{
    public void DefineServices(IServiceCollection services)
    {
        services.AddSingleton<ITuxfamilyVersionService, TuxfamilyVersionService>();
    }

    public void DefineEndpoints(WebApplication app)
    {
        app.MapGet("/listings/", async (ITuxfamilyVersionService service) => await service.Get());
        app.MapGet("/listings/{version}/{release}",
            async (ITuxfamilyVersionService service, string version, string release) =>
                await service.GetByVersion(version, release));
        app.MapGet("/listings/tag/{tag}",
            async (ITuxfamilyVersionService service, string tag) => await service.GetByTag(tag));
        app.MapGet("/listings/{engineId}",
            async (ITuxfamilyVersionService service, string engineId) => await service.Get(engineId));
    }
}