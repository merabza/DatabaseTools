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
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace DatabaseTools.SqLiteDbTools;

public sealed class SqLiteDbClient : DbClient
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public SqLiteDbClient(ILogger logger, DbConnectionStringBuilder conStrBuilder, DbKit dbKit, bool useConsole,
        IMessagesDataManager? messagesDataManager = null, string? userName = null) : base(logger, conStrBuilder, dbKit,
        useConsole, messagesDataManager, userName)
    {
    }

    public override Task<Option<Error[]>> BackupDatabase(string databaseName, string backupFilename, string backupName,
        EBackupType backupType, bool compression, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<Error[]>> CheckRepairDatabase(string databaseName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<List<DatabaseInfoModel>, Error[]>> GetDatabaseInfos(
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<DbServerInfo, Error[]>> GetDbServerInfo(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<List<RestoreFileModel>, Error[]>> GetRestoreFiles(string backupFileFullName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<string, Error[]>> HostPlatform(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<bool, Error[]>> IsDatabaseExists(string databaseName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<bool, Error[]>> IsServerAllowsCompression(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<bool, Error[]>> IsServerLocal(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<Error[]>> RecompileProcedures(string databaseName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<Error[]>> RestoreDatabase(string databaseName, string backupFileFullName,
        List<RestoreFileModel>? files, string dataFolderName, string dataLogFolderName, string dirSeparator,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<Error[]>> SetDefaultFolders(string defBackupFolder, string defDataFolder,
        string defLogFolder, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<Error[]>> ChangeDatabaseRecoveryModel(string databaseName,
        EDatabaseRecoveryModel databaseRecoveryModel, CancellationToken cancellationToken)
    {
        return Task.FromResult<Option<Error[]>>(null);
    }

    //public override Task<OneOf<Dictionary<string, DatabaseFoldersSet>, Error[]>> GetDatabaseFoldersSets(CancellationToken cancellationToken = default)
    //{
    //    throw new NotImplementedException();
    //}

    public override Task<Option<Error[]>> TestConnection(bool withDatabase,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<Error[]>> UpdateStatistics(string databaseName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<Error[]>> VerifyBackup(string databaseName, string backupFilename,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
