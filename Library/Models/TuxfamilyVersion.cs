using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TuxfamilyScraper.Library.Models;

public class TuxfamilyVersion
{
    [BsonId]
    public Guid Id { get; set; }
    public string TagName { get; set; }
    public VersionUrls Standard { get; set; }
    public VersionUrls CSharp { get; set; }
    public VersionUrl Sha512Sums { get; set; }
    public VersionUrl Source { get; set; }
    public VersionUrl AndroidEditor { get; set; }
    public VersionUrl AndroidLibs { get; set; }
    public string ReleaseStage { get; set; }

    public VersionUrl? this[string key] => key switch
    {
        "Sha512Sums" => Sha512Sums,
        "Source" => Source,
        "AndroidEditor" => AndroidEditor,
        "AndroidLibs" => AndroidLibs,
        _ => null
    };

    public VersionUrl? this[string key, string subkey] => key switch
    {
        "Standard" => Standard[subkey],
        "CSharp" => CSharp[subkey],
        _ => null
    };
}