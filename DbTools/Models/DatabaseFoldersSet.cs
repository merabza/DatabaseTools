using LibParameters;

namespace DbTools.Models;

public class DatabaseFoldersSet : ItemData
{
    public string? Backup { get; set; }
    public string? Data { get; set; }
    public string? DataLog { get; set; }


    public string GetStatus()
    {
        return $"{Backup} {Data} {DataLog}";
    }

}