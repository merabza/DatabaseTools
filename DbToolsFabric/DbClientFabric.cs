﻿using System.Data.OleDb;
using DbTools;
using DbTools.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SqlServerDbTools;
using SystemToolsShared;

namespace DbToolsFabric;

public static class DbClientFabric
{
    public static DbClient? GetDbClient(ILogger logger, bool useConsole, EDataProvider dataProvider,
        string serverAddress, DbAuthSettingsBase dbAuthSettingsBase, bool trustServerCertificate,
        string? applicationName, string? databaseName = null, IMessagesDataManager? messagesDataManager = null,
        string? userName = null)
    {
        var dbKit = ManagerFactory.GetKit(dataProvider);
        switch (dataProvider)
        {
            case EDataProvider.Sql:
                var conStrBuilder = dbAuthSettingsBase is not DbAuthSettings dbAuthSettings
                    ? new SqlConnectionStringBuilder { IntegratedSecurity = true }
                    : new SqlConnectionStringBuilder
                    {
                        IntegratedSecurity = false,
                        UserID = dbAuthSettings.ServerUser,
                        Password = dbAuthSettings.ServerPass
                    };

                conStrBuilder.DataSource = serverAddress;
                conStrBuilder.ApplicationName = applicationName;
                conStrBuilder.TrustServerCertificate = trustServerCertificate;

                if (databaseName != null)
                    conStrBuilder.InitialCatalog = databaseName;

                return new SqlDbClient(logger, conStrBuilder, dbKit, useConsole, messagesDataManager, userName);
            case EDataProvider.None:
            case EDataProvider.SqLite:
                //სერვერთან კავშირის შექმნა შეიძლება მხოლოდ იმ შემთხვევაში,
                //თუ მონაცემთა ბაზის ფაილის სახელი ცნობილია
                //ასხვანაირად კავშირის შექმნა შეუძლებელია აქსესის ბაზისთვის
                if (databaseName == null)
                    return null;
                var sqliteConStrBuilder = new SqliteConnectionStringBuilder
                {
                    DataSource = databaseName,
                    Password = dbAuthSettingsBase is not DbAuthSettings sqliteDbAuthSettings
                        ? null
                        : sqliteDbAuthSettings.ServerPass
                };
                return new SqLiteDbClient(logger, sqliteConStrBuilder, dbKit, useConsole);
            case EDataProvider.OleDb:
                //სერვერთან კავშირის შექმნა შეიძლება მხოლოდ იმ შემთხვევაში,
                //თუ პროგრამა გაშვებულია ვინდოუსზე და თუ მონაცემთა ბაზის ფაილის სახელი ცნობილია
                //ასხვანაირად კავშირის შექმნა შეუძლებელია აქსესის ბაზისთვის
                if (!SystemStat.IsWindows() || databaseName == null)
                    return null;
#pragma warning disable CA1416
                var msAccessConStrBuilder = new OleDbConnectionStringBuilder
                {
                    DataSource = databaseName, Provider = "Microsoft.ACE.OLEDB.12.0", PersistSecurityInfo = false
                };
                return new OleDbClient(logger, msAccessConStrBuilder, dbKit, useConsole);
#pragma warning restore CA1416
            default:
                return null;
        }
    }
}