using SystemToolsShared.Errors;

namespace DbTools.Errors;

public static class DbClientErrors
{
    public static readonly Err CannotCreateDatabaseConnection = new()
    {
        ErrorCode = nameof(CannotCreateDatabaseConnection), ErrorMessage = "Cannot create Database connection"
    };

    public static readonly Err NoBackupFolder = new()
    {
        ErrorCode = nameof(NoRestoreFileNames), ErrorMessage = "No information about Backup folder"
    };

    public static readonly Err NoRestoreFrom = new()
    {
        ErrorCode = nameof(NoRestoreFileNames), ErrorMessage = "No information about from folder to restore"
    };

    public static readonly Err NoDataFolder = new()
    {
        ErrorCode = nameof(NoRestoreFileNames), ErrorMessage = "No information about data folder to restore"
    };

    public static readonly Err NoDataLogFolder = new()
    {
        ErrorCode = nameof(NoRestoreFileNames), ErrorMessage = "No information about data log folder to restore"
    };

    public static readonly Err NoRestoreFileNames = new()
    {
        ErrorCode = nameof(NoRestoreFileNames), ErrorMessage = "No information about restore file logical parts"
    };

    public static readonly Err NoDataPart = new()
    {
        ErrorCode = nameof(NoDataPart), ErrorMessage = "No information about restore file Data Part"
    };

    public static readonly Err NoLogPart = new()
    {
        ErrorCode = nameof(NoLogPart), ErrorMessage = "No information about restore file Log Part"
    };

    public static readonly Err ConnectionServerDoesNotSpecified = new()
    {
        ErrorCode = nameof(ConnectionServerDoesNotSpecified), ErrorMessage = "Connection Server does Not specified"
    };

    public static readonly Err DatabaseNameIsNotSpecified = new()
    {
        ErrorCode = nameof(DatabaseNameIsNotSpecified),
        ErrorMessage = "Test Connection Succeeded, But Database name does Not specified"
    };

    public static readonly Err DatabaseNameIsNotSpecifiedForBackup = new()
    {
        ErrorCode = nameof(DatabaseNameIsNotSpecified), ErrorMessage = "Database Name is Not Specified For Backup"
    };

    public static Err ConnectionFailed(string message)
    {
        return new Err { ErrorCode = nameof(ConnectionFailed), ErrorMessage = $"Connection Failed {message}" };
    }

    public static Err ExecuteScalarAsyncResultIsNull()
    {
        return new Err
        {
            ErrorCode = nameof(ExecuteScalarAsyncResultIsNull), ErrorMessage = "ExecuteScalarAsync Result Is Null"
        };
    }
}