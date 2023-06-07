namespace DbTools.Models;

public sealed class DbServerInfo
{
    public DbServerInfo(string serverProductVersion, string serverInstanceName, string? backupDirectory,
        string? defaultDataDirectory, string? defaultLogDirectory, bool allowsCompression, string? serverName)
    {
        ServerProductVersion = serverProductVersion;
        ServerInstanceName = serverInstanceName;
        BackupDirectory = backupDirectory;
        DefaultDataDirectory = defaultDataDirectory;
        DefaultLogDirectory = defaultLogDirectory;
        AllowsCompression = allowsCompression;
        ServerName = serverName;
    }

    public string ServerProductVersion { get; set; }
    public string ServerInstanceName { get; set; }
    public string? BackupDirectory { get; set; }
    public string? DefaultDataDirectory { get; set; }
    public string? DefaultLogDirectory { get; set; }
    public bool AllowsCompression { get; set; }
    public string? ServerName { get; set; }
}