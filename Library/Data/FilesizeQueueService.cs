using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using TuxfamilyScraper.Library.Models;
using TuxfamilyScraper.Library.Settings;

namespace TuxfamilyScraper.Library.Data;

public class FilesizeQueueService : IFilesizeQueueService
{
    private readonly IMongoCollection<FilesizeQueue> _filesizeQueue;

    public FilesizeQueueService(IOptions<TuxfamilyDatabaseSettings> options)
    {
        var mongoClient = new MongoClient(options.Value.ConnectionString);
        _filesizeQueue = mongoClient.GetDatabase(options.Value.DatabaseName)
            .GetCollection<FilesizeQueue>(options.Value.FilesizeQueueCollectionName);
    }

    public async Task Create(FilesizeQueue queue) => await _filesizeQueue.InsertOneAsync(queue);

    public async Task<List<FilesizeQueue>> GetAll() => await _filesizeQueue.Find(_ => true).ToListAsync();

    public async Task Remove(ObjectId id) => await _filesizeQueue.DeleteOneAsync(m => m.Id == id);
}