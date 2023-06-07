namespace DbTools;

public sealed class InfoMessageEventArgs
{
    public InfoMessageEventArgs(string message, byte cls)
    {
        Message = message;
        Class = cls;
    }

    public string Message { get; }
    public byte Class { get; }
}