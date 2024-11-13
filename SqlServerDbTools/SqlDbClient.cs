using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DbTools;
using DbTools.Errors;
using DbTools.Models;
using LanguageExt;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using OneOf;
using SqlServerDbTools.Errors;
using SystemToolsShared;
using SystemToolsShared.Errors;

namespace SqlServerDbTools;

public sealed class SqlDbClient : DbClient
{
    private const string CBackupDirectory = "BackupDirectory";
    private const string CDefaultData = "DefaultData";
    private const string CDefaultLog = "DefaultLog";
    private const string CParameters = "Parameters";
    private readonly ILogger _logger;
    private string? _memoServerInstanceName;
    private string? _memoServerProductVersion;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SqlDbClient(ILogger logger, SqlConnectionStringBuilder conStrBuilder, DbKit dbKit, bool useConsole,
        IMessagesDataManager? messagesDataManager = null, string? userName = null) : base(logger, conStrBuilder, dbKit,
        useConsole, messagesDataManager, userName)
    {
        _logger = logger;
    }

    public override async Task<Option<Err[]>> BackupDatabase(string databaseName, string backupFilename,
        string backupName, EBackupType backupType, bool compression, CancellationToken cancellationToken)
    {
        var buTypeWord = "DATABASE";
        if (backupType == EBackupType.TrLog)
            buTypeWord = "LOG";
        var buDifferentialWord = string.Empty;
        if (backupType == EBackupType.Diff)
            buDifferentialWord = "DIFFERENTIAL, ";

        return await ExecuteCommand($"""
                                     BACKUP {buTypeWord} [{databaseName}]
                                     TO DISK=N'{backupFilename}'
                                     WITH {buDifferentialWord}NOFORMAT, NOINIT, NAME = N'{backupName}', SKIP, REWIND, NOUNLOAD{(compression ? ", COMPRESSION" : string.Empty)}
                                     """, false, false, cancellationToken);
        //STATS = 1 აქ ჯერჯერობით არ ვიყენებთ, რადგან არ გვაქვს უკუკავშირი აწყობილი პროცენტების ჩვენებით
        //თუმცა თუ STATS მითითებული არ აქვს ავტომატურად აკეთებს STATS=10
        //STATS [ = percentage ] Displays a message each time another percentage completes, and is used to gauge progress. If percentage is omitted, SQL Server displays a message after each 10 percent is completed.
    }

    public override async Task<OneOf<string, Err[]>> HostPlatform(CancellationToken cancellationToken)
    {
        const string queryString = "SELECT host_platform FROM sys.dm_os_host_info";
        return await ExecuteScalarAsync<string>(queryString, cancellationToken);
    }

    public override async Task<Option<Err[]>> VerifyBackup(string databaseName, string backupFilename,
        CancellationToken cancellationToken)
    {
        return await ExecuteCommand(
            $"""
             DECLARE @backupSetId as int
             SELECT @backupSetId = position
             FROM msdb..backupset
             WHERE database_name=N'{databaseName}' and backup_set_id=(
               SELECT max(backup_set_id)
               FROM msdb..backupset
               WHERE database_name=N'{databaseName}' )
             IF @backupSetId is null
              BEGIN
               RAISERROR(N'Verify failed. Backup information for database ''{databaseName}'' not found.', 16, 1)
              END
             RESTORE VERIFYONLY FROM DISK = N'{backupFilename}' WITH  FILE = @backupSetId, NOUNLOAD, NOREWIND
             """, false, false, cancellationToken);
        //STATS = 1 აქ ჯერჯერობით არ ვიყენებთ, რადგან არ გვაქვს უკუკავშირი აწყობილი პროცენტების ჩვენებით
    }

    public override async Task<OneOf<bool, Err[]>> IsDatabaseExists(string databaseName,
        CancellationToken cancellationToken)
    {
        const string query = "select count(*) from master.dbo.sysdatabases where name=@database";
        return await GetServerIntBool(query, cancellationToken, databaseName);
    }

