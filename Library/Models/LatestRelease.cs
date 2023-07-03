using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TuxfamilyScraper.Library.Models;

public class LatestRelease
{
    [BsonId]
    public Guid Id { get; set; }
    public int Major { get; set; }
    public Guid Release { get; set; }
    public Guid Prerelease { get; set; }
}