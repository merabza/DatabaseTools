using SystemTools.SystemToolsShared.Errors;

namespace DatabaseTools.DbTools.Errors;

public static class DbToolsErrors
{
    public static Error WindowsNtIntegratedSecurityIsOffAndServerUserDoesNotSpecifiedOrServerPassDoesNotSpecified =>
        new()
        {
            Code =
                nameof(WindowsNtIntegratedSecurityIsOffAndServerUserDoesNotSpecifiedOrServerPassDoesNotSpecified),
            Name =
                "WindowsNtIntegratedSecurity is off and serverUser does not specified or serverPass does not specified"
        };

    public static Error ServerAddressIsEmptyCannotCreateSqlServerManagementClient =>
        new()
        {
            Code = nameof(ServerAddressIsEmptyCannotCreateSqlServerManagementClient),
            Name = "ServerAddress is empty, Cannot create SqlServerManagementClient"
        };

    public static Error DatabaseProviderIsNone =>
        new() { Code = nameof(DatabaseProviderIsNone), Name = "Database Provider is None" };

    public static Error DatabaseConnectionNameIsNotSpecified =>
        new() { Code = nameof(DatabaseConnectionNameIsNotSpecified), Name = "databaseConnectionName is not specified" };

    public static Error DevDatabaseNameIsNotSpecified =>
        new() { Code = nameof(DevDatabaseNameIsNotSpecified), Name = "dev DatabaseName is not specified" };

    //public static Error DevDatabaseRecoveryModelIsNotSpecified =>
    //    new()
    //    {
    //        Code = nameof(DevDatabaseRecoveryModelIsNotSpecified),
    //        Name = "dev DatabaseRecoveryModel is not specified"
    //    };

    public static Error CreateSqLiteDatabaseManagerIsNotImplemented =>
        new()
        {
            Code = nameof(CreateSqLiteDatabaseManagerIsNotImplemented),
            Name = "CreateSqLiteDatabaseManager Is Not Implemented"
        };

    public static Error CreateOleDatabaseManagerIsNotImplemented =>
        new()
        {
            Code = nameof(CreateOleDatabaseManagerIsNotImplemented),
            Name = "CreateOleDatabaseManager Is Not Implemented"
        };

    public static Error ApiClientNameIsNotSpecifiedCannotCreateDatabaseApiClient =>
        new()
        {
            Code = nameof(ApiClientNameIsNotSpecifiedCannotCreateDatabaseApiClient),
            Name = "apiClientName is not specified, cannot create DatabaseApiClient"
        };

    public static Error ApiClientSettingsIsNull =>
        new()
        {
            Code = nameof(ApiClientSettingsIsNull),
            Name = "apiClientSettings is null, cannot create DatabaseApiClient"
        };

    public static Error ServerIsNotSpecifiedInApiClientSettings =>
        new()
        {
            Code = nameof(ServerIsNotSpecifiedInApiClientSettings),
            Name = "Server is not specified in apiClientSettings"
        };

    public static Error CancellationRequested(string methodName)
    {
        return new Error { Code = nameof(CancellationRequested), Name = $"Cancellation Requested in {methodName}" };
    }
}