    public override async Task<OneOf<List<RestoreFileModel>, Err[]>> GetRestoreFiles(string backupFileFullName,
        CancellationToken cancellationToken)
    {
        // ReSharper disable once using
        using var dbm = GetDbManager();
        if (dbm is null)
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                CancellationToken.None);

        try
        {
            var query = $"RESTORE FILELISTONLY FROM  DISK = N'{backupFileFullName}' WITH  NOUNLOAD,  FILE = 1";
            dbm.Open();
            // ReSharper disable once using
            using var reader = await dbm.ExecuteReaderAsync(query, cancellationToken);
            var fileNames = new List<RestoreFileModel>();
            while (reader.Read())
                fileNames.Add(new RestoreFileModel((string)reader["LogicalName"],
                    (string)reader["Type"]));

            return fileNames;
        }
        catch (Exception ex)
        {
            return await LogErrorAndSendMessageFromException(ex, nameof(GetRestoreFiles), cancellationToken);
        }
        finally
        {
            dbm.Close();
        }
    }

    public override async Task<Option<Err[]>> RestoreDatabase(string databaseName, string backupFileFullName,
        List<RestoreFileModel>? files, string dataFolderName, string dataLogFolderName, string dirSeparator,
        CancellationToken cancellationToken)
    {
        if (files == null)
            return await LogErrorAndSendMessageFromError(DbClientErrors.NoRestoreFileNames, cancellationToken);

        var dataPart = files.SingleOrDefault(s => s.Type == "D");
        if (dataPart == null)
            return await LogErrorAndSendMessageFromError(DbClientErrors.NoDataPart, cancellationToken);

        var logPart = files.SingleOrDefault(s => s.Type == "L");
        if (logPart == null)
            return await LogErrorAndSendMessageFromError(DbClientErrors.NoLogPart, cancellationToken);

        var dataPartFileFullName = $"{dataFolderName.AddNeedLastPart(dirSeparator)}{databaseName}.mdf";
        var dataLogPartFileFullName = $"{dataLogFolderName.AddNeedLastPart(dirSeparator)}{databaseName}_log.ldf";

        return await ExecuteCommand($"""
                                     RESTORE DATABASE [{databaseName}]
                                     FROM  DISK = N'{backupFileFullName}' WITH  FILE = 1,
                                     MOVE N'{dataPart.LogicalName}' TO N'{dataPartFileFullName}',
                                     MOVE N'{logPart.LogicalName}' TO N'{dataLogPartFileFullName}', NOUNLOAD, REPLACE
                                     """, false, false, cancellationToken);
        //STATS = 1 აქ ჯერჯერობით არ ვიყენებთ, რადგან არ გვაქვს უკუკავშირი აწყობილი პროცენტების ჩვენებით
    }

    public override async Task<Option<Err[]>> TestConnection(bool withDatabase, CancellationToken cancellationToken)
    {
        // ReSharper disable once using
        using var dbm = GetDbManager();
        if (dbm is null)
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                CancellationToken.None);

        if (dbm.ConnectionString == string.Empty)
            return await LogErrorAndSendMessageFromError(DbClientErrors.ConnectionServerDoesNotSpecified,
                CancellationToken.None);

        try
        {
            dbm.Open();
            dbm.Close();
            if (dbm.Database == string.Empty && withDatabase)
                return await LogErrorAndSendMessageFromError(DbClientErrors.DatabaseNameIsNotSpecified,
                    CancellationToken.None);

            _logger.LogInformation("Test Connection Succeeded");
            return null;
        }
        catch (Exception ex)
        {
            return new[] { DbClientErrors.ConnectionFailed(ex.Message) };
        }
    }


    //ამ მეთოდმა არ იმუშავა. საჭიროა სერვერის მხარეს გაეშვას ბძანებები
    //ლინუქსუს შემთხვევაში:
    //sudo /opt/mssql/bin/mssql-conf set filelocation.defaultbackupdir /tmp/backup
    //sudo /opt/mssql/bin/mssql-conf set filelocation.defaultdatadir /tmp/data
    //sudo /opt/mssql/bin/mssql-conf set filelocation.defaultlogdir /tmp/log
    private async Task<Option<Err[]>> RegWrite(string sqlServerProductVersion, string instanceName,
        string? subRegFolder, string parameterName, string newValue, CancellationToken cancellationToken)
    {
        var serverVersionParts = sqlServerProductVersion.Split('.');
        if (!int.TryParse(serverVersionParts[0], out var serverVersionNum))
            return new[] { SqlDbClientErrors.InvalidSqlServerProductVersion };
        if (serverVersionParts.Length <= 1)
            return new[] { SqlDbClientErrors.InvalidSqlServerVersionParts };

        // ReSharper disable once using
        using var dbm = GetDbManager();
        if (dbm is null)
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                CancellationToken.None);

        try
        {
            dbm.ClearParameters();
            dbm.Open();
            var query = serverVersionNum > 10
                ? $"""
                   EXEC master.dbo.xp_instance_regwrite
                    N'HKEY_LOCAL_MACHINE',
                    N'Software\Microsoft\MSSQLServer\MSSQLServer{(subRegFolder == null ? string.Empty : $@"\{subRegFolder}")}',
                    '{parameterName}',
                    REG_SZ,
                    N'{newValue}'
                   """
                : $"""
                   EXEC master.dbo.xp_regwrite
                    N'HKEY_LOCAL_MACHINE',
                    N'SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL{serverVersionParts[0]}_{serverVersionParts[1]}.{instanceName}\MSSQLServer{(subRegFolder == null ? string.Empty : $@"\{subRegFolder}")}',
                    N'{parameterName}',
                    REG_SZ,
                    N'{newValue}'
                   """;
            // ReSharper disable once using
            var affectedCount = await dbm.ExecuteNonQueryAsync(query, cancellationToken);

            return affectedCount != 1 ? new[] { SqlDbClientErrors.ErrorWriteRegData(parameterName, newValue) } : null;
        }
        catch (Exception ex)
        {
            return await LogErrorAndSendMessageFromException(ex, nameof(RegWrite), cancellationToken);
        }
        finally
        {
            dbm.Close();
        }
    }

    private async Task<OneOf<string?, Err[]>> RegRead(string sqlServerProductVersion, string instanceName,
        string? subRegFolder, string parameterName, CancellationToken cancellationToken)
    {
        var serverVersionParts = sqlServerProductVersion.Split('.');
        if (!int.TryParse(serverVersionParts[0], out var serverVersionNum))
            return new[] { SqlDbClientErrors.InvalidSqlServerProductVersion };
        if (serverVersionParts.Length <= 1)
            return new[] { SqlDbClientErrors.InvalidSqlServerVersionParts };

        // ReSharper disable once using
        using var dbm = GetDbManager();
        if (dbm is null)
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                CancellationToken.None);

        try
        {
            dbm.ClearParameters();
            dbm.Open();
            var query = serverVersionNum > 10
                ? $@"EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer{(subRegFolder == null ? string.Empty : $@"\{subRegFolder}")}', '{parameterName}'"
                : $@"EXEC master.dbo.xp_regread N'HKEY_LOCAL_MACHINE', N'SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL{serverVersionParts[0]}_{serverVersionParts[1]}.{instanceName}\MSSQLServer{(subRegFolder == null ? string.Empty : $@"\{subRegFolder}")}', N'{parameterName}'";
            // ReSharper disable once using
            using var reader = await dbm.ExecuteReaderAsync(query, cancellationToken);
            if (reader.Read())
                return reader.GetString(1);
            return (string?)null;
        }
        catch (Exception ex)
        {
            return await LogErrorAndSendMessageFromException(ex, nameof(RegRead), cancellationToken);
        }
        finally
        {
            dbm.Close();
        }
    }

    private static string? GetMasterDir(string? masterFileName)
    {
        //პირველი 2 სიმბოლო ზედმეტია
        return masterFileName == null ? null : Path.GetDirectoryName(masterFileName[2..]);
    }

    //თუ სპეციალურად არ არის განსაზღვრული, რომელი ფოლდერი უნდა გამოიყენოს სერვერმა ბაზებისათვის, მაშინ იყენებს მასტერის ადგილმდებარეობას
    private async Task<OneOf<string?, Err[]>> DoubleRegRead(string serverProductVersion, string serverInstanceName,
        string parameterName, string subRegFolder, string subParameterName, CancellationToken cancellationToken)
    {
        var regReadDefaultDataResult = await RegRead(serverProductVersion, serverInstanceName, null, parameterName,
            cancellationToken);
        if (regReadDefaultDataResult.IsT1)
            return regReadDefaultDataResult.AsT1;
        var defaultDataDirectory = regReadDefaultDataResult.AsT0;

        if (defaultDataDirectory is not null)
            return defaultDataDirectory;

        var regReadParametersResult0 = await RegRead(serverProductVersion, serverInstanceName, subRegFolder,
            subParameterName, cancellationToken);
        if (regReadParametersResult0.IsT1)
            return regReadParametersResult0.AsT1;

        return GetMasterDir(regReadParametersResult0.AsT0);
    }

    public override async Task<OneOf<DbServerInfo, Err[]>> GetDbServerInfo(CancellationToken cancellationToken)
    {
        var serverProductVersionResult = await GetServerProductVersion(cancellationToken);
        if (serverProductVersionResult.IsT1)
            return serverProductVersionResult.AsT1;
        var serverProductVersion = serverProductVersionResult.AsT0;
        var serverInstanceNameResult = await GetServerInstanceName(cancellationToken);
        if (serverInstanceNameResult.IsT1)
            return serverInstanceNameResult.AsT1;
        var serverInstanceName = serverInstanceNameResult.AsT0;
        var regReadBackupDirectoryResult = await RegRead(serverProductVersion, serverInstanceName, null,
            CBackupDirectory, cancellationToken);
        if (regReadBackupDirectoryResult.IsT1)
            return regReadBackupDirectoryResult.AsT1;
        var backupDirectory = regReadBackupDirectoryResult.AsT0;

        //თუ სპეციალურად არ არის განსაზღვრული, რომელი ფოლდერი უნდა გამოიყენოს სერვერმა ბაზებისათვის, მაშინ იყენებს მასტერის ადგილმდებარეობას
        var regReadDefaultDataResult = await DoubleRegRead(serverProductVersion, serverInstanceName, CDefaultData,
            CParameters, "SqlArg0", cancellationToken);
        if (regReadDefaultDataResult.IsT1)
            return regReadDefaultDataResult.AsT1;
        var defaultDataDirectory = regReadDefaultDataResult.AsT0;

        var regReadDefaultLogResult = await DoubleRegRead(serverProductVersion, serverInstanceName, CDefaultLog,
            CParameters, "SqlArg1", cancellationToken);
        if (regReadDefaultLogResult.IsT1)
            return regReadDefaultLogResult.AsT1;
        var defaultLogDirectory = regReadDefaultLogResult.AsT0;

        var isServerAllowsCompressionResult = await IsServerAllowsCompression(cancellationToken);
        if (isServerAllowsCompressionResult.IsT1)
            return isServerAllowsCompressionResult.AsT1;
        var isServerAllowsCompression = isServerAllowsCompressionResult.AsT0;

        var serverNameResult = await ServerName(cancellationToken);
        if (serverNameResult.IsT1)
            return serverNameResult.AsT1;
        var serverName = serverNameResult.AsT0;

        return new DbServerInfo(serverProductVersion, serverInstanceName, backupDirectory, defaultDataDirectory,
            defaultLogDirectory, isServerAllowsCompression, serverName);
    }

    private async Task<OneOf<string, Err[]>> GetServerString(string query, CancellationToken cancellationToken,
        string? defString = null)
    {
        // ReSharper disable once using
        using var dbm = GetDbManager();
        if (dbm is null)
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                CancellationToken.None);

        try
        {
            dbm.ClearParameters();
            dbm.Open();
            var executeScalarAsyncResult = await dbm.ExecuteScalarAsync<string>(query, cancellationToken) ?? defString;
            if (executeScalarAsyncResult is null)
                return new[] { SqlDbClientErrors.ServerStringIsNull };
            _memoServerProductVersion = executeScalarAsyncResult;
            return _memoServerProductVersion;
        }
        catch (Exception ex)
        {
            return await LogErrorAndSendMessageFromException(ex, nameof(GetServerString), cancellationToken);
        }
        finally
        {
            dbm.Close();
        }
    }

    private async Task<OneOf<string, Err[]>> GetServerProductVersion(CancellationToken cancellationToken)
    {
        if (_memoServerProductVersion != null)
            return _memoServerProductVersion;

        const string query = "SELECT SERVERPROPERTY('productversion')";
        var getServerStringResult = await GetServerString(query, cancellationToken);
        if (getServerStringResult.IsT1)
            return Err.RecreateErrors(getServerStringResult.AsT1, SqlDbClientErrors.ProductVersionIsNotDetected);
        _memoServerProductVersion = getServerStringResult.AsT0;
        return _memoServerProductVersion;
    }

    private async Task<OneOf<string, Err[]>> GetServerInstanceName(CancellationToken cancellationToken)
    {
        if (_memoServerInstanceName != null)
            return _memoServerInstanceName;

        //const string query = "SELECT SERVERPROPERTY('InstanceName')";
        const string query = "SELECT @@servicename";
        var getServerStringResult = await GetServerString(query, cancellationToken);
        if (getServerStringResult.IsT1)
            return Err.RecreateErrors(getServerStringResult.AsT1, SqlDbClientErrors.ServerInstanceNameIsNotDetected);
        _memoServerInstanceName = getServerStringResult.AsT0;
        return _memoServerInstanceName;
    }

    public override async Task<OneOf<List<DatabaseInfoModel>, Err[]>> GetDatabaseInfos(
        CancellationToken cancellationToken)
    {
        // ReSharper disable once using
        using var dbm = GetDbManager();
        if (dbm is null)
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                CancellationToken.None);

        try
        {
            dbm.Open();
            const string query = """
                                 SELECT database_id as dbId, name as dbName, recovery_model as recoveryModel,
                                   (CASE WHEN name IN ('master', 'model', 'msdb') THEN 1 ELSE is_distributor END) as isSystemDatabase,
                                   0 as dbChecked
                                 FROM sys.databases
                                 WHERE name <> 'tempdb'
                                 """;
            var dbNames = new List<DatabaseInfoModel>();
            // ReSharper disable once using
            using var reader = await dbm.ExecuteReaderAsync(query, cancellationToken);
            while (reader.Read())
                dbNames.Add(new DatabaseInfoModel(reader.GetString(1),
                    (EDatabaseRecovery)reader.GetByte(2),
                    reader.GetInt32(3) != 0));
            return dbNames;
        }
        catch (Exception ex)
        {
            return await LogErrorAndSendMessageFromException(ex, nameof(GetDatabaseInfos), cancellationToken);
        }
        finally
        {
            dbm.Close();
        }
    }

    private async Task<OneOf<bool, Err[]>> GetServerIntBool(string query, CancellationToken cancellationToken,
        string? databaseName = null)
    {
        // ReSharper disable once using
        using var dbm = GetDbManager();
        if (dbm is null)
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                CancellationToken.None);

        try
        {
            if (databaseName is not null)
                dbm.AddParameter("@database", databaseName);
            dbm.Open();
            return await dbm.ExecuteScalarAsync<int>(query, cancellationToken) == 1;
        }
        catch (Exception ex)
        {
            return await LogErrorAndSendMessageFromException(ex, nameof(GetServerIntBool), cancellationToken);
        }
        finally
        {
            dbm.Close();
        }
    }

    public override async Task<OneOf<bool, Err[]>> IsServerAllowsCompression(CancellationToken cancellationToken)
    {
        const string query = """
                             SELECT count(value)
                             FROM sys.configurations
                             WHERE name = 'backup compression default' AND maximum > 0
                             """;
        return await GetServerIntBool(query, cancellationToken);
    }

    public override async Task<OneOf<bool, Err[]>> IsServerLocal(CancellationToken cancellationToken)
    {
        const string queryString = "SELECT CONNECTIONPROPERTY('client_net_address') AS client_net_address";
        var getServerStringResult = await GetServerString(queryString, cancellationToken);
        if (getServerStringResult.IsT1)
            return Err.RecreateErrors(getServerStringResult.AsT1, SqlDbClientErrors.ClientNetAddressIsNotDetected);
        var clientNetAddress = getServerStringResult.AsT0;
        return clientNetAddress is "<local machine>" or "127.0.0.1";
    }

    public override async Task<Option<Err[]>> CheckRepairDatabase(string databaseName,
        CancellationToken cancellationToken)
    {
        var strCommand = $"DBCC CHECKDB(N'{databaseName}') WITH NO_INFOMSGS";
        return await ExecuteCommand(strCommand, true, false, cancellationToken);
    }

    private async Task<OneOf<List<Tuple<string, string>>, Err[]>> GetStoredProcedureNames(
        CancellationToken cancellationToken)

    {
        // ReSharper disable once using
        using var dbm = GetDbManager();
        if (dbm is null)
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                CancellationToken.None);

        try
        {
            dbm.Open();
            const string query = "exec sp_stored_procedures";

            // ReSharper disable once using
            using var reader = await dbm.ExecuteReaderAsync(query, cancellationToken);
            var storedProcedures = new List<Tuple<string, string>>();
            while (reader.Read())
                storedProcedures.Add(new Tuple<string, string>(reader.GetString(1), reader.GetString(2)));
            return storedProcedures;
        }
        catch (Exception ex)
        {
            return await LogErrorAndSendMessageFromException(ex, nameof(GetStoredProcedureNames), cancellationToken);
        }
        finally
        {
            dbm.Close();
        }
    }

    private async Task<OneOf<List<string>, Err[]>> GetTriggerNames(CancellationToken cancellationToken)
    {
        var triggers = new List<string>();

        // ReSharper disable once using
        using var dbm = GetDbManager();
        if (dbm is null)
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                CancellationToken.None);

        try
        {
            dbm.Open();
            const string query = "SELECT name FROM sys.triggers WHERE type = 'TR'";
            // ReSharper disable once using
            using var reader = await dbm.ExecuteReaderAsync(query, cancellationToken);
            while (reader.Read())
                triggers.Add(reader.GetString(0));
        }
        catch (Exception ex)
        {
            return await LogErrorAndSendMessageFromException(ex, nameof(GetTriggerNames), cancellationToken);
        }
        finally
        {
            dbm.Close();
        }

        return triggers;
    }

    private async Task<OneOf<List<string>, Err[]>> GetDatabaseTableNames(CancellationToken cancellationToken)
    {
        // ReSharper disable once using
        using var dbm = GetDbManager();
        if (dbm is null)
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                CancellationToken.None);

        try
        {
            dbm.Open();

            const string query = """
                                 SELECT o.name AS TableName
                                 FROM dbo.sysobjects o
                                   INNER JOIN dbo.sysindexes i ON o.id = i.id
                                 WHERE (OBJECTPROPERTY(o.id, N'IsTable') = 1)
                                   AND (i.indid < 2)
                                   AND (o.name NOT LIKE N'#%')
                                   AND (OBJECTPROPERTY(o.id, N'tableisfake') <> 1)
                                   AND USER_NAME(o.uid) <> 'sys'
                                 ORDER BY TableName
                                 """;

            // ReSharper disable once using
            using var reader = await dbm.ExecuteReaderAsync(query, cancellationToken);
            var tableNames = new List<string>();
            while (reader.Read())
                tableNames.Add(reader.GetString(0));
            return tableNames;
        }
        catch (Exception ex)
        {
            return await LogErrorAndSendMessageFromException(ex, nameof(GetDatabaseTableNames), cancellationToken);
        }
        finally
        {
            dbm.Close();
        }
    }

    private async Task<Option<Err[]>> RecompileDatabaseObject(string strObjectName, CancellationToken cancellationToken)
    {
        return await ExecuteCommand($"EXEC sp_recompile [{strObjectName}]", true, false, cancellationToken);
    }

    private async Task<Option<Err[]>> UpdateStatisticsForOneTable(string strTableName,
        CancellationToken cancellationToken)
    {
        return await ExecuteCommand($"UPDATE STATISTICS [{strTableName}] WITH FULLSCAN", true, false,
            cancellationToken);
    }


    public override async Task<Option<Err[]>> RecompileProcedures(string databaseName,
        CancellationToken cancellationToken)
    {
        await LogInfoAndSendMessage("Recompiling Tables, views and triggers for database {0}...",
            databaseName, cancellationToken);

        if (cancellationToken.IsCancellationRequested)
            return new[] { DbToolsErrors.CancellationRequested(nameof(RecompileProcedures)) };

        var serverName = await ServerName(cancellationToken);

        await LogInfoAndSendMessage("{0}_{1} Recompiling Stored Procedures...", serverName, databaseName,
            cancellationToken);

        var getStoredProcedureNamesResult = await GetStoredProcedureNames(cancellationToken);
        if (getStoredProcedureNamesResult.IsT1)
            return getStoredProcedureNamesResult.AsT1;
        var storedProcedureNames = getStoredProcedureNamesResult.AsT0;
        var procNames = storedProcedureNames.Where(w => w.Item1 != "sys" && !w.Item2.StartsWith("dt_"))
            .Select(s => s.Item2).ToArray();

        foreach (var strCurProcName in procNames)
        {
            if (cancellationToken.IsCancellationRequested)
                return new[] { DbToolsErrors.CancellationRequested(nameof(RecompileProcedures)) };

            char[] separators = [';'];
            var splitWords = strCurProcName.Split(separators);
            var strProcName = splitWords[0];
            try
            {
                if (cancellationToken.IsCancellationRequested)
                    return new[] { DbToolsErrors.CancellationRequested(nameof(RecompileProcedures)) };
                var recompileDatabaseObjectResult = await RecompileDatabaseObject(strProcName, cancellationToken);
                if (recompileDatabaseObjectResult.IsSome)
                    return (Err[])recompileDatabaseObjectResult;
            }
            catch (Exception ex)
            {
                StShared.WriteException(ex, $"{serverName}_{databaseName} Error in Recompile Stored Procedures",
                    UseConsole, _logger);
            }
        }

        await LogInfoAndSendMessage("{0}_{1} Recompiling Triggers...", serverName, databaseName, cancellationToken);

        var getTriggerNames = await GetTriggerNames(cancellationToken);
        if (getTriggerNames.IsT1)
            return getTriggerNames.AsT1;
        var triggerNames = getTriggerNames.AsT0;

        foreach (var strTriggerName in triggerNames)
        {
            if (cancellationToken.IsCancellationRequested)
                return new[] { DbToolsErrors.CancellationRequested(nameof(RecompileProcedures)) };

            try
            {
                var recompileDatabaseObjectResult = await RecompileDatabaseObject(strTriggerName, cancellationToken);
                if (recompileDatabaseObjectResult.IsSome)
                    return (Err[])recompileDatabaseObjectResult;
            }
            catch (Exception ex)
            {
                StShared.WriteException(ex, $"{serverName}_{databaseName} Error in Recompile trigger", UseConsole,
                    _logger);
            }
        }

        return null;
    }


    public override async Task<Option<Err[]>> UpdateStatistics(string databaseName, CancellationToken cancellationToken)
    {
        var serverName = await ServerName(cancellationToken);

        await LogInfoAndSendMessage("Update Statistics for database {0}_{1}...", serverName, databaseName,
            cancellationToken);

        if (cancellationToken.IsCancellationRequested)
            return new[] { DbToolsErrors.CancellationRequested(nameof(UpdateStatistics)) };
        //დადგინდეს მიმდინარე პერიოდისათვის შესრულდა თუ არა უკვე ეს პროცედურა. 
        //ამისათვის, საჭიროა ვიპოვოთ წინა პროცედურის დასრულების აღსანიშნავი ფაილი
        //და დავადგინოთ მისი შესრულების თარიღი.
        //თუ ეს თარიღი მიმდინარე პერიოდშია, მაშინ პროცედურა აღარ უნდა შესრულდეს
        try
        {
            var getDatabaseTableNamesResult = await GetDatabaseTableNames(cancellationToken);
            if (getDatabaseTableNamesResult.IsT1)
                return getDatabaseTableNamesResult.AsT1;
            var tableNames = getDatabaseTableNamesResult.AsT0;
            foreach (var strTableName in tableNames)
            {
                if (cancellationToken.IsCancellationRequested)
                    return new[] { DbToolsErrors.CancellationRequested(nameof(UpdateStatistics)) };
                var updateStatisticsForOneTableResult =
                    await UpdateStatisticsForOneTable(strTableName, cancellationToken);
                if (updateStatisticsForOneTableResult.IsSome)
                    return (Err[])updateStatisticsForOneTableResult;
            }
        }
        catch (Exception ex)
        {
            return await LogErrorAndSendMessageFromException(ex, nameof(UpdateStatistics), cancellationToken);
        }

        return null;
    }

    public override async Task<Option<Err[]>> SetDefaultFolders(string defBackupFolder, string defDataFolder,
        string defLogFolder, CancellationToken cancellationToken)
    {
        var serverProductVersionResult = await GetServerProductVersion(cancellationToken);
        if (serverProductVersionResult.IsT1)
            return serverProductVersionResult.AsT1;
        var serverProductVersion = serverProductVersionResult.AsT0;
        var serverInstanceNameResult = await GetServerInstanceName(cancellationToken);
        if (serverInstanceNameResult.IsT1)
            return serverInstanceNameResult.AsT1;
        var serverInstanceName = serverInstanceNameResult.AsT0;

        var regWriteResult = await RegWrite(serverProductVersion, serverInstanceName, null, CBackupDirectory,
            defBackupFolder, cancellationToken);
        if (regWriteResult.IsSome)
            return (Err[])regWriteResult;

        var regWriteDataResult = await RegWrite(serverProductVersion, serverInstanceName, null, CDefaultData,
            defDataFolder, cancellationToken);
        if (regWriteDataResult.IsSome)
            return (Err[])regWriteDataResult;

        var regWriteLogResult = await RegWrite(serverProductVersion, serverInstanceName, null, CDefaultLog,
            defLogFolder, cancellationToken);
        if (regWriteLogResult.IsSome)
            return (Err[])regWriteLogResult;

        return null;
    }


    private async Task<OneOf<string, Err[]>> ServerName(CancellationToken cancellationToken)
    {
        const string query = "SELECT @@servername";
        var getServerStringResult = await GetServerString(query, cancellationToken);
        if (getServerStringResult.IsT1)
            return Err.RecreateErrors(getServerStringResult.AsT1, SqlDbClientErrors.ServerNameIsNotDetected);
        return getServerStringResult.AsT0;
    }
}