using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using TuxfamilyScraper.Library.Models;
using TuxfamilyScraper.Library.Settings;

namespace TuxfamilyScraper.Library.Data;

public class TuxfamilyVersionService : ITuxfamilyVersionService
{
    private readonly IMongoCollection<TuxfamilyVersion> _versions;

    public TuxfamilyVersionService(IOptions<TuxfamilyDatabaseSettings> options)
    {
        var mongoClient = new MongoClient(options.Value.ConnectionString);
        _versions = mongoClient.GetDatabase(options.Value.DatabaseName)
            .GetCollection<TuxfamilyVersion>(options.Value.VersionCollectionName);
    }

    public async Task<List<TuxfamilyVersion>> Get() => await _versions.Find(_ => true).ToListAsync();

    public async Task<TuxfamilyVersion> Get(string id)
    {
        var objId = new ObjectId(id);
        return await _versions.Find(ver => ver.Id == objId).FirstOrDefaultAsync();
    }

    public async Task<List<TuxfamilyVersion>> GetByTag(string tag) =>
        await _versions.Find(ver => ver.ReleaseStage == tag).ToListAsync();

    public async Task<TuxfamilyVersion> GetByVersion(string version, string release) =>
        await _versions.Find(ver => ver.TagName == version && ver.ReleaseStage == release).FirstOrDefaultAsync();

    public async Task Create(TuxfamilyVersion version) => await _versions.InsertOneAsync(version);
    public async Task BulkCreate(List<TuxfamilyVersion> versions) => await _versions.InsertManyAsync(versions);

    public async Task Update(string id, TuxfamilyVersion version)
    {
        var objId = new ObjectId(id);
        await _versions.ReplaceOneAsync(m => m.Id == objId, version);
    }

    public async Task Remove(string id)
    {
        var objId = new ObjectId(id);
        await _versions.DeleteOneAsync(m => m.Id == objId);
    }
}