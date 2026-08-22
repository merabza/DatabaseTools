using SystemTools.SystemToolsShared.Errors;

namespace DatabaseTools.DbTools.Errors;

public static class DbClientErrors
{
    public static readonly ErrorOmd CannotCreateDatabaseConnection = new()
    {
        Code = nameof(CannotCreateDatabaseConnection), Name = "Cannot create Database connection"
    };

    public static readonly ErrorOmd NoBackupFolder = new()
    {
        Code = nameof(NoBackupFolder), Name = "No information about Backup folder"
    };

    public static readonly ErrorOmd NoRestoreFrom = new()
    {
        Code = nameof(NoRestoreFrom), Name = "No information about from folder to restore"
    };

    public static readonly ErrorOmd NoDataFolder = new()
    {
        Code = nameof(NoDataFolder), Name = "No information about data folder to restore"
    };

    public static readonly ErrorOmd NoDataLogFolder = new()
    {
        Code = nameof(NoDataLogFolder), Name = "No information about data log folder to restore"
    };

    public static readonly ErrorOmd NoRestoreFileNames = new()
    {
        Code = nameof(NoRestoreFileNames), Name = "No information about restore file logical parts"
    };

    public static readonly ErrorOmd NoDataPart = new()
    {
        Code = nameof(NoDataPart), Name = "No information about restore file Data Part"
    };

    public static readonly ErrorOmd NoLogPart = new()
    {
        Code = nameof(NoLogPart), Name = "No information about restore file Log Part"
    };

    public static readonly ErrorOmd ConnectionServerDoesNotSpecified = new()
    {
        Code = nameof(ConnectionServerDoesNotSpecified), Name = "Connection Server does Not specified"
    };

    public static readonly ErrorOmd DatabaseNameIsNotSpecified = new()
    {
        Code = nameof(DatabaseNameIsNotSpecified),
        Name = "Test Connection Succeeded, But Database name does Not specified"
    };

    public static readonly ErrorOmd DatabaseNameIsNotSpecifiedForBackup = new()
    {
        Code = nameof(DatabaseNameIsNotSpecified), Name = "Database Name is Not Specified For Backup"
    };

    public static ErrorOmd ConnectionFailed(string message)
    {
        return new ErrorOmd { Code = nameof(ConnectionFailed), Name = $"Connection Failed {message}" };
    }

    public static ErrorOmd ExecuteScalarAsyncResultIsNull()
    {
        return new ErrorOmd
        {
            Code = nameof(ExecuteScalarAsyncResultIsNull), Name = "ExecuteScalarAsync Result Is Null"
        };
    }
}
