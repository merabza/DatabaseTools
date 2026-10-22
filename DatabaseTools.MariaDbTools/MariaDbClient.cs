using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using DatabaseTools.DbTools;
using DatabaseTools.DbTools.Errors;
using DatabaseTools.DbTools.Models;
using DatabaseTools.MariaDbTools.Errors;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace DatabaseTools.MariaDbTools;

//MariaDB-ის კლიენტი: კავშირის შემოწმება, სერვერისა და ბაზების ინფორმაცია;
//ბექაპი, აღდგენა და ბაზის მომსახურების ოპერაციები MariaDB-სთვის მხარდაჭერილი არ არის
public sealed class MariaDbClient : DbClient
{
    private static readonly string[] SystemDatabaseNames = ["mysql", "sys"];
    private readonly ILogger _logger;

    public MariaDbClient(ILogger logger, MySqlConnectionStringBuilder conStrBuilder, DbKit dbKit, bool useConsole,
        IMessagesDataManager? messagesDataManager = null, string? userName = null) : base(logger, conStrBuilder, dbKit,
        useConsole, messagesDataManager, userName)
    {
        _logger = logger;
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

    public override async Task<Result<DbServerInfo>> GetDbServerInfo(CancellationToken cancellationToken = default)
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
            // ReSharper disable once using
            using IDataReader reader = await dbm.ExecuteReaderAsync("SELECT VERSION(), @@hostname, @@datadir",
                CommandType.Text, cancellationToken);
            if (!reader.Read())
            {
                return MariaDbClientErrors.ServerInfoIsNotDetected;
            }

            //MariaDB-ს დასახელებული ინსტანციები, სერვერის მხარის ბექაპის საქაღალდე და ბექაპის შეკუმშვა არ აქვს
            return new DbServerInfo(reader.GetString(0), string.Empty, null, reader.GetString(2), null, false,
                reader.GetString(1));
        }
        catch (Exception ex)
        {
            return await LogErrorAndSendMessageFromException(ex, nameof(GetDbServerInfo), cancellationToken);
        }
        finally
        {
            dbm.Close();
        }
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
            //ვირტუალური სქემები გამოირიცხება, როგორც SQL Server-ის tempdb
            const string query = """
                                 SELECT SCHEMA_NAME
                                 FROM information_schema.SCHEMATA
                                 WHERE SCHEMA_NAME NOT IN ('information_schema', 'performance_schema')
                                 ORDER BY SCHEMA_NAME
                                 """;
            var dbNames = new List<DatabaseInfoModel>();
            // ReSharper disable once using
            using IDataReader reader = await dbm.ExecuteReaderAsync(query, CommandType.Text, cancellationToken);
            while (reader.Read())
            {
                string databaseName = reader.GetString(0);
                //MariaDB-ს აღდგენის მოდელი არ აქვს — ყველაზე ახლოს Simple-ია (ლოგის ბექაპები არ არსებობს)
                dbNames.Add(new DatabaseInfoModel(databaseName, EDatabaseRecoveryModel.Simple,
                    SystemDatabaseNames.Contains(databaseName, StringComparer.OrdinalIgnoreCase)));
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

    public override async Task<Result<bool>> IsDatabaseExists(string databaseName,
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
            dbm.AddParameter("@database", databaseName);
            dbm.Open();
            //MariaDB-ში COUNT(*) BIGINT-ია
            return await dbm.ExecuteScalarAsync(
                "SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = @database", 0L, CommandType.Text,
                cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            return await LogErrorAndSendMessageFromException(ex, nameof(IsDatabaseExists), cancellationToken);
        }
        finally
        {
            dbm.Close();
        }
    }

    public override async Task<Result<bool>> IsServerLocal(CancellationToken cancellationToken = default)
    {
        //USER() აბრუნებს user@host-ს, სადაც host ის მისამართია, საიდანაც სერვერი ამ კავშირს ხედავს
        Result<string> clientHostResult =
            await ExecuteScalarAsync<string>("SELECT SUBSTRING_INDEX(USER(), '@', -1)", cancellationToken);
        if (clientHostResult.IsFailure)
        {
            return clientHostResult.Error;
        }

        return clientHostResult.Value is "localhost" or "127.0.0.1" or "::1";
    }

    public override Task<Result<bool>> IsServerAllowsCompression(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<bool>>(false);
    }

    public override Task<Result<string>> HostPlatform(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<string>>(MariaDbClientErrors.OperationIsNotSupported(nameof(HostPlatform)));
    }

    public override Task<Result> BackupDatabase(string databaseName, string backupFilename, string backupName,
        EBackupType backupType, bool compression, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result>(MariaDbClientErrors.OperationIsNotSupported(nameof(BackupDatabase)));
    }

    public override Task<Result> VerifyBackup(string databaseName, string backupFilename,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result>(MariaDbClientErrors.OperationIsNotSupported(nameof(VerifyBackup)));
    }

    public override Task<Result> RestoreDatabase(string databaseName, string backupFileFullName,
        List<RestoreFileModel>? files, string dataFolderName, string dataLogFolderName, string dirSeparator,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result>(MariaDbClientErrors.OperationIsNotSupported(nameof(RestoreDatabase)));
    }

    public override Task<Result<List<RestoreFileModel>>> GetRestoreFiles(string backupFileFullName,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<List<RestoreFileModel>>>(
            MariaDbClientErrors.OperationIsNotSupported(nameof(GetRestoreFiles)));
    }

    public override Task<Result> CheckRepairDatabase(string databaseName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result>(MariaDbClientErrors.OperationIsNotSupported(nameof(CheckRepairDatabase)));
    }

    public override Task<Result> RecompileProcedures(string databaseName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result>(MariaDbClientErrors.OperationIsNotSupported(nameof(RecompileProcedures)));
    }

    public override Task<Result> UpdateStatistics(string databaseName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result>(MariaDbClientErrors.OperationIsNotSupported(nameof(UpdateStatistics)));
    }

    public override Task<Result> SetDefaultFolders(string defBackupFolder, string defDataFolder, string defLogFolder,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result>(MariaDbClientErrors.OperationIsNotSupported(nameof(SetDefaultFolders)));
    }

    public override Task<Result> ChangeDatabaseRecoveryModel(string databaseName,
        EDatabaseRecoveryModel databaseRecoveryModel, CancellationToken cancellationToken)
    {
        return Task.FromResult<Result>(
            MariaDbClientErrors.OperationIsNotSupported(nameof(ChangeDatabaseRecoveryModel)));
    }
}
