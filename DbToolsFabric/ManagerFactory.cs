using DbTools;
using Microsoft.Data.Sqlite;
using SqLiteDbTools;
using SqlServerDbTools;
using System;
using System.Data.Common;
using System.Data.SqlClient;

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
        return Enum.TryParse(providerName, out EDataProvider providerType) ? GetKit(providerType) : null;
    }


    public static DbKit GetKit(EDataProvider providerType)
    {
        return providerType switch
        {
            EDataProvider.Sql => new SqlKit(),
            EDataProvider.SqLite => new SqLiteDbKit(),
            _ => throw new Exception("Unknown DataProvider")
        };
    }
}