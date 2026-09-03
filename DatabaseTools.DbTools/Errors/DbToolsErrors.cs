using SystemTools.SharedKernel;

namespace DatabaseTools.DbTools.Errors;

public static class DbToolsErrors
{
    public static Error WindowsNtIntegratedSecurityIsOffAndServerUserDoesNotSpecifiedOrServerPassDoesNotSpecified =>
        Error.Problem(nameof(WindowsNtIntegratedSecurityIsOffAndServerUserDoesNotSpecifiedOrServerPassDoesNotSpecified),
            "WindowsNtIntegratedSecurity is off and serverUser does not specified or serverPass does not specified");

    public static Error ServerAddressIsEmptyCannotCreateSqlServerManagementClient =>
        Error.Problem(nameof(ServerAddressIsEmptyCannotCreateSqlServerManagementClient),
            "ServerAddress is empty, Cannot create SqlServerManagementClient");

    public static Error DatabaseProviderIsNone =>
        Error.Problem(nameof(DatabaseProviderIsNone), "Database Provider is None");

    public static Error DatabaseConnectionNameIsNotSpecified =>
        Error.Problem(nameof(DatabaseConnectionNameIsNotSpecified), "databaseConnectionName is not specified");

    public static Error DevDatabaseNameIsNotSpecified =>
        Error.Problem(nameof(DevDatabaseNameIsNotSpecified), "dev DatabaseName is not specified");

    //public static ErrorOmd DevDatabaseRecoveryModelIsNotSpecified =>
    //    new()
    //    {
    //        Code = nameof(DevDatabaseRecoveryModelIsNotSpecified),
    //        Name = "dev DatabaseRecoveryModel is not specified"
    //    };

    public static Error CreateSqLiteDatabaseManagerIsNotImplemented =>
        Error.Problem(nameof(CreateSqLiteDatabaseManagerIsNotImplemented),
            "CreateSqLiteDatabaseManager Is Not Implemented");

    public static Error CreateOleDatabaseManagerIsNotImplemented =>
        Error.Problem(nameof(CreateOleDatabaseManagerIsNotImplemented), "CreateOleDatabaseManager Is Not Implemented");

    public static Error ApiClientNameIsNotSpecifiedCannotCreateDatabaseApiClient =>
        Error.Problem(nameof(ApiClientNameIsNotSpecifiedCannotCreateDatabaseApiClient),
            "apiClientName is not specified, cannot create DatabaseApiClient");

    public static Error ApiClientSettingsIsNull =>
        Error.Problem(nameof(ApiClientSettingsIsNull), "apiClientSettings is null, cannot create DatabaseApiClient");

    public static Error ServerIsNotSpecifiedInApiClientSettings =>
        Error.Problem(nameof(ServerIsNotSpecifiedInApiClientSettings), "Server is not specified in apiClientSettings");

    public static Error CancellationRequested(string methodName)
    {
        return Error.Problem(nameof(CancellationRequested), $"Cancellation Requested in {methodName}");
    }
}
