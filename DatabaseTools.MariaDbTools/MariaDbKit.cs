using System.Data;
using System.Data.Common;
using DatabaseTools.DbTools;
using MySqlConnector;

namespace DatabaseTools.MariaDbTools;

public sealed class MariaDbKit : DbKit
{
    public override DbConnectionStringBuilder GetConnectionStringBuilder()
    {
        return new MySqlConnectionStringBuilder();
    }

    public override DbConnection GetConnection(bool fireInfoMessageEventOnUserErrors = false)
    {
        // ReSharper disable once DisposableConstructor
        return new MySqlConnection();
    }

    public override DbCommand GetCommand()
    {
        // ReSharper disable once DisposableConstructor
        return new MySqlCommand();
    }

    public override IDbDataParameter GetParameter(DataParameter param)
    {
        var dbPar = new MySqlParameter();
        Convert(param, dbPar);
        return dbPar;
    }

    public override DbCommandBuilder GetCommandBuilder()
    {
        // ReSharper disable once DisposableConstructor
        return new MySqlCommandBuilder();
    }

    public override DbDataAdapter GetDataAdapter()
    {
        // ReSharper disable once DisposableConstructor
        return new MySqlDataAdapter();
    }
}
