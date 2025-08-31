using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using DbTools.Errors;
using DbTools.Models;
using LanguageExt;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;
using SystemToolsShared.Errors;

namespace DbTools;

public /*open*/ abstract class DbClient : MessageLogger
{
    private readonly DbConnectionStringBuilder _conStrBuilder;
    private readonly DbKit _dbKit;

    // ReSharper disable once ConvertToPrimaryConstructor
    protected DbClient(ILogger logger, DbConnectionStringBuilder conStrBuilder, DbKit dbKit, bool useConsole,
        IMessagesDataManager? messagesDataManager = null, string? userName = null) : base(logger, messagesDataManager,
        userName, useConsole)
    {
        _conStrBuilder = conStrBuilder;
        _dbKit = dbKit;
    }

    protected DbManager? GetDbManager()
    {
        return DbManager.Create(_dbKit, _conStrBuilder.ConnectionString);
    }

    public async Task<Option<IEnumerable<Err>>> ExecuteCommand(string strCommand, bool bLogStart, bool bLogFinish,
        CancellationToken cancellationToken = default)
    {
        // ReSharper disable once using
        using var dbm = GetDbManager();
        if (dbm is null)
            return (Err[])await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                cancellationToken);

        if (bLogStart)
            await LogInfoAndSendMessage("Start - {0} For Database - {1}.{2}", strCommand, dbm.Connection.DataSource,
                dbm.Connection.Database, cancellationToken);

        try
        {
            dbm.Open();
            await dbm.ExecuteNonQueryAsync(strCommand, CommandType.Text, cancellationToken);
        }
        catch (Exception ex)
        {
            return (Err[])await LogErrorAndSendMessageFromException(ex, nameof(ExecuteCommand), cancellationToken);
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

    protected async Task<OneOf<T, IEnumerable<Err>>> ExecuteScalarAsync<T>(string queryString,
        CancellationToken cancellationToken = default)
    {
        // ReSharper disable once using
        using var dbm = GetDbManager();
        if (dbm is null)
            return (Err[])await LogErrorAndSendMessageFromError(DbClientErrors.CannotCreateDatabaseConnection,
                cancellationToken);

        try
        {
            dbm.Open();
            var executeScalarAsyncResult =
                await dbm.ExecuteScalarAsync<T>(queryString, default, CommandType.Text, cancellationToken);
            if (executeScalarAsyncResult is null)
                return new[] { DbClientErrors.ExecuteScalarAsyncResultIsNull() };
            return executeScalarAsyncResult;
        }
        catch (Exception ex)
        {
            return (Err[])await LogErrorAndSendMessageFromException(ex, nameof(ExecuteScalarAsync), cancellationToken);
        }
        finally
        {
            dbm.Close();
        }
    }

    public abstract Task<Option<IEnumerable<Err>>> BackupDatabase(string databaseName, string backupFilename,
        string backupName, EBackupType backupType, bool compression, CancellationToken cancellationToken = default);

    public abstract Task<OneOf<string, IEnumerable<Err>>> HostPlatform(CancellationToken cancellationToken = default);

    public abstract Task<Option<IEnumerable<Err>>> VerifyBackup(string databaseName, string backupFilename,
        CancellationToken cancellationToken = default);

    public abstract Task<Option<IEnumerable<Err>>> RestoreDatabase(string databaseName, string backupFileFullName,
        List<RestoreFileModel>? files, string dataFolderName, string dataLogFolderName, string dirSeparator,
        CancellationToken cancellationToken = default);

    public abstract Task<OneOf<bool, IEnumerable<Err>>> IsDatabaseExists(string databaseName,
        CancellationToken cancellationToken = default);

    public abstract Task<OneOf<List<RestoreFileModel>, IEnumerable<Err>>> GetRestoreFiles(string backupFileFullName,
        CancellationToken cancellationToken = default);

    public abstract Task<OneOf<bool, IEnumerable<Err>>> IsServerAllowsCompression(
        CancellationToken cancellationToken = default);

    //withDatabase იყო True
    public abstract Task<Option<IEnumerable<Err>>> TestConnection(bool withDatabase,
        CancellationToken cancellationToken = default);

    public abstract Task<OneOf<DbServerInfo, IEnumerable<Err>>> GetDbServerInfo(
        CancellationToken cancellationToken = default);

    public abstract Task<OneOf<List<DatabaseInfoModel>, IEnumerable<Err>>> GetDatabaseInfos(
        CancellationToken cancellationToken = default);

    public abstract Task<OneOf<bool, IEnumerable<Err>>> IsServerLocal(CancellationToken cancellationToken = default);

    public abstract Task<Option<IEnumerable<Err>>> CheckRepairDatabase(string databaseName,
        CancellationToken cancellationToken = default);

    public abstract Task<Option<IEnumerable<Err>>> RecompileProcedures(string databaseName,
        CancellationToken cancellationToken = default);

    public abstract Task<Option<IEnumerable<Err>>> UpdateStatistics(string databaseName,
        CancellationToken cancellationToken = default);

    public abstract Task<Option<IEnumerable<Err>>> SetDefaultFolders(string defBackupFolder, string defDataFolder,
        string defLogFolder, CancellationToken cancellationToken = default);

    public abstract Task<Option<IEnumerable<Err>>> ChangeDatabaseRecoveryModel(string databaseName,
        EDatabaseRecoveryModel databaseRecoveryModel, CancellationToken cancellationToken);
}