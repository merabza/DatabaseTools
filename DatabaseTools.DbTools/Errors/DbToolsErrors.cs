using SystemTools.SystemToolsShared.Errors;

namespace DatabaseTools.DbTools.Errors;

public static class DbToolsErrors
{
    public static ErrorOmd WindowsNtIntegratedSecurityIsOffAndServerUserDoesNotSpecifiedOrServerPassDoesNotSpecified =>
        new()
        {
            Code =
                nameof(WindowsNtIntegratedSecurityIsOffAndServerUserDoesNotSpecifiedOrServerPassDoesNotSpecified),
            Name =
                "WindowsNtIntegratedSecurity is off and serverUser does not specified or serverPass does not specified"
        };

    public static ErrorOmd ServerAddressIsEmptyCannotCreateSqlServerManagementClient =>
        new()
        {
            Code = nameof(ServerAddressIsEmptyCannotCreateSqlServerManagementClient),
            Name = "ServerAddress is empty, Cannot create SqlServerManagementClient"
        };

    public static ErrorOmd DatabaseProviderIsNone =>
        new() { Code = nameof(DatabaseProviderIsNone), Name = "Database Provider is None" };

    public static ErrorOmd DatabaseConnectionNameIsNotSpecified =>
        new() { Code = nameof(DatabaseConnectionNameIsNotSpecified), Name = "databaseConnectionName is not specified" };

    public static ErrorOmd DevDatabaseNameIsNotSpecified =>
        new() { Code = nameof(DevDatabaseNameIsNotSpecified), Name = "dev DatabaseName is not specified" };

    //public static ErrorOmd DevDatabaseRecoveryModelIsNotSpecified =>
    //    new()
    //    {
    //        Code = nameof(DevDatabaseRecoveryModelIsNotSpecified),
    //        Name = "dev DatabaseRecoveryModel is not specified"
    //    };

    public static ErrorOmd CreateSqLiteDatabaseManagerIsNotImplemented =>
        new()
        {
            Code = nameof(CreateSqLiteDatabaseManagerIsNotImplemented),
            Name = "CreateSqLiteDatabaseManager Is Not Implemented"
        };

    public static ErrorOmd CreateOleDatabaseManagerIsNotImplemented =>
        new()
        {
            Code = nameof(CreateOleDatabaseManagerIsNotImplemented),
            Name = "CreateOleDatabaseManager Is Not Implemented"
        };

    public static ErrorOmd ApiClientNameIsNotSpecifiedCannotCreateDatabaseApiClient =>
        new()
        {
            Code = nameof(ApiClientNameIsNotSpecifiedCannotCreateDatabaseApiClient),
            Name = "apiClientName is not specified, cannot create DatabaseApiClient"
        };

    public static ErrorOmd ApiClientSettingsIsNull =>
        new()
        {
            Code = nameof(ApiClientSettingsIsNull),
            Name = "apiClientSettings is null, cannot create DatabaseApiClient"
        };

    public static ErrorOmd ServerIsNotSpecifiedInApiClientSettings =>
        new()
        {
            Code = nameof(ServerIsNotSpecifiedInApiClientSettings),
            Name = "Server is not specified in apiClientSettings"
        };

    public static ErrorOmd CancellationRequested(string methodName)
    {
        return new ErrorOmd { Code = nameof(CancellationRequested), Name = $"Cancellation Requested in {methodName}" };
    }
}
