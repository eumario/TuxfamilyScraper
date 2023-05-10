using MongoDB.Bson;
using TuxfamilyScraper.Library.Models;

namespace TuxfamilyScraper.Library.Data;

public interface IFilesizeQueueService
{
    Task Create(FilesizeQueue queue);
    Task BulkCreate(List<FilesizeQueue> queues);
    Task<List<FilesizeQueue>> GetAll();
    Task<int> Count();
    Task Update(FilesizeQueue queue);
    Task Remove(ObjectId id);
}