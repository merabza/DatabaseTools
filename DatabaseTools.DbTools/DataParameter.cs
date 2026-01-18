using System.Data;

namespace DatabaseTools.DbTools;

public sealed class DataParameter
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public DataParameter(string parameterName)
    {
        //IsNullable = false;
        Direction = ParameterDirection.Input;
        SourceColumn = string.Empty;
        SourceVersion = DataRowVersion.Current;
        ParameterName = parameterName;
    }

    public DataParameter(object? value, string parameterName) : this(parameterName)
    {
        Value = value;
        ParameterName = parameterName;
    }

    public DataParameter(DbType dbType, string parameterName) : this(parameterName)
    {
        DbType = dbType;
        ParameterName = parameterName;
    }

    public DataParameter(DbType dbType, string parameterName, int size) : this(parameterName)
    {
        DbType = dbType;
        ParameterName = parameterName;
        Size = size;
    }

    public DataParameter(DbType dbType, string parameterName, int size, ParameterDirection direction) : this(
        parameterName)
    {
        DbType = dbType;
        ParameterName = parameterName;
        Size = size;
        Direction = direction;
    }

    public DataParameter(DbType dbType, string parameterName, string sourceColumn) : this(parameterName)
    {
        DbType = dbType;
        ParameterName = parameterName;
        SourceColumn = sourceColumn;
    }

    public DataParameter(DbType dbType, string parameterName, int size, string sourceColumn) : this(parameterName)
    {
        DbType = dbType;
        ParameterName = parameterName;
        Size = size;
        SourceColumn = sourceColumn;
    }

    public DbType DbType { get; }

    public ParameterDirection Direction { get; }

    //private bool IsNullable { get; }
    public string ParameterName { get; set; }
    public byte Precision { get; set; }
    public byte Scale { get; set; }
    public int Size { get; }
    public string SourceColumn { get; }
    public DataRowVersion SourceVersion { get; }
    public object? Value { get; set; }
}