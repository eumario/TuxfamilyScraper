namespace TuxfamilyScraper.Library.SimpleAPI;

public interface IEndpointDefinition
{
    void DefineServices(IServiceCollection services);

    void DefineEndpoints(WebApplication app);
}