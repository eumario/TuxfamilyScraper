using MongoDB.Bson;
using TuxfamilyScraper.Library.Models;

namespace TuxfamilyScraper.Library.Data;

public interface IFilesizeQueueService
{
    Task Create(FilesizeQueue queue);
    Task BulkCreate(List<FilesizeQueue> queues);
    Task<List<FilesizeQueue>> GetAll();
    Task<List<FilesizeQueue>> GetByVersion(TuxfamilyVersion version);
    Task<int> Count();
    Task Update(FilesizeQueue queue);
    Task Remove(ObjectId id);
    Task Remove(FilesizeQueue queue);
    Task BulkRemove(List<FilesizeQueue> queues);
}