using SystemTools.SharedKernel;

namespace DatabaseTools.MariaDbTools.Errors;

public static class MariaDbClientErrors
{
    public static Error ServerInfoIsNotDetected =>
        Error.Problem(nameof(ServerInfoIsNotDetected), "MariaDB server information is not detected");

    public static Error OperationIsNotSupported(string operationName)
    {
        return Error.Problem(nameof(OperationIsNotSupported), $"{operationName} is not supported for MariaDB");
    }
}
