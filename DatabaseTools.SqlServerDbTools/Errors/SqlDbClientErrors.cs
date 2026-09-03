using SystemTools.SharedKernel;

namespace DatabaseTools.SqlServerDbTools.Errors;

public static class SqlDbClientErrors
{
    //public static  Error CannotCreateDatabaseConnection => Error.Problem()
    //    { Code = nameof(CannotCreateDatabaseConnection), "Cannot create Database connection" };

    public static Error InvalidSqlServerProductVersion =>
        Error.Problem(nameof(InvalidSqlServerProductVersion), "Invalid Sql Server Product Version");

    public static Error InvalidSqlServerVersionParts =>
        Error.Problem(nameof(InvalidSqlServerVersionParts), "Invalid Sql Server Version Parts");

    public static Error SqlServerRegistryValueIsEmpty =>
        Error.NotFound(nameof(SqlServerRegistryValueIsEmpty), "Sql Server Registry value is empty");

    public static Error ServerStringIsNull => Error.Problem(nameof(ServerStringIsNull), "Server string is null");

    public static Error ProductVersionIsNotDetected =>
        Error.Problem(nameof(ProductVersionIsNotDetected), "Product Version is not detected");

    public static Error ServerInstanceNameIsNotDetected =>
        Error.Problem(nameof(ServerInstanceNameIsNotDetected), "Server Instance Name is not detected");

    public static Error ClientNetAddressIsNotDetected =>
        Error.Problem(nameof(ClientNetAddressIsNotDetected), "Client Net Address is not detected");

    public static Error ServerNameIsNotDetected =>
        Error.Problem(nameof(ServerNameIsNotDetected), "Server name is not detected");

    public static Error GetRemoteOriginUrlError =>
        Error.Problem(nameof(GetRemoteOriginUrlError), "Error when detecting Remote Origin Url");

    public static Error NeedCommitError => Error.Problem(nameof(NeedCommitError), "Error when detecting Need Commit");

    public static Error ErrorWriteRegData(string parameterName, string newValue)
    {
        return Error.Problem(nameof(ErrorWriteRegData),
            $"Error when writing Registry Data {parameterName} => {newValue}");
    }
}
