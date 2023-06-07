using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DbTools;
using DbTools.Models;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace SqlServerDbTools;

public sealed class SqlDbClient : DbClient
{
    private string? _memoServerInstanceName;


    private string? _memoServerProductVersion;

    public SqlDbClient(ILogger logger, SqlConnectionStringBuilder conStrBuilder, DbKit dbKit, bool useConsole) :
        base(logger,
            conStrBuilder, dbKit, useConsole)
    {
    }

    public override async Task<bool> BackupDatabase(string databaseName, string backupFilename, string backupName,
        EBackupType backupType, bool compression)
    {
        var buTypeWord = "DATABASE";
        if (backupType == EBackupType.TrLog)
            buTypeWord = "LOG";
        var buDifferentialWord = "";
        if (backupType == EBackupType.Diff)
            buDifferentialWord = "DIFFERENTIAL, ";

        return await ExecuteCommandAsync(@$"BACKUP {buTypeWord} [{databaseName}] 
TO DISK=N'{backupFilename}' 
WITH {buDifferentialWord}NOFORMAT, NOINIT, NAME = N'{backupName}', SKIP, REWIND, NOUNLOAD{(compression ? ", COMPRESSION" : "")}");
        //STATS = 1 აქ ჯერჯერობით არ ვიყენებთ, რადგან არ გვაქვს უკუკავშირი აწყობილი პროცენტების ჩვენებით
        //თუმცა თუ STATS მითითებული არ აქვს ავტომატურად აკეთებს STATS=10
        //STATS [ = percentage ] Displays a message each time another percentage completes, and is used to gauge progress. If percentage is omitted, SQL Server displays a message after each 10 percent is completed.
    }

    public override async Task<string?> HostPlatform()
    {
        const string queryString = @"SELECT host_platform FROM sys.dm_os_host_info";
        return await ExecuteScalarAsync<string>(queryString);
    }

    public override async Task<bool> VerifyBackup(string databaseName, string backupFilename)
    {
        return await ExecuteCommandAsync(
            @$"DECLARE @backupSetId as int 
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
RESTORE VERIFYONLY FROM DISK = N'{backupFilename}' WITH  FILE = @backupSetId, NOUNLOAD, NOREWIND");
        //STATS = 1 აქ ჯერჯერობით არ ვიყენებთ, რადგან არ გვაქვს უკუკავშირი აწყობილი პროცენტების ჩვენებით
    }

    public override async Task<bool> CheckDatabase(string databaseName)
    {
        var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return false;
        }

        try
        {
            var query = @"select count(*) from master.dbo.sysdatabases where name=@database";
            dbm.AddParameter("@database", databaseName);
            dbm.Open();
            return await dbm.ExecuteScalarAsync<int>(query) == 1;
        }
        catch (Exception ex)
        {
            StShared.WriteException(ex, UseConsole, Logger);
        }
        finally
        {
            dbm.Close();
        }

        return false;
    }


    public override List<RestoreFileModel> GetRestoreFiles(string backupFileFullName)
    {
        var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return new List<RestoreFileModel>();
        }

        try
        {
            var query = $@"RESTORE FILELISTONLY FROM  DISK = N'{backupFileFullName}' WITH  NOUNLOAD,  FILE = 1";
            dbm.Open();
            var reader = dbm.ExecuteReader(query);
            var fileNames = new List<RestoreFileModel>();
            while (reader.Read())
                fileNames.Add(new RestoreFileModel((string)reader["LogicalName"],
                    (string)reader["Type"]));

            return fileNames;
        }
        catch (Exception ex)
        {
            StShared.WriteException(ex, UseConsole, Logger);
        }
        finally
        {
            dbm.Close();
        }

        return new List<RestoreFileModel>();
    }


