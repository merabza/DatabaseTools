using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using DatabaseTools.DbTools;
using DatabaseTools.DbTools.Models;
using Microsoft.Extensions.Logging;
using SystemTools.SharedKernel;

namespace DatabaseTools.OleDbTools;

public sealed class OleDbClient : DbClient
{
    
    public OleDbClient(ILogger logger, DbConnectionStringBuilder conStrBuilder, DbKit dbKit, bool useConsole) : base(
        logger, conStrBuilder, dbKit, useConsole)
    {
    }

    public override Task<Result> BackupDatabase(string databaseName, string backupFilename, string backupName,
        EBackupType backupType, bool compression, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Result> CheckRepairDatabase(string databaseName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Result<List<DatabaseInfoModel>>> GetDatabaseInfos(
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Result<DbServerInfo>> GetDbServerInfo(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Result<List<RestoreFileModel>>> GetRestoreFiles(string backupFileFullName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Result<string>> HostPlatform(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Result<bool>> IsDatabaseExists(string databaseName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Result<bool>> IsServerAllowsCompression(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Result<bool>> IsServerLocal(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Result> RecompileProcedures(string databaseName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Result> RestoreDatabase(string databaseName, string backupFileFullName,
        List<RestoreFileModel>? files, string dataFolderName, string dataLogFolderName, string dirSeparator,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Result> SetDefaultFolders(string defBackupFolder, string defDataFolder, string defLogFolder,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Result> ChangeDatabaseRecoveryModel(string databaseName,
        EDatabaseRecoveryModel databaseRecoveryModel, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    //public override Task<OneOf<Dictionary<string, DatabaseFoldersSet>, ErrorOmd[]>> GetDatabaseFoldersSets(CancellationToken cancellationToken = default)
    //{
    //    throw new NotImplementedException();
    //}

    public override Task<Result> TestConnection(bool withDatabase, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Result> UpdateStatistics(string databaseName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Result> VerifyBackup(string databaseName, string backupFilename,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
