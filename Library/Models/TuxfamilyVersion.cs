using MongoDB.Bson;

namespace TuxfamilyScraper.Library.Models;

public class TuxfamilyVersion
{
    public ObjectId Id { get; set; }
    public string TagName { get; set; }
    public VersionUrls Standard { get; set; }
    public VersionUrls CSharp { get; set; }
    public VersionUrl Sha512Sums { get; set; }
    public VersionUrl Source { get; set; }
    public VersionUrl AndroidEditor { get; set; }
    public VersionUrl AndroidLibs { get; set; }
    public string ReleaseStage { get; set; } 
}