    public override async Task<bool> RestoreDatabase(string databaseName, string backupFileFullName,
        List<RestoreFileModel>? files, string dataFolderName, string dataLogFolderName, string dirSeparator)
    {
        if (files == null)
        {
            Logger.LogError("No information about restore file logical parts");
            return false;
        }

        var dataPart = files.SingleOrDefault(s => s.Type == "D");
        if (dataPart == null)
        {
            Logger.LogError("No information about restore file Data Part");
            return false;
        }

        var logPart = files.SingleOrDefault(s => s.Type == "L");
        if (logPart == null)
        {
            Logger.LogError("No information about restore file Log Part");
            return false;
        }

        var dataPartFileFullName = $"{dataFolderName.AddNeedLastPart(dirSeparator)}{databaseName}.mdf";
        var dataLogPartFileFullName = $"{dataLogFolderName.AddNeedLastPart(dirSeparator)}{databaseName}_log.ldf";


        return await ExecuteCommandAsync(@$"RESTORE DATABASE [{databaseName}] 
FROM  DISK = N'{backupFileFullName}' WITH  FILE = 1,  
MOVE N'{dataPart.LogicalName}' TO N'{dataPartFileFullName}',  
MOVE N'{logPart.LogicalName}' TO N'{dataLogPartFileFullName}', NOUNLOAD, REPLACE");
        //STATS = 1 აქ ჯერჯერობით არ ვიყენებთ, რადგან არ გვაქვს უკუკავშირი აწყობილი პროცენტების ჩვენებით
    }

    public override bool TestConnection(bool withDatabase = true)
    {
        var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return false;
        }

        if (dbm.ConnectionString == "")
        {
            Logger.LogError("Connection Server Not specified");
            return false;
        }

        try
        {
            dbm.Open();
            dbm.Close();
            if (dbm.Database == "")
                if (withDatabase)
                {
                    Logger.LogError("Test Connection Succeeded, But Database name Not specified");
                    return false;
                }

            Logger.LogInformation("Test Connection Succeeded");
            return true;
        }
        catch (Exception ex)
        {
            StShared.WriteException(ex, "Connection Failed", UseConsole, Logger);
            return false;
        }
    }

    private async Task<string?> RegRead(string sqlServerProductVersion, string instanceName, string? subRegFolder,
        string parameterName)
    {
        var serverVersionParts = sqlServerProductVersion.Split('.');
        int.TryParse(serverVersionParts[0], out var serverVersionNum);
        if (serverVersionParts.Length <= 1)
            return null;

        var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return null;
        }

