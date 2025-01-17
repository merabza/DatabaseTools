using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using DbTools;
using DbTools.Models;
using LanguageExt;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemToolsShared;
using SystemToolsShared.Errors;

namespace SqLiteDbTools;

public class SqLiteDbClient : DbClient
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public SqLiteDbClient(ILogger logger, DbConnectionStringBuilder conStrBuilder, DbKit dbKit, bool useConsole,
        IMessagesDataManager? messagesDataManager = null, string? userName = null) : base(logger, conStrBuilder, dbKit,
        useConsole, messagesDataManager, userName)
    {
    }

    public override Task<Option<IEnumerable<Err>>> BackupDatabase(string databaseName, string backupFilename,
        string backupName, EBackupType backupType, bool compression, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<IEnumerable<Err>>> CheckRepairDatabase(string databaseName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<List<DatabaseInfoModel>, IEnumerable<Err>>> GetDatabaseInfos(
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<DbServerInfo, IEnumerable<Err>>> GetDbServerInfo(
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<List<RestoreFileModel>, IEnumerable<Err>>> GetRestoreFiles(string backupFileFullName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<string, IEnumerable<Err>>> HostPlatform(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<bool, IEnumerable<Err>>> IsDatabaseExists(string databaseName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<bool, IEnumerable<Err>>> IsServerAllowsCompression(
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<OneOf<bool, IEnumerable<Err>>> IsServerLocal(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<IEnumerable<Err>>> RecompileProcedures(string databaseName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<IEnumerable<Err>>> RestoreDatabase(string databaseName, string backupFileFullName,
        List<RestoreFileModel>? files, string dataFolderName, string dataLogFolderName, string dirSeparator,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<IEnumerable<Err>>> SetDefaultFolders(string defBackupFolder, string defDataFolder,
        string defLogFolder, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    //public override Task<OneOf<Dictionary<string, DatabaseFoldersSet>, IEnumerable<Err>>> GetDatabaseFoldersSets(CancellationToken cancellationToken = default)
    //{
    //    throw new NotImplementedException();
    //}

    public override Task<Option<IEnumerable<Err>>> TestConnection(bool withDatabase,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<IEnumerable<Err>>> UpdateStatistics(string databaseName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Option<IEnumerable<Err>>> VerifyBackup(string databaseName, string backupFilename,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}