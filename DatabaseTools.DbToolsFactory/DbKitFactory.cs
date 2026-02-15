using System;
using System.Data.Common;
using DatabaseTools.DbTools;
using DatabaseTools.OleDbTools;
using DatabaseTools.SqLiteDbTools;
using DatabaseTools.SqlServerDbTools;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using SystemTools.SystemToolsShared;

namespace DatabaseTools.DbToolsFactory;

public static class DbKitFactory
{
    internal static DbKit? GetKit(DbConnection connection)
    {
        if (connection is SqlConnection)
        {
            return new SqlKit();
        }

        return connection is SqliteConnection ? new SqLiteDbKit() : null;
    }

    public static DbKit? GetKit(string providerName)
    {
        return Enum.TryParse(providerName, out EDatabaseProvider provider) ? GetKit(provider) : null;
    }

    public static DbKit GetKit(EDatabaseProvider provider)
    {
        return provider switch
        {
            EDatabaseProvider.SqlServer => new SqlKit(),
            EDatabaseProvider.SqLite => new SqLiteDbKit(),
            EDatabaseProvider.OleDb => new OleDbKit(),
            _ => throw new Exception("Unknown DataProvider")
        };
    }
}
