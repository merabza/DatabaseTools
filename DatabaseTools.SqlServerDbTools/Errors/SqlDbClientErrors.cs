using SystemTools.SystemToolsShared.Errors;

namespace DatabaseTools.SqlServerDbTools.Errors;

public static class SqlDbClientErrors
{
    //public static readonly Error CannotCreateDatabaseConnection = new()
    //    { ErrorCode = nameof(CannotCreateDatabaseConnection), ErrorMessage = "Cannot create Database connection" };

    public static readonly Error InvalidSqlServerProductVersion = new()
    {
        Code = nameof(InvalidSqlServerProductVersion), Name = "Invalid Sql Server Product Version"
    };

    public static readonly Error InvalidSqlServerVersionParts = new()
    {
        Code = nameof(InvalidSqlServerVersionParts), Name = "Invalid Sql Server Version Parts"
    };

    public static readonly Error ServerStringIsNull = new()
    {
        Code = nameof(ServerStringIsNull), Name = "Server string is null"
    };

    public static readonly Error ProductVersionIsNotDetected = new()
    {
        Code = nameof(ProductVersionIsNotDetected), Name = "Product Version is not detected"
    };

    public static readonly Error ServerInstanceNameIsNotDetected = new()
    {
        Code = nameof(ServerInstanceNameIsNotDetected), Name = "Server Instance Name is not detected"
    };

    public static readonly Error ClientNetAddressIsNotDetected = new()
    {
        Code = nameof(ClientNetAddressIsNotDetected), Name = "Client Net Address is not detected"
    };

    public static readonly Error ServerNameIsNotDetected = new()
    {
        Code = nameof(ServerNameIsNotDetected), Name = "Server name is not detected"
    };

    public static readonly Error GetRemoteOriginUrlError = new()
    {
        Code = nameof(GetRemoteOriginUrlError), Name = "Error when detecting Remote Origin Url"
    };

    public static readonly Error NeedCommitError = new()
    {
        Code = nameof(NeedCommitError), Name = "Error when detecting Need Commit"
    };

    public static Error ErrorWriteRegData(string parameterName, string newValue)
    {
        return new Error
        {
            Code = nameof(ErrorWriteRegData), Name = $"Error Write Reg Data {parameterName} => {newValue}"
        };
    }
}
