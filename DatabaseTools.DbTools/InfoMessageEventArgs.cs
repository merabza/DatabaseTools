namespace DatabaseTools.DbTools;

public sealed class InfoMessageEventArgs
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public InfoMessageEventArgs(string message, byte cls)
    {
        Message = message;
        Class = cls;
    }

    public string Message { get; }
    public byte Class { get; }
}