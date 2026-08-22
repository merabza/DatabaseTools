using SystemTools.SystemToolsShared.Errors;

namespace DatabaseTools.SqlServerDbTools.Errors;

public static class SqlDbClientErrors
{
    //public static readonly ErrorOmd CannotCreateDatabaseConnection = new()
    //    { Code = nameof(CannotCreateDatabaseConnection), Name = "Cannot create Database connection" };

    public static readonly ErrorOmd InvalidSqlServerProductVersion = new()
    {
        Code = nameof(InvalidSqlServerProductVersion), Name = "Invalid Sql Server Product Version"
    };

    public static readonly ErrorOmd InvalidSqlServerVersionParts = new()
    {
        Code = nameof(InvalidSqlServerVersionParts), Name = "Invalid Sql Server Version Parts"
    };

    public static readonly ErrorOmd ServerStringIsNull = new()
    {
        Code = nameof(ServerStringIsNull), Name = "Server string is null"
    };

    public static readonly ErrorOmd ProductVersionIsNotDetected = new()
    {
        Code = nameof(ProductVersionIsNotDetected), Name = "Product Version is not detected"
    };

    public static readonly ErrorOmd ServerInstanceNameIsNotDetected = new()
    {
        Code = nameof(ServerInstanceNameIsNotDetected), Name = "Server Instance Name is not detected"
    };

    public static readonly ErrorOmd ClientNetAddressIsNotDetected = new()
    {
        Code = nameof(ClientNetAddressIsNotDetected), Name = "Client Net Address is not detected"
    };

    public static readonly ErrorOmd ServerNameIsNotDetected = new()
    {
        Code = nameof(ServerNameIsNotDetected), Name = "Server name is not detected"
    };

    public static readonly ErrorOmd GetRemoteOriginUrlError = new()
    {
        Code = nameof(GetRemoteOriginUrlError), Name = "ErrorOmd when detecting Remote Origin Url"
    };

    public static readonly ErrorOmd NeedCommitError = new()
    {
        Code = nameof(NeedCommitError), Name = "ErrorOmd when detecting Need Commit"
    };

    public static ErrorOmd ErrorWriteRegData(string parameterName, string newValue)
    {
        return new ErrorOmd
        {
            Code = nameof(ErrorWriteRegData), Name = $"ErrorOmd Write Reg Data {parameterName} => {newValue}"
        };
    }
}
