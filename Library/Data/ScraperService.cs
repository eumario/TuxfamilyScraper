using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Hangfire;
using HtmlAgilityPack;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using TuxfamilyScraper.Library.Extensions;
using TuxfamilyScraper.Library.Models;

namespace TuxfamilyScraper.Library.Data;

public class ScraperService : IScraperService
{
    private readonly ITuxfamilyVersionService _versionService;
    private readonly IFilesizeQueueService _filesizeQueue;
    private readonly IFileSizeScraperService _fileSizeScraper;
    private readonly ILogger _logger;
    private readonly Regex _versionMatch = new Regex(@"\d+(?:\.\d+)+");
    private const string Url = "https://downloads.tuxfamily.org/godotengine/";

    private List<FilesizeQueue> _foundUrls = new List<FilesizeQueue>();

    private readonly string[] _linux32 = new[]
    {
        "linux_x86_32", "linux.x86_32",
        "x11_32", "linux_32",
        "x11.32", "linux.32",
    };
    
    private readonly string[] _linux64 = new []
    {
        "linux.x86_64", "linux_x86_64",
        "x11_64", "x11.64",
        "linux_64", "linux.64",
    };

    private readonly string[] _windows32 = new[]
    {
        "win_32", "win32", "win32.exe",
    };
    
    private readonly string[] _windows64 = new[]
    {
            "win_64", "win64", "win64.exe",
    };

    private readonly string[] _macOS = new[]
    {
        "osx32", "osx.universal", "macos.universal",
        "osx64", "osx.64", "osx.fat",
    };

    private readonly string[] _headless = new[]
    {
        "linux_headless.64",
        "linux_headless_64",
    };

    private readonly string[] _server = new[]
    {
        "linux_server.64",
        "linux_server_64",
    };

    private readonly string _android = "android_editor.apk";
    private readonly string _androidLibs = ".aar";
    private readonly string _templates = ".tpz";
    private readonly string _source = "tar.xz";
    private readonly string _sha512 = "SHA512-SUMS.txt";

    private readonly string[] _releases = new[]
    {
        "rc", "alpha", "beta", "dev",
    };

    public ScraperService(ITuxfamilyVersionService versionService, IFilesizeQueueService filesizeQueue,
        IFileSizeScraperService fileSizeScraper, ILogger<ScraperService> logger)
    {
        _versionService = versionService;
        _fileSizeScraper = fileSizeScraper;
        _filesizeQueue = filesizeQueue;
        _logger = logger;
    }

    private bool IsVersionStored(string version, string release)
    {
        var res = _versionService.GetByVersion(version, release);
        while (!res.IsCompleted)
            Thread.Sleep(500);
        return res.Result is not null;
    }

    private Dictionary<string, string>? GatherVersions()
    {
        Dictionary<string, string> urls = new Dictionary<string, string>();
        var web = new HtmlWeb();
        var doc = web.Load(Url);

        var links = doc.DocumentNode.SelectNodes("//tr/td/a");

        if (links is null)
            return null;

        var found = links.Where(link => _versionMatch.IsMatch(link.InnerText))
            .Select(link => new { link.InnerText, link.Attributes["href"].Value }).ToList();
        
        _logger.LogInformation($"Found {found.Count()} links.");

        foreach (var link in found)
        {
            _logger.LogInformation($"Found Version: {link.InnerText} [HREF: {link.Value}]");
            urls[link.InnerText] = link.Value;
        }

        return urls;
    }

    private void GatherZips(string url, TuxfamilyVersion version, VersionUrls urls)
    {
        GatherZips(new HtmlWeb().Load(url), url, version, urls);
    }
    
