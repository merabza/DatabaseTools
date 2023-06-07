namespace DbTools.Models;

public sealed class RestoreFileModel
{
    public RestoreFileModel(string logicalName, string type)
    {
        LogicalName = logicalName;
        Type = type;
    }

    public string LogicalName { get; set; }
    public string Type { get; set; }
}