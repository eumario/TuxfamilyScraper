using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using TuxfamilyScraper.Library.Models;
using TuxfamilyScraper.Library.Settings;

namespace TuxfamilyScraper.Library.Data;

public class IgnoreReleaseService : IIgnoreReleaseService
{
    private readonly IMongoCollection<IgnoreRelease> _ignoreRelease;
    
    public IgnoreReleaseService(IOptions<TuxfamilyDatabaseSettings> options)
    {
        var mongoClient = new MongoClient(options.Value.ConnectionString);
        _ignoreRelease = mongoClient.GetDatabase(options.Value.DatabaseName)
            .GetCollection<IgnoreRelease>(options.Value.VersionCollectionName);
    }

    public async Task<List<IgnoreRelease>> GetAll() => await _ignoreRelease.Find(_ => true).ToListAsync();

    public async Task<bool> HasRelease(string tag) => await _ignoreRelease.Find(a => a.Tag == tag).FirstOrDefaultAsync() is not null;

    public async Task Create(IgnoreRelease release) => await _ignoreRelease.InsertOneAsync(release);

    public async Task Remove(string tag) => await _ignoreRelease.DeleteOneAsync(m => m.Tag == tag);

    public async Task Remove(ObjectId id) => await _ignoreRelease.DeleteOneAsync(m => m.Id == id);

    public async Task Remove(IgnoreRelease release) => await _ignoreRelease.DeleteOneAsync(m => m.Id == release.Id);
}