    private void GatherZips(HtmlDocument doc, string url, TuxfamilyVersion version, VersionUrls urls)
    {
        var uri = new Uri(url);

        var links = doc.DocumentNode.SelectNodes("//tr/td/a");

        var zips = links?.Where(link => link.InnerText.EndsWith(".zip")).Select(link => link.Attributes["href"].Value);

        if (zips is null)
        {
            return;
        }

        foreach (var zip in zips)
        {
            var fsQueue = new FilesizeQueue
            {
                Version = version,
                ClassLocation = new List<string> { url.Contains("mono") ? "CSharp" : "Standard" }
            };

            if (_macOS.Any(x => zip.Contains(x)))
            {
                urls.OSX.Url = fsQueue.Url = uri.Append(zip).AbsoluteUri;
                fsQueue.ClassLocation.Add("OSX");
            }

            if (_linux32.Any(x => zip.Contains(x)))
            {
                urls.Linux32.Url = fsQueue.Url = uri.Append(zip).AbsoluteUri;
                fsQueue.ClassLocation.Add("Linux32");
            }

            if (_linux64.Any(x => zip.Contains(x)))
            {
                urls.Linux64.Url = fsQueue.Url =  uri.Append(zip).AbsoluteUri;
                fsQueue.ClassLocation.Add("Linux64");
            }

            if (_windows32.Any(x => zip.Contains(x)))
            {
                urls.Win32.Url = fsQueue.Url =  uri.Append(zip).AbsoluteUri;
                fsQueue.ClassLocation.Add("Win32");
            }

            if (_windows64.Any(x => zip.Contains(x)))
            {
                urls.Win64.Url = fsQueue.Url =  uri.Append(zip).AbsoluteUri;
                fsQueue.ClassLocation.Add("Win64");
            }

            if (!string.IsNullOrEmpty(fsQueue.Url))
                _foundUrls.Add(fsQueue);
        }

        if (links is null)
            return;
        
        foreach (var link in links)
        {
            var fsQueue = new FilesizeQueue
            {
                Version = version,
                ClassLocation = new List<string> { url.Contains("mono") ? "CSharp" : "Standard" }
            };
            if (link.InnerText.EndsWith(_templates))
            {
                urls.Templates.Url = fsQueue.Url = uri.Append(link.Attributes["href"].Value).AbsoluteUri;
                fsQueue.ClassLocation.Add("Templates");
            }

            if (_headless.Any(x => link.InnerText.Contains(x)))
            {
                urls.Headless.Url = fsQueue.Url = uri.Append(link.Attributes["href"].Value).AbsoluteUri;
                fsQueue.ClassLocation.Add("Headless");
            }

            if (_server.Any(x => link.InnerText.Contains(x)))
            {
                urls.Server.Url = fsQueue.Url = uri.Append(link.Attributes["href"].Value).AbsoluteUri;
                fsQueue.ClassLocation.Add("Server");
            }

            if (!string.IsNullOrEmpty(fsQueue.Url))
                _foundUrls.Add(fsQueue);
        }
    }

    private TuxfamilyVersion? ProcessRelease(string baseUrl, string version, string release)
    {
        return ProcessRelease(new HtmlWeb().Load(baseUrl), baseUrl, version, release);
    }

