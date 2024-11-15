using DbTools;
using DbTools.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SqlServerDbTools;
using SystemToolsShared;

namespace DbToolsFabric;

public static class DbClientFabric
{
    public static DbClient? GetDbClient(ILogger logger, bool useConsole, EDataProvider dataProvider,
        string serverAddress, DbAuthSettingsBase dbAuthSettingsBase, string? applicationName,
        string? databaseName = null, IMessagesDataManager? messagesDataManager = null, string? userName = null)
    {
        switch (dataProvider)
        {
            case EDataProvider.Sql:
                var conStrBuilder = dbAuthSettingsBase is not DbAuthSettings dbAuthSettings
                    ? new SqlConnectionStringBuilder { IntegratedSecurity = true }
                    : new SqlConnectionStringBuilder
                    {
                        IntegratedSecurity = false, UserID = dbAuthSettings.ServerUser,
                        Password = dbAuthSettings.ServerPass
                    };

                conStrBuilder.DataSource = serverAddress;
                conStrBuilder.ApplicationName = applicationName;
                conStrBuilder.TrustServerCertificate = true;

                if (databaseName != null)
                    conStrBuilder.InitialCatalog = databaseName;

                var dbKit = ManagerFactory.GetKit(EDataProvider.Sql);
                return new SqlDbClient(logger, conStrBuilder, dbKit, useConsole, messagesDataManager, userName);
            case EDataProvider.None:
            case EDataProvider.SqLite:
            default:
                return null;
        }
    }
}