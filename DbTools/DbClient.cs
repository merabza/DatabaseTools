using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using DbTools.Models;
using Microsoft.Extensions.Logging;

namespace DbTools;

public /*open*/ class DbClient
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
            Logger.LogInformation("Start - " + strCommand + " For Database - " + dbm.Connection.DataSource + "." +
                                  dbm.Connection.Database);

        try
        {
            dbm.Open();
            dbm.ExecuteNonQuery(strCommand);
            success = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, null);
        }
        finally
        {
            dbm.Close();
        }

        if (bLogFinish)
            Logger.LogInformation("Finish - " + strCommand + " For Database - " + dbm.Connection.DataSource + "." +
                                  dbm.Connection.Database);

        return success;
    }

    public async Task<bool> ExecuteCommandAsync(string strCommand, bool bLogStart = false, bool bLogFinish = false)
    {
        var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return false;
        }

        var success = false;

        if (bLogStart)
            Logger.LogInformation("Start - " + strCommand + " For Database - " + dbm.Connection.DataSource + "." +
                                  dbm.Connection.Database);

        try
        {
            dbm.Open();
            await dbm.ExecuteNonQueryAsync(strCommand);
            success = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, null);
        }
        finally
        {
            dbm.Close();
        }

        if (bLogFinish)
            Logger.LogInformation("Finish - " + strCommand + " For Database - " + dbm.Connection.DataSource + "." +
                                  dbm.Connection.Database);

        return success;
    }


    protected async Task<T?> ExecuteScalarAsync<T>(string queryString)
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
            return await dbm.ExecuteScalarAsync<T>(queryString);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, null);
        }
        finally
        {
            dbm.Close();
        }

        return default;
    }


    public virtual Task<bool> BackupDatabase(string databaseName, string backupFilename, string backupName,
        EBackupType backupType, bool compression)
    {
        return Task.FromResult(false);
    }

    public virtual Task<string?> HostPlatform()
    {
        return Task.FromResult<string?>(null);
    }

    public virtual Task<bool> VerifyBackup(string databaseName, string backupFilename)
    {
        return Task.FromResult(false);
    }

    public virtual Task<bool> RestoreDatabase(string databaseName, string backupFileFullName,
        List<RestoreFileModel>? files, string dataFolderName, string dataLogFolderName, string dirSeparator)
    {
        return Task.FromResult(false);
    }

    public virtual Task<bool> CheckDatabase(string databaseName)
    {
        return Task.FromResult(false);
    }

    public virtual List<RestoreFileModel> GetRestoreFiles(string backupFileFullName)
    {
        return new List<RestoreFileModel>();
    }

    public virtual Task<bool> IsServerAllowsCompression()
    {
        return Task.FromResult(false);
    }

    public virtual bool TestConnection(bool withDatabase = true)
    {
        return false;
    }

    public virtual Task<DbServerInfo> GetDbServerInfo()
    {
        throw new NotImplementedException();
    }

    public virtual Task<List<DatabaseInfoModel>> GetDatabaseInfos()
    {
        throw new NotImplementedException();
    }

    public virtual bool IsServerLocal()
    {
        return false;
    }

    public virtual Task<bool> CheckRepairDatabase(string databaseName)
    {
        return Task.FromResult(false);
    }

    public virtual Task<bool> RecompileProcedures(string databaseName)
    {
        return Task.FromResult(false);
    }

    public virtual Task<bool> UpdateStatistics(string databaseName)
    {
        return Task.FromResult(false);
    }
}