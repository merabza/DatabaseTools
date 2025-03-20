using System;
using System.Data.Common;
using DbTools;
using LibDatabaseParameters;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using OleDbTools;
using SqLiteDbTools;
using SqlServerDbTools;

namespace DbToolsFabric;

public static class ManagerFactory
{
    internal static DbKit? GetKit(DbConnection connection)
    {
        if (connection is SqlConnection)
            return new SqlKit();
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