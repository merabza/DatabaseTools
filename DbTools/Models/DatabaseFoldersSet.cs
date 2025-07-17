using LibParameters;

namespace DbTools.Models;

public sealed class DatabaseFoldersSet : ItemData
{
    public string? Backup { get; set; }
    public string? Data { get; set; }
    public string? DataLog { get; set; }

    public override string GetItemKey()
    {
        return $"{Backup} {Data} {DataLog}";
    }
}