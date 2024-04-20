using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
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

    public Option<Err[]> ExecuteCommand(string strCommand, bool bLogStart = false, bool bLogFinish = false)
    {
        // ReSharper disable once using
        using var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return new Err[]
            {
                new()
                {
                    ErrorCode = "CannotCreateDatabaseConnection", ErrorMessage = "Cannot create Database connection"
                }
            };
        }

        if (bLogStart)
            Logger.LogInformation(
                "Start - {strCommand} For Database - {dbm.Connection.DataSource}.{dbm.Connection.Database}", strCommand,
                dbm.Connection.DataSource, dbm.Connection.Database);

        try
        {
            dbm.Open();
            dbm.ExecuteNonQuery(strCommand);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in ExecuteCommand");
            return new Err[]
            {
                new()
                {
                    ErrorCode = "ErrorInExecuteCommand",
                    ErrorMessage = $"Error in ExecuteCommand {ex.Message}"
                }
            };
        }
        finally
        {
            dbm.Close();
        }

        if (bLogFinish)
            Logger.LogInformation(
                "Finish - {strCommand} For Database - {dbm.Connection.DataSource}.{dbm.Connection.Database}",
                strCommand, dbm.Connection.DataSource, dbm.Connection.Database);

        return null;
    }

    public async Task<Option<Err[]>> ExecuteCommandAsync(string strCommand, CancellationToken cancellationToken,
        bool bLogStart = false, bool bLogFinish = false)
    {
        // ReSharper disable once using
        using var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return new Err[]
            {
                new()
                {
                    ErrorCode = "CannotCreateDatabaseConnection", ErrorMessage = "Cannot create Database connection"
                }
            };
        }

        if (bLogStart)
            Logger.LogInformation(
                "Start - {strCommand} For Database - {dbm.Connection.DataSource}.{dbm.Connection.Database}", strCommand,
                dbm.Connection.DataSource, dbm.Connection.Database);

        try
        {
            dbm.Open();
            await dbm.ExecuteNonQueryAsync(strCommand, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in ExecuteCommandAsync");
            return new Err[]
            {
                new()
                {
                    ErrorCode = "ErrorInExecuteCommandAsync",
                    ErrorMessage = $"Error in ExecuteCommandAsync {ex.Message}"
                }
            };
        }
        finally
        {
            dbm.Close();
        }

        if (bLogFinish)
            Logger.LogInformation(
                "Finish - {strCommand} For Database - {dbm.Connection.DataSource}.{dbm.Connection.Database}",
                strCommand,
                dbm.Connection.DataSource, dbm.Connection.Database);

        return null;
    }


    protected async Task<OneOf<T, Err[]>> ExecuteScalarAsync<T>(string queryString, CancellationToken cancellationToken)
    {
        // ReSharper disable once using
        using var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return new Err[]
            {
                new()
                {
                    ErrorCode = "CannotCreateDatabaseConnection", ErrorMessage = "Cannot create Database connection"
                }
            };
        }

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
            Logger.LogError(ex, "Error in ExecuteScalarAsync");
            return new Err[]
            {
                new()
                {
                    ErrorCode = "ErrorInExecuteScalarAsync", ErrorMessage = "Error in ExecuteScalarAsync"
                }
            };
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

    public abstract OneOf<List<RestoreFileModel>, Err[]> GetRestoreFiles(string backupFileFullName);

    public abstract Task<OneOf<bool, Err[]>> IsServerAllowsCompression(CancellationToken cancellationToken);

    public abstract Option<Err[]> TestConnection(bool withDatabase = true);

    public abstract Task<OneOf<DbServerInfo, Err[]>> GetDbServerInfo(CancellationToken cancellationToken);

    public abstract Task<OneOf<List<DatabaseInfoModel>, Err[]>> GetDatabaseInfos(CancellationToken cancellationToken);

    public abstract Task<OneOf<bool, Err[]>> IsServerLocal(CancellationToken cancellationToken);

    public abstract Task<Option<Err[]>> CheckRepairDatabase(string databaseName, CancellationToken cancellationToken);

    public abstract Task<Option<Err[]>> RecompileProcedures(string databaseName, CancellationToken cancellationToken);

    public abstract Task<Option<Err[]>> UpdateStatistics(string databaseName, CancellationToken cancellationToken);
}