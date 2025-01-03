using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using DbTools;

namespace OleDbTools;

public sealed class OleDbKit : DbKit
{
#pragma warning disable CA1416
    public override DbConnectionStringBuilder GetConnectionStringBuilder()
    {
        return new OleDbConnectionStringBuilder();
    }

    public override DbConnection GetConnection(bool fireInfoMessageEventOnUserErrors = false)
    {
        // ReSharper disable once DisposableConstructor
        return new OleDbConnection();
    }

    public override DbCommand GetCommand()
    {
        // ReSharper disable once DisposableConstructor
        return new OleDbCommand();
    }

    public override IDbDataParameter GetParameter(DataParameter param)
    {
        var dbPar = new OleDbParameter();
        Convert(param, dbPar);
        return dbPar;
    }

    public override DbCommandBuilder GetCommandBuilder()
    {
        // ReSharper disable once DisposableConstructor
        return new OleDbCommandBuilder();
    }

    public override DbDataAdapter GetDataAdapter()
    {
        // ReSharper disable once DisposableConstructor
        return new OleDbDataAdapter();
    }
#pragma warning restore CA1416

}