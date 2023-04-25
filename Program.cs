using TuxfamilyScraper.Library.Services;
using TuxfamilyScraper.Library.Settings;
using TuxfamilyScraper.Library.SimpleAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TuxfamilyDatabaseSettings>(builder.Configuration.GetSection("TuxfamilyDatabaseSettings"));

builder.ConfigureHangfireServices();

builder.Services.AddEndpointDefinitions(typeof(IEndpointDefinition));

var app = builder.Build();
app.MapGet("/", () => "Godot Tuxfamily Mirror Scraper!");
app.UseEndpointDefinitions();

app.UseHangfireDashboard();

app.Run("http://localhost:7010");