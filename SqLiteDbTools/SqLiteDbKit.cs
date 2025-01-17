using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using DbTools;
using Microsoft.Data.Sqlite;

namespace SqLiteDbTools;

public sealed class SqLiteDbKit : DbKit
{
    public override DbConnectionStringBuilder GetConnectionStringBuilder()
    {
        return new SqliteConnectionStringBuilder();
    }

    public override DbConnection GetConnection(bool fireInfoMessageEventOnUserErrors = false)
    {
        // ReSharper disable once DisposableConstructor
        return new SQLiteConnection();
    }

    public override DbCommand GetCommand()
    {
        // ReSharper disable once DisposableConstructor
        return new SQLiteCommand();
    }

    public override IDbDataParameter GetParameter(DataParameter param)
    {
        var dbPar = new SQLiteParameter();
        Convert(param, dbPar);
        return dbPar;
    }

    public override DbCommandBuilder GetCommandBuilder()
    {
        // ReSharper disable once DisposableConstructor
        return new SQLiteCommandBuilder();
    }

    public override DbDataAdapter GetDataAdapter()
    {
        // ReSharper disable once DisposableConstructor
        return new SQLiteDataAdapter();
    }
}