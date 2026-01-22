using SystemTools.SystemToolsShared.Errors;

namespace DatabaseTools.DbTools.Errors;

public static class DbToolsErrors
{
    public static Err WindowsNtIntegratedSecurityIsOffAndServerUserDoesNotSpecifiedOrServerPassDoesNotSpecified =>
        new()
        {
            ErrorCode =
                nameof(WindowsNtIntegratedSecurityIsOffAndServerUserDoesNotSpecifiedOrServerPassDoesNotSpecified),
            ErrorMessage =
                "WindowsNtIntegratedSecurity is off and serverUser does not specified or serverPass does not specified"
        };

    public static Err ServerAddressIsEmptyCannotCreateSqlServerManagementClient =>
        new()
        {
            ErrorCode = nameof(ServerAddressIsEmptyCannotCreateSqlServerManagementClient),
            ErrorMessage = "ServerAddress is empty, Cannot create SqlServerManagementClient"
        };

    public static Err DatabaseProviderIsNone =>
        new() { ErrorCode = nameof(DatabaseProviderIsNone), ErrorMessage = "Database Provider is None" };

    public static Err DatabaseConnectionNameIsNotSpecified =>
        new()
        {
            ErrorCode = nameof(DatabaseConnectionNameIsNotSpecified),
            ErrorMessage = "databaseConnectionName is not specified"
        };

    public static Err DevDatabaseNameIsNotSpecified =>
        new() { ErrorCode = nameof(DevDatabaseNameIsNotSpecified), ErrorMessage = "dev DatabaseName is not specified" };

    //public static Err DevDatabaseRecoveryModelIsNotSpecified =>
    //    new()
    //    {
    //        ErrorCode = nameof(DevDatabaseRecoveryModelIsNotSpecified),
    //        ErrorMessage = "dev DatabaseRecoveryModel is not specified"
    //    };

    public static Err CreateSqLiteDatabaseManagerIsNotImplemented =>
        new()
        {
            ErrorCode = nameof(CreateSqLiteDatabaseManagerIsNotImplemented),
            ErrorMessage = "CreateSqLiteDatabaseManager Is Not Implemented"
        };

    public static Err CreateOleDatabaseManagerIsNotImplemented =>
        new()
        {
            ErrorCode = nameof(CreateOleDatabaseManagerIsNotImplemented),
            ErrorMessage = "CreateOleDatabaseManager Is Not Implemented"
        };

    public static Err ApiClientNameIsNotSpecifiedCannotCreateDatabaseApiClient =>
        new()
        {
            ErrorCode = nameof(ApiClientNameIsNotSpecifiedCannotCreateDatabaseApiClient),
            ErrorMessage = "apiClientName is not specified, cannot create DatabaseApiClient"
        };

    public static Err ApiClientSettingsIsNull =>
        new()
        {
            ErrorCode = nameof(ApiClientSettingsIsNull),
            ErrorMessage = "apiClientSettings is null, cannot create DatabaseApiClient"
        };

    public static Err ServerIsNotSpecifiedInApiClientSettings =>
        new()
        {
            ErrorCode = nameof(ServerIsNotSpecifiedInApiClientSettings),
            ErrorMessage = "Server is not specified in apiClientSettings"
        };

    public static Err CancellationRequested(string methodName)
    {
        return new Err
        {
            ErrorCode = nameof(CancellationRequested), ErrorMessage = $"Cancellation Requested in {methodName}"
        };
    }
}
