namespace DbTools.Models;

public sealed class DatabaseInfoModel
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public DatabaseInfoModel(string name, EDatabaseRecovery recoveryModel, bool isSystemDatabase)
    {
        Name = name;
        RecoveryModel = recoveryModel;
        IsSystemDatabase = isSystemDatabase;
    }

    public string Name { get; set; }
    public EDatabaseRecovery RecoveryModel { get; set; }
    public bool IsSystemDatabase { get; set; }
}