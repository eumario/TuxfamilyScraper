using MongoDB.Bson;

namespace TuxfamilyScraper.Library.Settings;

public class TuxfamilyDatabaseSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string VersionCollectionName { get; set; }
    public string LatestVersionCollectionName { get; set; }
    public string FilesizeQueueCollectionName { get; set; }
    public string AdminCollectionName { get; set; }
    public string IgnoreReleaseCollectionName { get; set; }
}