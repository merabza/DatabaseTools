using SystemTools.SystemToolsShared.Errors;

namespace DatabaseTools.DbTools.Errors;

public static class DbClientErrors
{
    public static readonly Error CannotCreateDatabaseConnection = new()
    {
        Code = nameof(CannotCreateDatabaseConnection), Name = "Cannot create Database connection"
    };

    public static readonly Error NoBackupFolder = new()
    {
        Code = nameof(NoBackupFolder), Name = "No information about Backup folder"
    };

    public static readonly Error NoRestoreFrom = new()
    {
        Code = nameof(NoRestoreFrom), Name = "No information about from folder to restore"
    };

    public static readonly Error NoDataFolder = new()
    {
        Code = nameof(NoDataFolder), Name = "No information about data folder to restore"
    };

    public static readonly Error NoDataLogFolder = new()
    {
        Code = nameof(NoDataLogFolder), Name = "No information about data log folder to restore"
    };

    public static readonly Error NoRestoreFileNames = new()
    {
        Code = nameof(NoRestoreFileNames), Name = "No information about restore file logical parts"
    };

    public static readonly Error NoDataPart = new()
    {
        Code = nameof(NoDataPart), Name = "No information about restore file Data Part"
    };

    public static readonly Error NoLogPart = new()
    {
        Code = nameof(NoLogPart), Name = "No information about restore file Log Part"
    };

    public static readonly Error ConnectionServerDoesNotSpecified = new()
    {
        Code = nameof(ConnectionServerDoesNotSpecified), Name = "Connection Server does Not specified"
    };

    public static readonly Error DatabaseNameIsNotSpecified = new()
    {
        Code = nameof(DatabaseNameIsNotSpecified),
        Name = "Test Connection Succeeded, But Database name does Not specified"
    };

    public static readonly Error DatabaseNameIsNotSpecifiedForBackup = new()
    {
        Code = nameof(DatabaseNameIsNotSpecified), Name = "Database Name is Not Specified For Backup"
    };

    public static Error ConnectionFailed(string message)
    {
        return new Error { Code = nameof(ConnectionFailed), Name = $"Connection Failed {message}" };
    }

    public static Error ExecuteScalarAsyncResultIsNull()
    {
        return new Error { Code = nameof(ExecuteScalarAsyncResultIsNull), Name = "ExecuteScalarAsync Result Is Null" };
    }
}
