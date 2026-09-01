using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DatabaseTools.DbTools;
using DatabaseTools.DbTools.Errors;
using DatabaseTools.DbTools.Models;
using DatabaseTools.SqlServerDbTools.Errors;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace DatabaseTools.SqlServerDbTools;

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

    public override Task<Result> BackupDatabase(string databaseName, string backupFilename, string backupName,
        EBackupType backupType, bool compression, CancellationToken cancellationToken = default)
    {
        string buTypeWord = "DATABASE";
        if (backupType == EBackupType.TrLog)
        {
            buTypeWord = "LOG";
        }

        string buDifferentialWord = string.Empty;
        if (backupType == EBackupType.Diff)
        {
            buDifferentialWord = "DIFFERENTIAL, ";
        }

        return ExecuteCommand($"""
                               BACKUP {buTypeWord} [{databaseName}]
                               TO DISK=N'{backupFilename}'
                               WITH {buDifferentialWord}NOFORMAT, NOINIT, NAME = N'{backupName}', SKIP, REWIND, NOUNLOAD{(compression ? ", COMPRESSION" : string.Empty)}
                               """, false, false, cancellationToken);
        //STATS = 1 აქ ჯერჯერობით არ ვიყენებთ, რადგან არ გვაქვს უკუკავშირი აწყობილი პროცენტების ჩვენებით
        //თუმცა თუ STATS მითითებული არ აქვს ავტომატურად აკეთებს STATS=10
        //STATS [ = percentage ] Displays a message each time another percentage completes, and is used to gauge progress. If percentage is omitted, SQL Server displays a message after each 10 percent is completed.
    }

    public override Task<Result<string>> HostPlatform(CancellationToken cancellationToken = default)
    {
        const string queryString = "SELECT host_platform FROM sys.dm_os_host_info";
        return ExecuteScalarAsync<string>(queryString, cancellationToken);
    }

    public override Task<Result> VerifyBackup(string databaseName, string backupFilename,
        CancellationToken cancellationToken = default)
    {
        return ExecuteCommand($"""
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

    public override Task<Result<bool>> IsDatabaseExists(string databaseName,
        CancellationToken cancellationToken = default)
    {
        const string query = "select count(*) from master.dbo.sysdatabases where name=@database";
        return GetServerIntBool(query, cancellationToken, databaseName);
    }

    public override async Task<Result<List<RestoreFileModel>>> GetRestoreFiles(string backupFileFullName,
        CancellationToken cancellationToken = default)
    {
        // ReSharper disable once using
        using DbManager? dbm = GetDbManager();
        if (dbm is null)
        {
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                cancellationToken);
        }

        try
        {
            string query = $"RESTORE FILELISTONLY FROM  DISK = N'{backupFileFullName}' WITH  NOUNLOAD,  FILE = 1";
            dbm.Open();
            // ReSharper disable once using
            using IDataReader reader = await dbm.ExecuteReaderAsync(query, CommandType.Text, cancellationToken);
            var fileNames = new List<RestoreFileModel>();
            while (reader.Read())
            {
                fileNames.Add(new RestoreFileModel((string)reader["LogicalName"], (string)reader["Type"]));
            }

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

    public override async Task<Result> RestoreDatabase(string databaseName, string backupFileFullName,
        List<RestoreFileModel>? files, string dataFolderName, string dataLogFolderName, string dirSeparator,
        CancellationToken cancellationToken = default)
    {
        if (files == null)
        {
            return await LogErrorAndSendMessageFromError(DbClientErrors.NoRestoreFileNames, cancellationToken);
        }

        RestoreFileModel? dataPart = files.SingleOrDefault(s => s.Type == "D");
        if (dataPart == null)
        {
            return await LogErrorAndSendMessageFromError(DbClientErrors.NoDataPart, cancellationToken);
        }

        RestoreFileModel? logPart = files.SingleOrDefault(s => s.Type == "L");
        if (logPart == null)
        {
            return await LogErrorAndSendMessageFromError(DbClientErrors.NoLogPart, cancellationToken);
        }

        Result<bool> isDatabaseExistsResult = await IsDatabaseExists(databaseName, cancellationToken);
        if (isDatabaseExistsResult.IsFailure)
        {
            return isDatabaseExistsResult.Error;
        }

        bool databaseExists = isDatabaseExistsResult.Value;

        if (databaseExists)
        {
            //RESTORE-ს ბაზაზე ექსკლუზიური წვდომა სჭირდება, ამიტომ ჯერ არსებული კავშირები უნდა გაწყდეს
            Result setSingleUserResult = await ExecuteCommand(
                $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", true, false,
                cancellationToken);
            if (setSingleUserResult.IsFailure)
            {
                return setSingleUserResult;
            }
        }

        string dataPartFileFullName = $"{dataFolderName.AddNeedLastPart(dirSeparator)}{databaseName}.mdf";
        string dataLogPartFileFullName = $"{dataLogFolderName.AddNeedLastPart(dirSeparator)}{databaseName}_log.ldf";

        Result restoreResult = await ExecuteCommand($"""
                                                     RESTORE DATABASE [{databaseName}]
                                                     FROM  DISK = N'{backupFileFullName}' WITH  FILE = 1,
                                                     MOVE N'{dataPart.LogicalName}' TO N'{dataPartFileFullName}',
                                                     MOVE N'{logPart.LogicalName}' TO N'{dataLogPartFileFullName}', NOUNLOAD, REPLACE
                                                     """, false, false, cancellationToken);
        //STATS = 1 აქ ჯერჯერობით არ ვიყენებთ, რადგან არ გვაქვს უკუკავშირი აწყობილი პროცენტების ჩვენებით

        if (databaseExists)
        {
            //ბაზა MULTI_USER რეჟიმში ბრუნდება აღდგენის შედეგის მიუხედავად, რომ SINGLE_USER-ში ჩაკეტილი არ დარჩეს.
            //შეცდომა იგნორირდება (ExecuteCommand თვითონ ლოგავს); CancellationToken.None — რომ გაუქმების შემდეგაც შესრულდეს
            await ExecuteCommand($"ALTER DATABASE [{databaseName}] SET MULTI_USER", true, false,
                CancellationToken.None);
        }

        return restoreResult;
    }

    public override async Task<Result> TestConnection(bool withDatabase, CancellationToken cancellationToken = default)
    {
        // ReSharper disable once using
        using DbManager? dbm = GetDbManager();
        if (dbm is null)
        {
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                cancellationToken);
        }

        if (string.IsNullOrEmpty(dbm.ConnectionString))
        {
            return await LogErrorAndSendMessageFromError(DbClientErrors.ConnectionServerDoesNotSpecified,
                cancellationToken);
        }

        try
        {
            dbm.Open();
            dbm.Close();
            if (string.IsNullOrEmpty(dbm.Database) && withDatabase)
            {
                return await LogErrorAndSendMessageFromError(DbClientErrors.DatabaseNameIsNotSpecified,
                    cancellationToken);
            }

            _logger.LogInformation("Test Connection Succeeded");
            return Result.Success();
        }
        catch (Exception ex)
        {
            return DbClientErrors.ConnectionFailed(ex.Message);
        }
    }

    //ამ მეთოდმა არ იმუშავა. საჭიროა სერვერის მხარეს გაეშვას ბძანებები
    //ლინუქსუს შემთხვევაში:
    //sudo /opt/mssql/bin/mssql-conf set filelocation.defaultbackupdir /tmp/backup
    //sudo /opt/mssql/bin/mssql-conf set filelocation.defaultdatadir /tmp/data
    //sudo /opt/mssql/bin/mssql-conf set filelocation.defaultlogdir /tmp/log
    private async Task<Result> RegWrite(string sqlServerProductVersion, string instanceName, string? subRegFolder,
        string parameterName, string newValue, CancellationToken cancellationToken = default)
    {
        string[] serverVersionParts = sqlServerProductVersion.Split('.');
        if (!int.TryParse(serverVersionParts[0], out int serverVersionNum))
        {
            return SqlDbClientErrors.InvalidSqlServerProductVersion;
        }

        if (serverVersionParts.Length <= 1)
        {
            return SqlDbClientErrors.InvalidSqlServerVersionParts;
        }

        // ReSharper disable once using
        using DbManager? dbm = GetDbManager();
        if (dbm is null)
        {
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                cancellationToken);
        }

        try
        {
            dbm.ClearParameters();
            dbm.Open();
            string srf = subRegFolder == null ? string.Empty : $@"\{subRegFolder}";
            string query = serverVersionNum > 10
                ? $"""
                   EXEC master.dbo.xp_instance_regwrite
                    N'HKEY_LOCAL_MACHINE',
                    N'Software\Microsoft\MSSQLServer\MSSQLServer{srf}',
                    '{parameterName}',
                    REG_SZ,
                    N'{newValue}'
                   """
                : $"""
                   EXEC master.dbo.xp_regwrite
                    N'HKEY_LOCAL_MACHINE',
                    N'SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL{serverVersionParts[0]}_{serverVersionParts[1]}.{instanceName}\MSSQLServer{srf}',
                    N'{parameterName}',
                    REG_SZ,
                    N'{newValue}'
                   """;
            // ReSharper disable once using
            int affectedCount = await dbm.ExecuteNonQueryAsync(query, CommandType.Text, cancellationToken);

            return affectedCount != 1 ? SqlDbClientErrors.ErrorWriteRegData(parameterName, newValue) : Result.Success();
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

    private async ValueTask<Result<string>> RegRead(string sqlServerProductVersion, string instanceName,
        string? subRegFolder, string parameterName, CancellationToken cancellationToken = default)
    {
        string[] serverVersionParts = sqlServerProductVersion.Split('.');
        if (!int.TryParse(serverVersionParts[0], out int serverVersionNum))
        {
            return SqlDbClientErrors.InvalidSqlServerProductVersion;
        }

        if (serverVersionParts.Length <= 1)
        {
            return SqlDbClientErrors.InvalidSqlServerVersionParts;
        }

        // ReSharper disable once using
        using DbManager? dbm = GetDbManager();
        if (dbm is null)
        {
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                cancellationToken);
        }

        try
        {
            dbm.ClearParameters();
            dbm.Open();
            string srf = subRegFolder == null ? string.Empty : $@"\{subRegFolder}";
            string query = serverVersionNum > 10
                ? $@"EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer{srf}', '{parameterName}'"
                : $@"EXEC master.dbo.xp_regread N'HKEY_LOCAL_MACHINE', N'SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL{serverVersionParts[0]}_{serverVersionParts[1]}.{instanceName}\MSSQLServer{srf}', N'{parameterName}'";
            // ReSharper disable once using
            using IDataReader reader = await dbm.ExecuteReaderAsync(query, CommandType.Text, cancellationToken);
            if (reader.Read())
            {
                return reader.GetString(1);
            }

            return SqlDbClientErrors.SqlServerRegistryValueIsEmpty;
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
    private async Task<Result<string>> DoubleRegRead(string serverProductVersion, string serverInstanceName,
        string parameterName, string subRegFolder, string subParameterName,
        CancellationToken cancellationToken = default)
    {
        Result<string> regReadDefaultDataResult = await RegRead(serverProductVersion, serverInstanceName, null,
            parameterName, cancellationToken);

        if (regReadDefaultDataResult.IsSuccess)
        {
            return regReadDefaultDataResult.Value;
        }

        if (regReadDefaultDataResult.Error.Code != nameof(SqlDbClientErrors.SqlServerRegistryValueIsEmpty))
        {
            return regReadDefaultDataResult;
        }

        Result<string> regReadParametersResult0 = await RegRead(serverProductVersion, serverInstanceName, subRegFolder,
            subParameterName, cancellationToken);
        if (regReadParametersResult0.IsFailure)
        {
            return regReadParametersResult0.Error;
        }

        return GetMasterDir(regReadParametersResult0.Value);
    }

    public override async Task<Result<DbServerInfo>> GetDbServerInfo(CancellationToken cancellationToken = default)
    {
        Result<string> serverProductVersionResult = await GetServerProductVersion(cancellationToken);
        if (serverProductVersionResult.IsFailure)
        {
            return serverProductVersionResult.Error;
        }

        string serverProductVersion = serverProductVersionResult.Value;
        Result<string> serverInstanceNameResult = await GetServerInstanceName(cancellationToken);
        if (serverInstanceNameResult.IsFailure)
        {
            return serverInstanceNameResult.Error;
        }

        string serverInstanceName = serverInstanceNameResult.Value;
        Result<string> regReadBackupDirectoryResult = await RegRead(serverProductVersion, serverInstanceName, null,
            CBackupDirectory, cancellationToken);
        if (regReadBackupDirectoryResult.IsFailure)
        {
            return regReadBackupDirectoryResult.Error;
        }

        string backupDirectory = regReadBackupDirectoryResult.Value;

        //თუ სპეციალურად არ არის განსაზღვრული, რომელი ფოლდერი უნდა გამოიყენოს სერვერმა ბაზებისათვის, მაშინ იყენებს მასტერის ადგილმდებარეობას
        Result<string> regReadDefaultDataResult = await DoubleRegRead(serverProductVersion, serverInstanceName,
            CDefaultData, CParameters, "SqlArg0", cancellationToken);
        if (regReadDefaultDataResult.IsFailure)
        {
            return regReadDefaultDataResult.Error;
        }

        string defaultDataDirectory = regReadDefaultDataResult.Value;

        Result<string> regReadDefaultLogResult = await DoubleRegRead(serverProductVersion, serverInstanceName,
            CDefaultLog, CParameters, "SqlArg1", cancellationToken);
        if (regReadDefaultLogResult.IsFailure)
        {
            return regReadDefaultLogResult.Error;
        }

        string defaultLogDirectory = regReadDefaultLogResult.Value;

        Result<bool> isServerAllowsCompressionResult = await IsServerAllowsCompression(cancellationToken);
        if (isServerAllowsCompressionResult.IsFailure)
        {
            return isServerAllowsCompressionResult.Error;
        }

        bool isServerAllowsCompression = isServerAllowsCompressionResult.Value;

        Result<string> serverNameResult = await ServerName(cancellationToken);
        if (serverNameResult.IsFailure)
        {
            return serverNameResult.Error;
        }

        string serverName = serverNameResult.Value;

        return new DbServerInfo(serverProductVersion, serverInstanceName, backupDirectory, defaultDataDirectory,
            defaultLogDirectory, isServerAllowsCompression, serverName);
    }

    private async Task<Result<string>> GetServerString(string query, CancellationToken cancellationToken,
        string? defString = null)
    {
        // ReSharper disable once using
        using DbManager? dbm = GetDbManager();
        if (dbm is null)
        {
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                cancellationToken);
        }

        try
        {
            dbm.ClearParameters();
            dbm.Open();
            string? executeScalarAsyncResult =
                await dbm.ExecuteScalarAsync<string>(query, null, CommandType.Text, cancellationToken) ?? defString;
            if (executeScalarAsyncResult is null)
            {
                return SqlDbClientErrors.ServerStringIsNull;
            }

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

    private async ValueTask<Result<string>> GetServerProductVersion(CancellationToken cancellationToken = default)
    {
        if (_memoServerProductVersion != null)
        {
            return _memoServerProductVersion;
        }

        const string query = "SELECT SERVERPROPERTY('productversion')";
        Result<string> getServerStringResult = await GetServerString(query, cancellationToken);
        if (getServerStringResult.IsFailure)
        {
            return getServerStringResult.Error;
        }

        _memoServerProductVersion = getServerStringResult.Value;
        return _memoServerProductVersion;
    }

    private async ValueTask<Result<string>> GetServerInstanceName(CancellationToken cancellationToken = default)
    {
        if (_memoServerInstanceName != null)
        {
            return _memoServerInstanceName;
        }

        //const string query = "SELECT SERVERPROPERTY('InstanceName')";
        const string query = "SELECT @@servicename";
        Result<string> getServerStringResult = await GetServerString(query, cancellationToken);
        if (getServerStringResult.IsFailure)
        {
            return getServerStringResult.Error;
        }

        _memoServerInstanceName = getServerStringResult.Value;
        return _memoServerInstanceName;
    }

    public override async Task<Result<List<DatabaseInfoModel>>> GetDatabaseInfos(
        CancellationToken cancellationToken = default)
    {
        // ReSharper disable once using
        using DbManager? dbm = GetDbManager();
        if (dbm is null)
        {
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                cancellationToken);
        }

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
            using IDataReader reader = await dbm.ExecuteReaderAsync(query, CommandType.Text, cancellationToken);
            while (reader.Read())
            {
                dbNames.Add(new DatabaseInfoModel(reader.GetString(1), (EDatabaseRecoveryModel)reader.GetByte(2),
                    reader.GetInt32(3) != 0));
            }

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

    private async Task<Result<bool>> GetServerIntBool(string query, CancellationToken cancellationToken,
        string? databaseName = null)
    {
        // ReSharper disable once using
        using DbManager? dbm = GetDbManager();
        if (dbm is null)
        {
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                cancellationToken);
        }

        try
        {
            if (databaseName is not null)
            {
                dbm.AddParameter("@database", databaseName);
            }

            dbm.Open();
            return await dbm.ExecuteScalarAsync(query, 0, CommandType.Text, cancellationToken) == 1;
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

    public override Task<Result<bool>> IsServerAllowsCompression(CancellationToken cancellationToken = default)
    {
        const string query = """
                             SELECT count(value)
                             FROM sys.configurations
                             WHERE name = 'backup compression default' AND maximum > 0
                             """;
        return GetServerIntBool(query, cancellationToken);
    }

    public override async Task<Result<bool>> IsServerLocal(CancellationToken cancellationToken = default)
    {
        const string queryString = "SELECT CONNECTIONPROPERTY('client_net_address') AS client_net_address";
        Result<string> getServerStringResult = await GetServerString(queryString, cancellationToken);
        if (getServerStringResult.IsFailure)
        {
            return getServerStringResult.Error;
        }

        string clientNetAddress = getServerStringResult.Value;
        return clientNetAddress is "<local machine>" or "127.0.0.1";
    }

    public override Task<Result> CheckRepairDatabase(string databaseName, CancellationToken cancellationToken = default)
    {
        string strCommand = $"DBCC CHECKDB(N'{databaseName}') WITH NO_INFOMSGS";
        return ExecuteCommand(strCommand, true, false, cancellationToken);
    }

    private async Task<Result<List<Tuple<string, string>>>> GetStoredProcedureNames(
        CancellationToken cancellationToken = default)

    {
        // ReSharper disable once using
        using DbManager? dbm = GetDbManager();
        if (dbm is null)
        {
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                cancellationToken);
        }

        try
        {
            dbm.Open();
            const string query = "exec sp_stored_procedures";

            // ReSharper disable once using
            using IDataReader reader = await dbm.ExecuteReaderAsync(query, CommandType.Text, cancellationToken);
            var storedProcedures = new List<Tuple<string, string>>();
            while (reader.Read())
            {
                storedProcedures.Add(new Tuple<string, string>(reader.GetString(1), reader.GetString(2)));
            }

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

    private async Task<Result<List<string>>> GetTriggerNames(CancellationToken cancellationToken = default)
    {
        var triggers = new List<string>();

        // ReSharper disable once using
        using DbManager? dbm = GetDbManager();
        if (dbm is null)
        {
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                cancellationToken);
        }

        try
        {
            dbm.Open();
            const string query = "SELECT name FROM sys.triggers WHERE type = 'TR'";
            // ReSharper disable once using
            using IDataReader reader = await dbm.ExecuteReaderAsync(query, CommandType.Text, cancellationToken);
            while (reader.Read())
            {
                triggers.Add(reader.GetString(0));
            }
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

    private async Task<Result<List<string>>> GetDatabaseTableNames(CancellationToken cancellationToken = default)
    {
        // ReSharper disable once using
        using DbManager? dbm = GetDbManager();
        if (dbm is null)
        {
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                cancellationToken);
        }

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
            using IDataReader reader = await dbm.ExecuteReaderAsync(query, CommandType.Text, cancellationToken);
            var tableNames = new List<string>();
            while (reader.Read())
            {
                tableNames.Add(reader.GetString(0));
            }

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

    private Task<Result> RecompileDatabaseObject(string strObjectName, CancellationToken cancellationToken = default)
    {
        return ExecuteCommand($"EXEC sp_recompile [{strObjectName}]", true, false, cancellationToken);
    }

    private Task<Result> UpdateStatisticsForOneTable(string strTableName, CancellationToken cancellationToken = default)
    {
        return ExecuteCommand($"UPDATE STATISTICS [{strTableName}] WITH FULLSCAN", true, false, cancellationToken);
    }

    public override async Task<Result> RecompileProcedures(string databaseName,
        CancellationToken cancellationToken = default)
    {
        await LogInfoAndSendMessage("Recompiling Tables, views and triggers for database {0}...", databaseName,
            cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            return DbToolsErrors.CancellationRequested(nameof(RecompileProcedures));
        }

        Result<string> serverName = await ServerName(cancellationToken);

        await LogInfoAndSendMessage("{0}_{1} Recompiling Stored Procedures...", serverName, databaseName,
            cancellationToken);

        Result<List<Tuple<string, string>>> getStoredProcedureNamesResult =
            await GetStoredProcedureNames(cancellationToken);
        if (getStoredProcedureNamesResult.IsFailure)
        {
            return getStoredProcedureNamesResult.Error;
        }

        List<Tuple<string, string>> storedProcedureNames = getStoredProcedureNamesResult.Value;
        string[] procNames =
        [
            .. storedProcedureNames.Where(w => w.Item1 != "sys" && !w.Item2.StartsWith("dt_", StringComparison.Ordinal))
                .Select(s => s.Item2)
        ];

        foreach (string strCurProcName in procNames)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return DbToolsErrors.CancellationRequested(nameof(RecompileProcedures));
            }

            char[] separators = [';'];
            string[] splitWords = strCurProcName.Split(separators);
            string strProcName = splitWords[0];
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return DbToolsErrors.CancellationRequested(nameof(RecompileProcedures));
                }

                Result recompileDatabaseObjectResult = await RecompileDatabaseObject(strProcName, cancellationToken);
                if (recompileDatabaseObjectResult.IsFailure)
                {
                    return recompileDatabaseObjectResult;
                }
            }
            catch (Exception ex)
            {
                StShared.WriteException(ex, $"{serverName}_{databaseName} ErrorOmd in Recompile Stored Procedures",
                    UseConsole, _logger);
            }
        }

        await LogInfoAndSendMessage("{0}_{1} Recompiling Triggers...", serverName, databaseName, cancellationToken);

        Result<List<string>> getTriggerNames = await GetTriggerNames(cancellationToken);
        if (getTriggerNames.IsFailure)
        {
            return getTriggerNames.Error;
        }

        List<string> triggerNames = getTriggerNames.Value;

        foreach (string strTriggerName in triggerNames)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return DbToolsErrors.CancellationRequested(nameof(RecompileProcedures));
            }

            try
            {
                Result recompileDatabaseObjectResult = await RecompileDatabaseObject(strTriggerName, cancellationToken);
                if (recompileDatabaseObjectResult.IsFailure)
                {
                    return recompileDatabaseObjectResult;
                }
            }
            catch (Exception ex)
            {
                StShared.WriteException(ex, $"{serverName}_{databaseName} ErrorOmd in Recompile trigger", UseConsole,
                    _logger);
            }
        }

        return null;
    }

    public override async Task<Result> UpdateStatistics(string databaseName,
        CancellationToken cancellationToken = default)
    {
        Result<string> serverName = await ServerName(cancellationToken);

        await LogInfoAndSendMessage("Update Statistics for database {0}_{1}...", serverName, databaseName,
            cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            return DbToolsErrors.CancellationRequested(nameof(UpdateStatistics));
        }

        //დადგინდეს მიმდინარე პერიოდისათვის შესრულდა თუ არა უკვე ეს პროცედურა. 
        //ამისათვის, საჭიროა ვიპოვოთ წინა პროცედურის დასრულების აღსანიშნავი ფაილი
        //და დავადგინოთ მისი შესრულების თარიღი.
        //თუ ეს თარიღი მიმდინარე პერიოდშია, მაშინ პროცედურა აღარ უნდა შესრულდეს
        try
        {
            Result<List<string>> getDatabaseTableNamesResult = await GetDatabaseTableNames(cancellationToken);
            if (getDatabaseTableNamesResult.IsFailure)
            {
                return getDatabaseTableNamesResult.Error;
            }

            List<string> tableNames = getDatabaseTableNamesResult.Value;
            foreach (string strTableName in tableNames)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return DbToolsErrors.CancellationRequested(nameof(UpdateStatistics));
                }

                Result updateStatisticsForOneTableResult =
                    await UpdateStatisticsForOneTable(strTableName, cancellationToken);
                if (updateStatisticsForOneTableResult.IsFailure)
                {
                    return updateStatisticsForOneTableResult;
                }
            }
        }
        catch (Exception ex)
        {
            return await LogErrorAndSendMessageFromException(ex, nameof(UpdateStatistics), cancellationToken);
        }

        return null;
    }

    public override async Task<Result> SetDefaultFolders(string defBackupFolder, string defDataFolder,
        string defLogFolder, CancellationToken cancellationToken = default)
    {
        Result<string> serverProductVersionResult = await GetServerProductVersion(cancellationToken);
        if (serverProductVersionResult.IsFailure)
        {
            return serverProductVersionResult.Error;
        }

        string serverProductVersion = serverProductVersionResult.Value;
        Result<string> serverInstanceNameResult = await GetServerInstanceName(cancellationToken);
        if (serverInstanceNameResult.IsFailure)
        {
            return serverInstanceNameResult.Error;
        }

        string serverInstanceName = serverInstanceNameResult.Value;

        Result regWriteResult = await RegWrite(serverProductVersion, serverInstanceName, null, CBackupDirectory,
            defBackupFolder, cancellationToken);
        if (regWriteResult.IsFailure)
        {
            return regWriteResult;
        }

        Result regWriteDataResult = await RegWrite(serverProductVersion, serverInstanceName, null, CDefaultData,
            defDataFolder, cancellationToken);
        if (regWriteDataResult.IsFailure)
        {
            return regWriteDataResult;
        }

        Result regWriteLogResult = await RegWrite(serverProductVersion, serverInstanceName, null, CDefaultLog,
            defLogFolder, cancellationToken);
        if (regWriteLogResult.IsFailure)
        {
            return regWriteLogResult;
        }

        return null;
    }

    public override Task<Result> ChangeDatabaseRecoveryModel(string databaseName,
        EDatabaseRecoveryModel databaseRecoveryModel, CancellationToken cancellationToken)
    {
        string recoveryModel = databaseRecoveryModel switch
        {
            EDatabaseRecoveryModel.Full => "FULL",
            EDatabaseRecoveryModel.BulkLogged => "BULK_LOGGED",
            EDatabaseRecoveryModel.Simple => "SIMPLE",
            _ => throw new ArgumentOutOfRangeException(nameof(databaseRecoveryModel), databaseRecoveryModel, null)
        };

        return ExecuteCommand($"ALTER DATABASE [{databaseName}] SET RECOVERY {recoveryModel}", true, false,
            cancellationToken);
    }

    //public override Task<Result<Dictionary<string, DatabaseFoldersSet>>> GetDatabaseFoldersSets(CancellationToken cancellationToken = default)
    //{
    //    throw new NotImplementedException();
    //}

    private async Task<Result<string>> ServerName(CancellationToken cancellationToken = default)
    {
        const string query = "SELECT @@servername";
        Result<string> getServerStringResult = await GetServerString(query, cancellationToken);
        return getServerStringResult;
    }
}
