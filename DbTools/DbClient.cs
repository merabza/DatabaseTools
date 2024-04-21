using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using DbTools.ErrorModels;
using DbTools.Models;
using LanguageExt;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;

// ReSharper disable ConvertToPrimaryConstructor

namespace DbTools;

public /*open*/ abstract class DbClient : MessageLogger
{
    private readonly DbConnectionStringBuilder _conStrBuilder;
    private readonly DbKit _dbKit;
    protected readonly bool UseConsole;

    protected DbClient(ILogger logger, DbConnectionStringBuilder conStrBuilder, DbKit dbKit, bool useConsole,
        IMessagesDataManager? messagesDataManager = null, string? userName = null) : base(logger, messagesDataManager,
        userName, useConsole)
    {
        _conStrBuilder = conStrBuilder;
        _dbKit = dbKit;
        UseConsole = useConsole;
    }

    protected DbManager? GetDbManager()
    {
        return DbManager.Create(_dbKit, _conStrBuilder.ConnectionString);
    }

    //public async Task<Option<Err[]>> ExecuteCommand(string strCommand, CancellationToken cancellationToken,
    //    bool bLogStart = false, bool bLogFinish = false)
    public async Task<Option<Err[]>> ExecuteCommand(string strCommand, bool bLogStart, bool bLogFinish,
        CancellationToken cancellationToken)
    {
        // ReSharper disable once using
        using var dbm = GetDbManager();
        if (dbm is null)
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                CancellationToken.None);

        if (bLogStart)
            await LogInfoAndSendMessage("Start - {0} For Database - {1}.{2}", strCommand, dbm.Connection.DataSource,
                dbm.Connection.Database, cancellationToken);

        try
        {
            dbm.Open();
            await dbm.ExecuteNonQueryAsync(strCommand, cancellationToken);
        }
        catch (Exception ex)
        {
            return await LogErrorAndSendMessageFromException(ex, nameof(ExecuteCommand), cancellationToken);
        }
        finally
        {
            dbm.Close();
        }

        if (bLogFinish)
            await LogInfoAndSendMessage("Finish - {0} For Database - {1}.{2}", strCommand, dbm.Connection.DataSource,
                dbm.Connection.Database, cancellationToken);

        return null;
    }


    protected async Task<OneOf<T, Err[]>> ExecuteScalarAsync<T>(string queryString, CancellationToken cancellationToken)
    {
        // ReSharper disable once using
        using var dbm = GetDbManager();
        if (dbm is null)
            return await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                CancellationToken.None);

        try
        {
            dbm.Open();
            var executeScalarAsyncResult = await dbm.ExecuteScalarAsync<T>(queryString, cancellationToken);
            if (executeScalarAsyncResult is null)
                return new Err[]
                {
                    new()
                    {
                        ErrorCode = "ExecuteScalarAsyncResultIsNull", ErrorMessage = "ExecuteScalarAsync Result Is Null"
                    }
                };
            return executeScalarAsyncResult;
        }
        catch (Exception ex)
        {
            return await LogErrorAndSendMessageFromException(ex, nameof(ExecuteScalarAsync), cancellationToken);
        }
        finally
        {
            dbm.Close();
        }
    }

    public abstract Task<Option<Err[]>> BackupDatabase(string databaseName, string backupFilename, string backupName,
        EBackupType backupType, bool compression, CancellationToken cancellationToken);

    public abstract Task<OneOf<string, Err[]>> HostPlatform(CancellationToken cancellationToken);

    public abstract Task<Option<Err[]>> VerifyBackup(string databaseName, string backupFilename,
        CancellationToken cancellationToken);

    public abstract Task<Option<Err[]>> RestoreDatabase(string databaseName, string backupFileFullName,
        List<RestoreFileModel>? files, string dataFolderName, string dataLogFolderName, string dirSeparator,
        CancellationToken cancellationToken);

    public abstract Task<OneOf<bool, Err[]>> IsDatabaseExists(string databaseName, CancellationToken cancellationToken);

    public abstract Task<OneOf<List<RestoreFileModel>, Err[]>> GetRestoreFiles(string backupFileFullName,
        CancellationToken cancellationToken);

    public abstract Task<OneOf<bool, Err[]>> IsServerAllowsCompression(CancellationToken cancellationToken);

    //withDatabase იყო True
    public abstract Task<Option<Err[]>> TestConnection(bool withDatabase, CancellationToken cancellationToken);

    public abstract Task<OneOf<DbServerInfo, Err[]>> GetDbServerInfo(CancellationToken cancellationToken);

    public abstract Task<OneOf<List<DatabaseInfoModel>, Err[]>> GetDatabaseInfos(CancellationToken cancellationToken);

    public abstract Task<OneOf<bool, Err[]>> IsServerLocal(CancellationToken cancellationToken);

    public abstract Task<Option<Err[]>> CheckRepairDatabase(string databaseName, CancellationToken cancellationToken);

    public abstract Task<Option<Err[]>> RecompileProcedures(string databaseName, CancellationToken cancellationToken);

    public abstract Task<Option<Err[]>> UpdateStatistics(string databaseName, CancellationToken cancellationToken);
}