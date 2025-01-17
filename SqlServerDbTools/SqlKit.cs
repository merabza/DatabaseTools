using DbTools;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace SqlServerDbTools;

public sealed class SqlKit : DbKit
{
    public override DbConnectionStringBuilder GetConnectionStringBuilder()
    {
        return new SqlConnectionStringBuilder();
    }

    public override DbConnection GetConnection(bool fireInfoMessageEventOnUserErrors = false)
    {
        // ReSharper disable once using
        var sqlConnection = new SqlConnection
        { FireInfoMessageEventOnUserErrors = fireInfoMessageEventOnUserErrors };
        if (fireInfoMessageEventOnUserErrors)
            sqlConnection.InfoMessage += sqlConnection_InfoMessage;
        return sqlConnection;
    }

    private void sqlConnection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
    {
        if (!IsInfoMessageUsed())
            return;
        foreach (SqlError info in e.Errors)
            RaiseInfoMessageEvent(info.Message, info.Class);
    }

    public override DbCommand GetCommand()
    {
        // ReSharper disable once DisposableConstructor
        return new SqlCommand();
    }

    public override IDbDataParameter GetParameter(DataParameter param)
    {
        var dbPar = new SqlParameter();
        Convert(param, dbPar);
        return dbPar;
    }


    public override IDbDataParameter GetGuidListParameter()
    {
        return new SqlParameter { SqlDbType = SqlDbType.Structured, TypeName = "uniqueidentifier_list_tbltype" };
    }

    //public override DbCommandBuilder GetCommandBuilder()
    //{
    //    return new SqlCommandBuilder();
    //}

    //public override DbDataAdapter GetDataAdapter()
    //{
    //    return new SqlDataAdapter();
    //}
}