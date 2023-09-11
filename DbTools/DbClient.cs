using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using DbTools.Models;
using Microsoft.Extensions.Logging;

namespace DbTools;

public /*open*/ abstract class DbClient
{
    private readonly DbConnectionStringBuilder _conStrBuilder;
    private readonly DbKit _dbKit;
    protected readonly ILogger Logger;
    protected readonly bool UseConsole;

    protected DbClient(ILogger logger, DbConnectionStringBuilder conStrBuilder, DbKit dbKit, bool useConsole)
    {
        Logger = logger;
        _conStrBuilder = conStrBuilder;
        _dbKit = dbKit;
        UseConsole = useConsole;
    }

    protected DbManager? GetDbManager()
    {
        return DbManager.Create(_dbKit, _conStrBuilder.ConnectionString);
    }

    public bool ExecuteCommand(string strCommand, bool bLogStart = false, bool bLogFinish = false)
    {
        var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return false;
        }

        var success = false;

        if (bLogStart)
            Logger.LogInformation(
                "Start - {strCommand} For Database - {dbm.Connection.DataSource}.{dbm.Connection.Database}", strCommand,
                dbm.Connection.DataSource, dbm.Connection.Database);

        try
        {
            dbm.Open();
            dbm.ExecuteNonQuery(strCommand);
            success = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in ExecuteCommandAsync");
        }
        finally
        {
            dbm.Close();
        }

        if (bLogFinish)
            Logger.LogInformation(
                "Finish - {strCommand} For Database - {dbm.Connection.DataSource}.{dbm.Connection.Database}",
                strCommand, dbm.Connection.DataSource, dbm.Connection.Database);

        return success;
    }

    public async Task<bool> ExecuteCommandAsync(string strCommand, CancellationToken cancellationToken,
        bool bLogStart = false, bool bLogFinish = false)
    {
        var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return false;
        }

        var success = false;

        if (bLogStart)
            Logger.LogInformation(
                "Start - {strCommand} For Database - {dbm.Connection.DataSource}.{dbm.Connection.Database}", strCommand,
                dbm.Connection.DataSource, dbm.Connection.Database);

        try
        {
            dbm.Open();
            await dbm.ExecuteNonQueryAsync(strCommand, cancellationToken);
            success = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in ExecuteCommandAsync");
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


        return success;
    }


    protected async Task<T?> ExecuteScalarAsync<T>(string queryString, CancellationToken cancellationToken)
    {
        var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return default;
        }

        try
        {
            dbm.Open();
            return await dbm.ExecuteScalarAsync<T>(queryString, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in ExecuteScalarAsync");
        }
        finally
        {
            dbm.Close();
        }

        return default;
    }


    public abstract Task<bool> BackupDatabase(string databaseName, string backupFilename, string backupName,
        EBackupType backupType, bool compression, CancellationToken cancellationToken);

    public abstract Task<string?> HostPlatform(CancellationToken cancellationToken);

    public abstract Task<bool> VerifyBackup(string databaseName, string backupFilename,
        CancellationToken cancellationToken);

    public abstract Task<bool> RestoreDatabase(string databaseName, string backupFileFullName,
        List<RestoreFileModel>? files, string dataFolderName, string dataLogFolderName, string dirSeparator,
        CancellationToken cancellationToken);

    public abstract Task<bool> CheckDatabase(string databaseName, CancellationToken cancellationToken);

    public abstract List<RestoreFileModel> GetRestoreFiles(string backupFileFullName);

    public abstract Task<bool> IsServerAllowsCompression(CancellationToken cancellationToken);

    public abstract bool TestConnection(bool withDatabase = true);

    public abstract Task<DbServerInfo> GetDbServerInfo(CancellationToken cancellationToken);

    public abstract Task<List<DatabaseInfoModel>> GetDatabaseInfos(CancellationToken cancellationToken);

    public abstract bool IsServerLocal();

    public abstract Task<bool> CheckRepairDatabase(string databaseName, CancellationToken cancellationToken);

    public abstract Task<bool> RecompileProcedures(string databaseName, CancellationToken cancellationToken);

    public abstract Task<bool> UpdateStatistics(string databaseName, CancellationToken cancellationToken);
}