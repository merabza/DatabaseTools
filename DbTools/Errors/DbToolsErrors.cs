using SystemToolsShared.Errors;

namespace DbTools.Errors;

public static class DbToolsErrors
{
    public static Err CancellationRequested(string methodName)
    {
        return new Err
        {
            ErrorCode = nameof(CancellationRequested),
            ErrorMessage = $"Cancellation Requested in {methodName}"
        };
    }
}