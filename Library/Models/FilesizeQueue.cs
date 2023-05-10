using MongoDB.Bson;

namespace TuxfamilyScraper.Library.Models;

public class FilesizeQueue
{
    public ObjectId Id { get; set; }
    public TuxfamilyVersion Version { get; set; }
    public List<string> ClassLocation { get; set; }
    public string Url { get; set; }
    public int Size { get; set; }
    
    // ClassLocation Values:
    // {"Standard","Win32"}, {"Standard","Win64"}, {"Standard", "Linux32"}, {"Standard", "Linux64"}
    // {"CSharp","Win32"}, {"CSharp","Win64"}, {"CSharp", "Linux32"}, {"CSharp", "Linux64"}
    // {"Source"}, {"AndroidEditor"}, {"AndroidLibs"}

}