using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TuxfamilyScraper.Library.Models;

public class LatestRelease
{
    [BsonId]
    [BsonRepresentation((BsonType.ObjectId))]
    public string? Id { get; set; }
    public int Major { get; set; }
    [BsonRepresentation((BsonType.ObjectId))]
    public string? Release { get; set; }
    [BsonRepresentation((BsonType.ObjectId))]
    public string? Prerelease { get; set; }
}