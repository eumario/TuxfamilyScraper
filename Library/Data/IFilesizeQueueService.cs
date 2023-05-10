using MongoDB.Bson;
using TuxfamilyScraper.Library.Models;

namespace TuxfamilyScraper.Library.Data;

public interface IFilesizeQueueService
{
    Task Create(FilesizeQueue queue);
    Task<List<FilesizeQueue>> GetAll();
    Task Remove(ObjectId id);
}