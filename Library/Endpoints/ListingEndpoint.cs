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
        
    }
}