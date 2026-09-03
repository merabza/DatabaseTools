using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using DatabaseTools.DbTools;
using DatabaseTools.DbTools.Models;
using LanguageExt;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace DatabaseTools.OleDbTools;

public sealed class OleDbClient : DbClient
{
    
    public OleDbClient(ILogger logger, DbConnectionStringBuilder conStrBuilder, DbKit dbKit, bool useConsole) : base(
        logger, conStrBuilder, dbKit, useConsole)
    {
    }

    public override Task<Option<ErrorOmd[]>> BackupDatabase(string databaseName, string backupFilename,
        string backupName, EBackupType backupType, bool compression, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<ErrorOmd[]>> CheckRepairDatabase(string databaseName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<List<DatabaseInfoModel>, ErrorOmd[]>> GetDatabaseInfos(
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<DbServerInfo, ErrorOmd[]>> GetDbServerInfo(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<List<RestoreFileModel>, ErrorOmd[]>> GetRestoreFiles(string backupFileFullName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<string, ErrorOmd[]>> HostPlatform(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<bool, ErrorOmd[]>> IsDatabaseExists(string databaseName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<bool, ErrorOmd[]>> IsServerAllowsCompression(
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<bool, ErrorOmd[]>> IsServerLocal(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<ErrorOmd[]>> RecompileProcedures(string databaseName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<ErrorOmd[]>> RestoreDatabase(string databaseName, string backupFileFullName,
        List<RestoreFileModel>? files, string dataFolderName, string dataLogFolderName, string dirSeparator,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<ErrorOmd[]>> SetDefaultFolders(string defBackupFolder, string defDataFolder,
        string defLogFolder, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<ErrorOmd[]>> ChangeDatabaseRecoveryModel(string databaseName,
        EDatabaseRecoveryModel databaseRecoveryModel, CancellationToken cancellationToken)
    {
        return Task.FromResult<Option<ErrorOmd[]>>(null);
    }

    //public override Task<OneOf<Dictionary<string, DatabaseFoldersSet>, ErrorOmd[]>> GetDatabaseFoldersSets(CancellationToken cancellationToken = default)
    //{
    //    throw new NotImplementedException();
    //}

    public override Task<Option<ErrorOmd[]>> TestConnection(bool withDatabase,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<ErrorOmd[]>> UpdateStatistics(string databaseName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<ErrorOmd[]>> VerifyBackup(string databaseName, string backupFilename,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
