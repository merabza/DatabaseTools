namespace DbTools.Models;

public sealed class DbAuthSettings : DbAuthSettingsBase
{
    public DbAuthSettings(string serverUser, string serverPass)
    {
        ServerUser = serverUser;
        ServerPass = serverPass;
    }

    public string ServerUser { get; set; }
    public string ServerPass { get; set; }
}