using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using TuxfamilyScraper.Library.Models;
using TuxfamilyScraper.Library.Settings;

namespace TuxfamilyScraper.Library.Data;

public class LatestReleaseService : ILatestReleaseService
{
    private readonly IMongoCollection<LatestRelease> _latestRelease;
    
    public LatestReleaseService(IOptions<TuxfamilyDatabaseSettings> options)
    {
        var mongoClient = new MongoClient(options.Value.ConnectionString);
        _latestRelease = mongoClient.GetDatabase(options.Value.DatabaseName)
            .GetCollection<LatestRelease>(options.Value.LatestVersionCollectionName);
    }
    
    public async Task<bool> HasMajor(int major)
    {
        var count = await _latestRelease.CountDocumentsAsync(_ => true);
        if (count == 0) return false;
        return await _latestRelease.Find(a => a.Major == major).FirstOrDefaultAsync() is not null;
    }

    public async Task<LatestRelease> Get(ObjectId id) => await _latestRelease.Find(a => a.Id == id).FirstOrDefaultAsync();

    public async Task<LatestRelease> Get(int major) =>
        await _latestRelease.Find(a => a.Major == major).FirstOrDefaultAsync();

    public async Task<List<LatestRelease>> Get() => await _latestRelease.Find(_ => true).ToListAsync();

    public async Task Add(LatestRelease release) => await _latestRelease.InsertOneAsync(release);

    public async Task Update(ObjectId id, LatestRelease version) =>
        await _latestRelease.ReplaceOneAsync(a => a.Id == id, version);

    public async Task Update(LatestRelease version) =>
        await _latestRelease.ReplaceOneAsync(a => a.Id == version.Id, version);

    public async Task Remove(ObjectId id) => await _latestRelease.DeleteOneAsync(a => a.Id == id);

    public async Task Remove(LatestRelease release) => await _latestRelease.DeleteOneAsync(a => a.Id == release.Id);
}