using SystemToolsShared;

namespace SqlServerDbTools;

public static class SqlDbClientErrors
{
    //public static readonly Err CannotCreateDatabaseConnection = new()
    //    { ErrorCode = nameof(CannotCreateDatabaseConnection), ErrorMessage = "Cannot create Database connection" };

    public static readonly Err InvalidSqlServerProductVersion = new()
        { ErrorCode = nameof(InvalidSqlServerProductVersion), ErrorMessage = "Invalid Sql Server Product Version" };

    public static readonly Err InvalidSqlServerVersionParts = new()
        { ErrorCode = nameof(InvalidSqlServerVersionParts), ErrorMessage = "Invalid Sql Server Version Parts" };

    public static readonly Err ServerStringIsNull = new()
        { ErrorCode = nameof(ServerStringIsNull), ErrorMessage = "Server string is null" };

    public static readonly Err ProductVersionIsNotDetected = new()
        { ErrorCode = nameof(ProductVersionIsNotDetected), ErrorMessage = "Product Version is not detected" };

    public static readonly Err ServerInstanceNameIsNotDetected = new()
        { ErrorCode = nameof(ServerInstanceNameIsNotDetected), ErrorMessage = "Server Instance Name is not detected" };

    public static readonly Err ClientNetAddressIsNotDetected = new()
        { ErrorCode = nameof(ClientNetAddressIsNotDetected), ErrorMessage = "Client Net Address is not detected" };

    public static readonly Err ServerNameIsNotDetected = new()
        { ErrorCode = nameof(ServerNameIsNotDetected), ErrorMessage = "Server name is not detected" };

    public static readonly Err GetRemoteOriginUrlError = new()
        { ErrorCode = nameof(GetRemoteOriginUrlError), ErrorMessage = "Error when detecting Remote Origin Url" };

    public static readonly Err NeedCommitError = new()
        { ErrorCode = nameof(NeedCommitError), ErrorMessage = "Error when detecting Need Commit" };



    public static Err ErrorWriteRegData(string parameterName, string newValue) => new()
        { ErrorCode = nameof(ErrorWriteRegData), ErrorMessage = $"Error Write Reg Data {parameterName} => {newValue}" };

}