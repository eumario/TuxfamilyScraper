using MongoDB.Bson;
using Semver;
using TuxfamilyScraper.Library.Models;

namespace TuxfamilyScraper.Library.Data;

public class LatestScraper : ILatestScraper
{
    private readonly ITuxfamilyVersionService _versionService;
    private readonly IIgnoreReleaseService _ignoreRelease;
    private readonly ILatestReleaseService _latestRelease;
    private readonly ILogger _logger;

    public LatestScraper(ITuxfamilyVersionService versionService, IIgnoreReleaseService ignoreRelease,
        ILatestReleaseService latestRelease, ILogger<LatestScraper> logger)
    {
        _versionService = versionService;
        _ignoreRelease = ignoreRelease;
        _latestRelease = latestRelease;
        _logger = logger;
    }
    
    public async Task ScrapeLatest()
    {
        _logger.LogInformation("Starting check for latest versions...");
        foreach (var version in await _versionService.Get())
        {
            var semvers = new SemVersion(0,0,0);
            var verString = version.ReleaseStage != "Stable"
                ? $"{version.TagName}-{version.ReleaseStage.ToLower()}"
                : version.TagName;
            
            try
            {
                semvers = SemVersion.Parse(verString, SemVersionStyles.Any);
            } catch { }

            if (semvers.Major == 0)
            {
                _logger.LogInformation($"Unable to parse {version.TagName}");
                continue;
            }
            if (await _latestRelease.HasMajor(semvers.Major))
            {
                var major = await _latestRelease.Get(semvers.Major);

                switch (semvers.IsPrerelease)
                {
                    case true when major.Prerelease == ObjectId.Empty:
                        major.Prerelease = version.Id;
                        _logger.LogInformation($"New Pre-Release Found: {version.TagName}-{version.ReleaseStage}");
                        await _latestRelease.Update(major);
                        continue;
                    case false when major.Release == ObjectId.Empty:
                        major.Release = version.Id;
                        _logger.LogInformation($"New Stable Release Found: {version.TagName}");
                        await _latestRelease.Update(major);
                        continue;
                }

                var majorRelease = await _versionService.Get(semvers.IsPrerelease ? major.Prerelease : major.Release);

                try
                {
                    var mvsemvers = SemVersion.Parse(semvers.IsPrerelease
                        ? $"{majorRelease.TagName}-{majorRelease.ReleaseStage.ToLower()}"
                        : majorRelease.TagName, SemVersionStyles.Any);
                    if (mvsemvers is null) continue;
                    if (SemVersion.ComparePrecedence(mvsemvers, semvers) > 0) continue;
                } catch { }

                _logger.LogInformation(!semvers.IsPrerelease
                    ? $"Found new stable {major.Major}.x release ({version.TagName})"
                    : $"Found new pre-release {major.Major}.x release, {version.TagName}-{version.ReleaseStage}");

                major.Prerelease = semvers.IsPrerelease ? version.Id : major.Prerelease;
                major.Release = !semvers.IsPrerelease ? version.Id : major.Release;

                await _latestRelease.Update(major);
            }
            else
            {
                _logger.LogInformation($"Found new Major Release. {semvers.Major}.x  ({version.TagName})");
                var major = new LatestRelease()
                {
                    Major = semvers.Major,
                    Release = !semvers.IsPrerelease ? version.Id : ObjectId.Empty,
                    Prerelease = semvers.IsPrerelease ? version.Id : ObjectId.Empty
                };
                await _latestRelease.Add(major);
            }
        }
        _logger.LogInformation("Finished checking for latest versions.");
    }
}