using MongoDB.Bson;
using TuxfamilyScraper.Library.Models;

namespace TuxfamilyScraper.Library.Data;

public interface ITuxfamilyVersionService
{
    Task<List<TuxfamilyVersion>> Get();
    Task<TuxfamilyVersion> Get(string id);
    Task<List<TuxfamilyVersion>> GetByTag(string tag);
    Task<TuxfamilyVersion> GetByVersion(string version, string release);
    Task Create(TuxfamilyVersion version);
    Task BulkCreate(List<TuxfamilyVersion> versions);
    Task Update(string id, TuxfamilyVersion version);
    Task Remove(string id);
}