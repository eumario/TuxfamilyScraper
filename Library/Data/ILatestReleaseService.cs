using MongoDB.Bson;
using TuxfamilyScraper.Library.Models;

namespace TuxfamilyScraper.Library.Data;

public interface ILatestReleaseService
{
    Task<bool> HasMajor(int major);
    Task<LatestRelease> Get(Guid id);
    Task<LatestRelease> Get(int major);
    Task<List<LatestRelease>> Get();
    Task Add(LatestRelease release);
    Task Update(Guid id, LatestRelease version);
    Task Update(LatestRelease version);
    Task Remove(Guid id);
    Task Remove(LatestRelease release);
}