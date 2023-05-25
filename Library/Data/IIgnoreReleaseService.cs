using MongoDB.Bson;
using TuxfamilyScraper.Library.Models;

namespace TuxfamilyScraper.Library.Data;

public interface IIgnoreReleaseService
{
    Task<List<IgnoreRelease>> GetAll();
    Task<bool> HasRelease(string tag);
    Task Create(IgnoreRelease release);
    Task Remove(string tag);
    Task Remove(ObjectId id);
    Task Remove(IgnoreRelease release);
}