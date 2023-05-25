using MongoDB.Bson;

namespace TuxfamilyScraper.Library.Models;

public class IgnoreRelease
{
    public ObjectId Id;
    public string Tag { get; set; }
}