using System;
using System.Data;
using System.Data.Common;

namespace DatabaseTools.DbTools;

public /*open*/ class DbKit
{
    public event EventHandler<InfoMessageEventArgs>? InfoMessage;

    public virtual DbConnectionStringBuilder? GetConnectionStringBuilder()
    {
        return null;
    }

    public virtual DbConnection? GetConnection(bool fireInfoMessageEventOnUserErrors = false)
    {
        return null;
    }

    public virtual DbCommand? GetCommand()
    {
        return null;
    }

    public virtual IDbDataParameter? GetParameter(DataParameter param)
    {
        return null;
    }

    public virtual IDbDataParameter? GetParameterByBaseTypeName(string baseTypeName)
    {
        return null;
    }

    public virtual IDbDataParameter? GetGuidListParameter()
    {
        return null;
    }

    public virtual DbCommandBuilder? GetCommandBuilder()
    {
        return null;
    }

    public virtual DbDataAdapter? GetDataAdapter()
    {
        return null;
    }

    protected bool IsInfoMessageUsed()
    {
        return InfoMessage != null;
    }

    protected void RaiseInfoMessageEvent(string message, byte cls)
    {
        InfoMessage?.Invoke(this, new InfoMessageEventArgs(message, cls));
    }

    public void Convert(DataParameter dataParam, DbParameter dbParam)
    {
        dbParam.ParameterName = dataParam.ParameterName;
        dbParam.DbType = dataParam.DbType;
        dbParam.Direction = dataParam.Direction;
        dbParam.Precision = dataParam.Precision;
        dbParam.Scale = dataParam.Scale;
        dbParam.Size = dataParam.Size;
        dbParam.SourceColumn = dataParam.SourceColumn;
        dbParam.SourceVersion = dataParam.SourceVersion;
        dbParam.Value = dataParam.Value;
    }
}
