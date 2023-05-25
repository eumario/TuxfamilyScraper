using Microsoft.Extensions.Options;
using TuxfamilyScraper.Library.Data;
using TuxfamilyScraper.Library.Models;
using TuxfamilyScraper.Library.Settings;
using TuxfamilyScraper.Library.SimpleAPI;

namespace TuxfamilyScraper.Library.Endpoints;

public class IgnoreEndpoint : IEndpointDefinition
{
    public void DefineServices(IServiceCollection services)
    {
        services.AddSingleton<IIgnoreReleaseService, IgnoreReleaseService>();
        services.AddSingleton<IAdminPassword, AdminPassword>();
    }

    public void DefineEndpoints(WebApplication app)
    {
        app.MapGet("/ignoreRelease/{releaseId}/{password}", AddIgnoreRelease);
    }

    internal IResult AddIgnoreRelease(IAdminPassword adminPassword, IIgnoreReleaseService ignoreRelease, string releaseId, string password)
    {
        if (adminPassword.GetPassword() != password)
            return Results.NotFound();
        var task = ignoreRelease.Create(new IgnoreRelease { Tag = releaseId });
        while (!task.IsCompleted)
            Thread.Sleep(500);
        return task.IsCompletedSuccessfully ? Results.Ok(new Dictionary<string, string> { { "Result", "Ok" } }) : Results.StatusCode(500);
    }
}