using SystemTools.SharedKernel;

namespace DatabaseTools.DbTools.Errors;

public static class DbClientErrors
{
    public static Error NoBackupFolder => Error.Problem(nameof(NoBackupFolder), "No information about Backup folder");

    public static Error NoRestoreFrom =>
        Error.Problem(nameof(NoRestoreFrom), "No information about from folder to restore");

    public static Error NoDataFolder =>
        Error.Problem(nameof(NoDataFolder), "No information about data folder to restore");

    public static Error NoDataLogFolder =>
        Error.Problem(nameof(NoDataLogFolder), "No information about data log folder to restore");

    public static Error NoRestoreFileNames =>
        Error.Problem(nameof(NoRestoreFileNames), "No information about restore file logical parts");

    public static Error NoDataPart => Error.Problem(nameof(NoDataPart), "No information about restore file Data Part");

    public static Error NoLogPart => Error.Problem(nameof(NoLogPart), "No information about restore file Log Part");

    public static Error ConnectionServerDoesNotSpecified =>
        Error.Problem(nameof(ConnectionServerDoesNotSpecified), "Connection Server does Not specified");

    public static Error DatabaseNameIsNotSpecified =>
        Error.Problem(nameof(DatabaseNameIsNotSpecified),
            "Test Connection Succeeded, But Database name does Not specified");

    public static Error DatabaseNameIsNotSpecifiedForBackup =>
        Error.Problem(nameof(DatabaseNameIsNotSpecifiedForBackup), "Database Name is Not Specified For Backup");

    public static Error CannotCreateDatabaseConnection =>
        Error.Problem(nameof(CannotCreateDatabaseConnection), "Cannot create Database connection");

    public static Error ConnectionFailed(string message)
    {
        return Error.Problem(nameof(ConnectionFailed), $"Connection Failed {message}");
    }

    public static Error ExecuteScalarAsyncResultIsNull()
    {
        return Error.Problem(nameof(ExecuteScalarAsyncResultIsNull), "ExecuteScalarAsync Result Is Null");
    }
}
