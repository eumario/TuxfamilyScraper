using MongoDB.Bson;

namespace TuxfamilyScraper.Library.Models;

public class LatestRelease
{
    public ObjectId Id;
    public int Major;
    public ObjectId Release;
    public ObjectId Prerelease;
}