        try
        {
            dbm.ClearParameters();
            dbm.Open();
            var query = serverVersionNum > 10
                ? $@"EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer{(subRegFolder == null ? "" : $@"\{subRegFolder}")}', '{parameterName}'"
                : $@"EXEC master.dbo.xp_regread N'HKEY_LOCAL_MACHINE', N'SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL{serverVersionParts[0]}_{serverVersionParts[1]}.{instanceName}\MSSQLServer{(subRegFolder == null ? "" : $@"\{subRegFolder}")}', N'{parameterName}'";
            var reader = await dbm.ExecuteReaderAsync(query);
            if (reader.Read())
                return reader.GetString(1);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, null);
            throw;
        }
        finally
        {
            dbm.Close();
        }

        return null;
    }

    private string? GetMasterDir(string? masterFileName)
    {
        //პირველი 2 სიმბოლო ზედმეტია
        return masterFileName == null ? null : Path.GetDirectoryName(masterFileName[2..]);
    }


    public override async Task<DbServerInfo> GetDbServerInfo()
    {
        var serverProductVersion = await GetServerProductVersion();
        var serverInstanceName = await GetServerInstanceName();
        if (serverProductVersion is null || serverInstanceName is null)
            throw new Exception("error when get server info 1");
        var backupDirectory = await RegRead(serverProductVersion, serverInstanceName, null, "BackupDirectory");
        //თუ სპეციალურად არ არის განსაზღვრული, რომელი ფოლდერი უნდა გამოიყენოს სერვერმა ბაზებისათვის, მაშინ იყენებს მასტერის ადგილმდებარეობას
        var defaultDataDirectory =
            await RegRead(serverProductVersion, serverInstanceName, null, "DefaultData") ??
            GetMasterDir(await RegRead(serverProductVersion, serverInstanceName, "Parameters", "SqlArg0"));
        //თუ სპეციალურად არ არის განსაზღვრული, რომელი ფოლდერი უნდა გამოიყენოს სერვერმა ბაზების ლოგებისათვის, მაშინ იყენებს მასტერის ლოგების ადგილმდებარეობას
        var defaultLogDirectory = await RegRead(serverProductVersion, serverInstanceName, null, "DefaultLog") ??
                                  GetMasterDir(await RegRead(serverProductVersion, serverInstanceName,
                                      "Parameters", "SqlArg1"));
        //if (backupDirectory is null || defaultDataDirectory is null || defaultLogDirectory is null)
        //    throw new Exception("error when get server info 2");
        return new DbServerInfo(serverProductVersion, serverInstanceName, backupDirectory, defaultDataDirectory,
            defaultLogDirectory, await IsServerAllowsCompression(), await ServerName());
    }

    private async Task<string?> GetServerProductVersion()
    {
        if (_memoServerProductVersion != null)
            return _memoServerProductVersion;

        var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return null;
        }

        try
        {
            dbm.ClearParameters();
            dbm.Open();
            const string query = @"SELECT SERVERPROPERTY('productversion')";
            _memoServerProductVersion = await dbm.ExecuteScalarAsync<string>(query);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, null);
            throw;
        }
        finally
        {
            dbm.Close();
        }

        return _memoServerProductVersion;
    }

    private async Task<string?> GetServerInstanceName()
    {
        if (_memoServerInstanceName != null)
            return _memoServerInstanceName;

        var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return null;
        }

        try
        {
            dbm.ClearParameters();
            dbm.Open();
            const string query = @"SELECT SERVERPROPERTY('InstanceName')";
            _memoServerInstanceName = await dbm.ExecuteScalarAsync<string>(query) ?? "MSSQLSERVER";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, null);
            throw;
        }
        finally
        {
            dbm.Close();
        }

        return _memoServerInstanceName;
    }

    public override async Task<List<DatabaseInfoModel>> GetDatabaseInfos()
    {
        var dbm = GetDbManager();
        if (dbm is null)
            //Logger.LogError("Cannot create Database connection");
            throw new Exception("Cannot create Database connection");

        try
        {
            dbm.Open();
            const string query = @"SELECT database_id as dbId, name as dbName, recovery_model as recoveryModel, 
  (CASE WHEN name IN ('master', 'model', 'msdb') THEN 1 ELSE is_distributor END) as isSystemDatabase, 
  0 as dbChecked
FROM sys.databases
WHERE name <> 'tempdb'";
            var dbNames = new List<DatabaseInfoModel>();
            var reader = await dbm.ExecuteReaderAsync(query);
            while (reader.Read())
                dbNames.Add(new DatabaseInfoModel(reader.GetString(1),
                    (EDatabaseRecovery)reader.GetByte(2),
                    reader.GetInt32(3) != 0));
            return dbNames;
        }
        catch (Exception ex)
        {
            StShared.WriteException(ex, UseConsole, Logger);
            throw new Exception("Exception on Get Database info");
        }
        finally
        {
            dbm.Close();
        }
    }

    public override async Task<bool> IsServerAllowsCompression()
    {
        var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return false;
        }

        try
        {
            dbm.Open();
            const string query = @"SELECT count(value)
FROM sys.configurations 
WHERE name = 'backup compression default' AND maximum > 0";
            return await dbm.ExecuteScalarAsync<int>(query) == 1;
        }
        catch (Exception ex)
        {
            StShared.WriteException(ex, UseConsole, Logger);
            return false;
        }
        finally
        {
            dbm.Close();
        }
    }


    public override bool IsServerLocal()
    {
        const string queryString = @"SELECT CONNECTIONPROPERTY('client_net_address') AS client_net_address";

        var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return false;
        }

        try
        {
            dbm.Open();
            var clientNetAddress = dbm.ExecuteScalar<string>(queryString);
            if (clientNetAddress is null)
                return false;
            return clientNetAddress is "<local machine>" or "127.0.0.1";
        }
        catch (Exception ex)
        {
            StShared.WriteException(ex, UseConsole, Logger);
            return false;
        }
        finally
        {
            dbm.Close();
        }
    }


    public override async Task<bool> CheckRepairDatabase(string databaseName)
    {
        var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return false;
        }

        try
        {
            var strCommand = $@"DBCC CHECKDB(N'{databaseName}') WITH NO_INFOMSGS";
            dbm.Open();
            await dbm.ExecuteNonQueryAsync(strCommand);
            return true;
        }
        catch (Exception ex)
        {
            StShared.WriteException(ex, UseConsole, Logger);
        }
        finally
        {
            dbm.Close();
        }

        return false;
    }

    private async Task<List<Tuple<string, string>>> GetStoredProcedureNames()

    {
        var storedProcedures = new List<Tuple<string, string>>();


        var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return storedProcedures;
        }

        try
        {
            dbm.Open();
            const string query = @"exec sp_stored_procedures";

            var reader = await dbm.ExecuteReaderAsync(query);
            while (reader.Read())
                storedProcedures.Add(new Tuple<string, string>(reader.GetString(1), reader.GetString(2)));
        }
        catch (Exception ex)
        {
            StShared.WriteException(ex, UseConsole, Logger);
        }
        finally
        {
            dbm.Close();
        }

        return storedProcedures;
    }

    private List<string> GetTriggerNames()
    {
        var triggers = new List<string>();

        var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return triggers;
        }

        try
        {
            dbm.Open();
            const string query = @"SELECT name FROM sys.triggers WHERE type = 'TR'";
            var reader = dbm.ExecuteReader(query);
            while (reader.Read())
                triggers.Add(reader.GetString(0));
        }
        catch (Exception ex)
        {
            StShared.WriteException(ex, UseConsole, Logger);
        }
        finally
        {
            dbm.Close();
        }

        return triggers;
    }

    private async Task<List<string>> GetDatabaseTableNames()
    {
        var triggers = new List<string>();

        var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return triggers;
        }

        try
        {
            dbm.Open();

            const string query = @"SELECT o.name AS TableName
FROM dbo.sysobjects o 
  INNER JOIN dbo.sysindexes i ON o.id = i.id
WHERE (OBJECTPROPERTY(o.id, N'IsTable') = 1) 
  AND (i.indid < 2) 
  AND (o.name NOT LIKE N'#%') 
  AND (OBJECTPROPERTY(o.id, N'tableisfake') <> 1)
  AND USER_NAME(o.uid) <> 'sys'
ORDER BY TableName";

            var reader = await dbm.ExecuteReaderAsync(query);
            while (reader.Read())
                triggers.Add(reader.GetString(0));
        }
        catch (Exception ex)
        {
            StShared.WriteException(ex, UseConsole, Logger);
        }
        finally
        {
            dbm.Close();
        }

        return triggers;
    }

    private async Task RecompileDatabaseObject(string strObjectName)
    {
        await ExecuteCommandAsync($"EXEC sp_recompile [{strObjectName}]", true);
    }

    private async Task UpdateStatisticsForOneTable(string strTableName)
    {
        await ExecuteCommandAsync($"UPDATE STATISTICS [{strTableName}] WITH FULLSCAN", true);
    }


    public override async Task<bool> RecompileProcedures(string databaseName)
    {
        Logger.LogInformation($"Recompiling Tables, views and triggers for database {databaseName}...");

        //if (_bp.CancelationPending())
        //  return;

        //DbManager dbm = GetDbManager();
        var serverName = await ServerName();

        //-------------------------------------------------------------------
        //_bp.CurrentActivity = $"{serverName}_{dbname} Recompiling Stored Procedures...";

        var storedProcedureNames = await GetStoredProcedureNames();
        var procNames = storedProcedureNames.Where(w => w.Item1 != "sys" && !w.Item2.StartsWith("dt_"))
            .Select(s => s.Item2).ToArray();
        //_bp.SubLength = procNames.Length;
        //_bp.SubCounted = 0;
        foreach (var strCurProcName in procNames)
        {
            //if (_bp.CancelationPending())
            //  return;

            char[] separators = { ';' };
            var splitWords = strCurProcName.Split(separators);
            var strProcName = splitWords[0];
            try
            {
                //if (_bp.CancelationPending())
                //  return;
                await RecompileDatabaseObject(strProcName); //.RemoveUnnecesseryLeadPart("dbo.")
            }
            catch (Exception ex)
            {
                StShared.WriteException(ex, $"{serverName}_{databaseName} Error in Recompile Stored Procedures",
                    UseConsole, Logger);
            }
            //_bp.SubCounted++;
        }

        //_bp.SubLength = 0;
        //_bp.SubCounted = 0;
        //-------------------------------------------------------------------
        //_bp.CurrentActivity = $"{serverName}_{dbname} Recompiling Triggers...";
        var triggerNames = GetTriggerNames();
        //_bp.SubLength = triggerNames.Length;
        //_bp.SubCounted = 0;
        foreach (var strTriggerName in triggerNames)
            //if (_bp.CancelationPending())
            //  return;

            try
            {
                //if (_bp.CancelationPending())
                //  return;
                await RecompileDatabaseObject(strTriggerName); //.RemoveUnnecesseryLeadPart("dbo.")
            }
            catch (Exception ex)
            {
                StShared.WriteException(ex, $"{serverName}_{databaseName} Error in Recompile trigger", UseConsole,
                    Logger);
            }
        //_bp.SubCounted++;
        //_bp.SubLength = 0;
        //_bp.SubCounted = 0;


        ////სამუშაო ფოლდერში შეიქმნას ფაილი, რომელიც იქნება იმის აღმნიშვნელი, რომ ეს პროცესი შესრულდა და წარმატებით დასრულდა.
        ////ფაილის სახელი უნდა შედგებოდეს პროცედურის სახელისაგან თარიღისა და დროისაგან (როცა პროცესი დასრულდა)
        ////ასევე სერვერის სახელი და ბაზის სახელი.
        ////გაფართოება log
        //procLogFile.CreateNow("Ok");
        ////ასევე წაიშალოს ანალოგიური პროცესის მიერ წინათ შექმნილი ფაილები
        //procLogFile.DeleteOldFiles();
        //Loger.Instance.LogMessage("Ok");
        //_bp.Counted++;

        //  _bp.Length = 0;
        //_bp.Counted = 0;


        return true;
    }


    public override async Task<bool> UpdateStatistics(string databaseName)
    {
        Logger.LogInformation($"Update Statistics for database {databaseName}...");

        //_bp.Length = dbnames.Length;
        //_bp.Counted = 0;

        var serverName = await ServerName();

        //if (_bp.CancelationPending())
        //    return;

        //დადგინდეს მიმდინარე პერიოდისათვის შესრულდა თუ არა უკვე ეს პროცედურა. 
        //ამისათვის, საჭიროა ვიპოვოთ წინა პროცედურის დასრულების აღსანიშნავი ფაილი
        //და დავადგინოთ მისი შესრულების თარიღი.
        //თუ ეს თარიღი მიმდინარე პერიოდშია, მაშინ პროცედურა აღარ უნდა შესრულდეს
        //ProcLogFile procLogFile = new ProcLogFile($"UpdateStatistics_{serverName}_{dbname}_",
        //  (EPeriodTypes)_dpspRow.mdpspPeriodType, _dpspRow.mdpspStartAt);
        //if (procLogFile.HaveCurrentPeriodFile())
        //{
        //  Loger.Instance.LogMessage($"{serverName}_{dbname} Update Statistics already had executed in this period");
        //  continue;
        //}

        //DbManager dbm = _procDb.GetDbManager(0, dbname);
        //_bp.CurrentActivity = $"{serverName}_{dbname} Update Statistics...";

        try
        {
            var tableNames = await GetDatabaseTableNames();
            //_bp.SubLength = tableNames.Length;
            //_bp.SubCounted = 0;
            foreach (var strTableName in tableNames)
                //if (_bp.CancelationPending())
                //  return;

                await UpdateStatisticsForOneTable(strTableName);
            //_bp.SubCounted++;
            //_bp.SubLength = 0;
            //_bp.SubCounted = 0;
            //სამუშაო ფოლდერში შეიქმნას ფაილი, რომელიც იქნება იმის აღმნიშვნელი, რომ ეს პროცესი შესრულდა და წარმატებით დასრულდა.
            //ფაილის სახელი უნდა შედგებოდეს პროცედურის სახელისაგან თარიღისა და დროისაგან (როცა პროცესი დასრულდა)
            //ასევე სერვერის სახელი და ბაზის სახელი.
            //გაფართოება log
            //procLogFile.CreateNow("Ok");
            ////ასევე წაიშალოს ანალოგიური პროცესის მიერ წინათ შექმნილი ფაილები
            //procLogFile.DeleteOldFiles();
            //Loger.Instance.LogMessage("Ok");
        }
        catch (Exception ex)
        {
            StShared.WriteException(ex, $"{serverName}_{databaseName} Error in Update Statistics", UseConsole,
                Logger);
            return false;
        }
        //finally
        //{
        //  dbm.Close();
        //}
        //_bp.Counted++;


        //_bp.Length = 0;
        //_bp.Counted = 0;

        return true;
    }


    private async Task<string?> ServerName()
    {
        const string queryString = @"SELECT @@servername";

        var dbm = GetDbManager();
        if (dbm is null)
        {
            Logger.LogError("Cannot create Database connection");
            return null;
        }

        try
        {
            dbm.Open();
            return await dbm.ExecuteScalarAsync<string>(queryString);
        }
        catch (Exception ex)
        {
            StShared.WriteException(ex, UseConsole, Logger);
            return null;
        }
        finally
        {
            dbm.Close();
        }
    }
}