    private TuxfamilyVersion? ProcessRelease(HtmlDocument doc, string baseUrl, string version, string release)
    {
        if (IsVersionStored(version, release)) return null;
        
        var tfVersion = new TuxfamilyVersion()
        {
            TagName = version,
            ReleaseStage = release,
            Standard = new VersionUrls(),
            CSharp = new VersionUrls(),
            Sha512Sums = new VersionUrl(),
            Source = new VersionUrl(),
            AndroidLibs = new VersionUrl(),
            AndroidEditor = new VersionUrl(),
        };
        
        GatherZips(doc, baseUrl, tfVersion, tfVersion.Standard);
        if (string.IsNullOrEmpty(tfVersion.Standard.Win64.Url)) return null;
        
        var links = doc.DocumentNode.SelectNodes("//tr/td/a");
        if (links.Select(x => x.InnerText == "mono").ToList().Count > 0)
            GatherZips(new Uri(baseUrl).Append("mono").AbsoluteUri, tfVersion, tfVersion.CSharp);
        var uri = new Uri(baseUrl);
        foreach (var link in links)
        {
            var fsQueue = new FilesizeQueue
            {
                Version = tfVersion,
                ClassLocation = new List<string>()
            };
            if (link.InnerText.Contains(_android))
            {
                tfVersion.AndroidEditor.Url = fsQueue.Url = uri.Append(link.Attributes["href"].Value).AbsoluteUri;
                fsQueue.ClassLocation.Add("AndroidEditor");
            }

            if (link.InnerText.EndsWith(_source))
            {
                tfVersion.Source.Url = fsQueue.Url = uri.Append(link.Attributes["href"].Value).AbsoluteUri;
                fsQueue.ClassLocation.Add("Source");
            }

            if (link.InnerText.Contains(_androidLibs))
            {
                tfVersion.AndroidLibs.Url = fsQueue.Url = uri.Append(link.Attributes["href"].Value).AbsoluteUri;
                fsQueue.ClassLocation.Add("AndroidLibs");
            }

            if (link.InnerText.Contains(_sha512))
            {
                tfVersion.Sha512Sums.Url = fsQueue.Url = uri.Append(link.Attributes["href"].Value).AbsoluteUri;
                fsQueue.ClassLocation.Add("Sha512Sums");
            }
            
            if (!string.IsNullOrEmpty(fsQueue.Url))
                _foundUrls.Add(fsQueue);
        }
        return tfVersion;
    }

    private List<TuxfamilyVersion> ProcessUrls(string baseUrl, string version, string release = "Stable")
    {
        var versions = new List<TuxfamilyVersion>();
        var web = new HtmlWeb();
        var uri = new Uri(baseUrl);
        var doc = web.Load(uri);

        var tfVersion = ProcessRelease(doc, baseUrl, version, release);
        if (tfVersion is not null)
            versions.Add(tfVersion);

        var links = doc.DocumentNode.SelectNodes("//tr/td/a");

        var releases = links.Where(x => _releases.Any(y => x.InnerText.Contains(y)))
            .Select(x => new {x.InnerText, x.Attributes["href"].Value});

        foreach (var rel in releases)
        {
            var res = ProcessRelease(uri.Append(rel.Value).AbsoluteUri, version, 
                rel.InnerText.StartsWith("rc")
                ? rel.InnerText.ToUpper()
                : rel.InnerText.Capitalize());
            if (res is not null)
                versions.Add(res);
        }

        return versions;
    }

    public async Task ScrapeSite()
    {
        _logger.LogInformation("Starting scraping...");
        var urls = GatherVersions();
        var versions = new List<TuxfamilyVersion>();
        if (urls is null)
        {
            _logger.LogError("No Version information found!");
            return;
        }

        var foundVersions = new List<TuxfamilyVersion>();

        foreach (var processedVersions in 
                 urls.Keys.Select(version =>
                     ProcessUrls(new Uri(Url).Append(version).AbsoluteUri, version))
                     .Where(processedVersions => processedVersions.Count > 0)
                 )
            foundVersions.AddRange(processedVersions);

        if (foundVersions.Count > 0)
        {
            await _versionService.BulkCreate(foundVersions);
            _logger.LogInformation($"Found {foundVersions.Count} new versions, and added to database.");
        }

        if (_foundUrls.Count > 0)
        {
            await _filesizeQueue.BulkCreate(_foundUrls);
            _logger.LogInformation($"Queued {_foundUrls.Count} new URLs to filesize scrape.");
            BackgroundJob.Schedule(
                () => _fileSizeScraper.ScrapeFileSizes(),
                TimeSpan.FromMinutes(2));
            _logger.LogInformation("Scheduled Filesize Scrape run.");
        }

        _logger.LogInformation("Finished Scrapping.");
    }
}