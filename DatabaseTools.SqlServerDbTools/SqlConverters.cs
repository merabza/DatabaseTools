using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace DatabaseTools.SqlServerDbTools;

public sealed class SqlConverters
{
    private static SqlConverters? _pInstance;
    private static readonly Lock SyncRoot = new();
    private readonly Dictionary<SqlDbType, DbType> _sqlDbTypeToDbTypeMap;

    private readonly Dictionary<SqlDbType, Type> _sqlDbTypeToTypeMap;
    private readonly Dictionary<Type, SqlDbType> _typeToSqlDbTypeMap;

    private SqlConverters()
    {
        _sqlDbTypeToTypeMap = PrepareSqlDbTypeToTypeMapConverter();
        _typeToSqlDbTypeMap = PrepareTypeToSqlDbTypeMapConverter();
        _sqlDbTypeToDbTypeMap = PrepareSqlDbTypeToDbTypeMapConverter();
    }

    public static SqlConverters Instance
    {
        get
        {
            if (_pInstance != null)
            {
                return _pInstance;
            }

            lock (SyncRoot) //thread safe singleton
            {
                _pInstance = new SqlConverters();
            }

            return _pInstance;
        }
    }

    public DbType GetDbTypeFromSqlDbType(SqlDbType sqlDbType)
    {
        return _sqlDbTypeToDbTypeMap[sqlDbType];
    }

    public SqlDbType GetSqlDbTypeFromType(Type type)
    {
        return _typeToSqlDbTypeMap[type];
    }

    private Type GetTypeFromSqlDbType(SqlDbType sqlDbType)
    {
        return _sqlDbTypeToTypeMap[sqlDbType];
    }

    private static SqlDbType GetSqlDbTypeBySqlDbTypeName(string sqlDbTypeName)
    {
        return Enum.TryParse(sqlDbTypeName, true, out SqlDbType toRet) ? toRet : SqlDbType.Variant;
    }

    public Type GetTypeFromSqlDbTypeName(string sqlDbTypeName)
    {
        return GetTypeFromSqlDbType(GetSqlDbTypeBySqlDbTypeName(sqlDbTypeName));
    }

    private Dictionary<SqlDbType, DbType> PrepareSqlDbTypeToDbTypeMapConverter()
    {
        return new Dictionary<SqlDbType, DbType>
        {
            { SqlDbType.BigInt, DbType.Int64 },
            { SqlDbType.VarBinary, DbType.Binary },
            { SqlDbType.Bit, DbType.Boolean },
            { SqlDbType.Char, DbType.String },
            { SqlDbType.Date, DbType.Date },
            { SqlDbType.DateTime, DbType.DateTime },
            { SqlDbType.DateTime2, DbType.DateTime2 },
            { SqlDbType.DateTimeOffset, DbType.DateTimeOffset },
            { SqlDbType.Decimal, DbType.Decimal },
            { SqlDbType.Float, DbType.Double },
            { SqlDbType.Binary, DbType.Binary },
            { SqlDbType.Int, DbType.Int32 },
            { SqlDbType.Money, DbType.Decimal },
            { SqlDbType.NChar, DbType.StringFixedLength },
            { SqlDbType.NText, DbType.String },
            { SqlDbType.NVarChar, DbType.String },
            { SqlDbType.Real, DbType.Single },
            { SqlDbType.Timestamp, DbType.Binary },
            { SqlDbType.SmallInt, DbType.Int16 },
            { SqlDbType.SmallMoney, DbType.Decimal },
            { SqlDbType.Variant, DbType.Object },
            { SqlDbType.Text, DbType.String },
            { SqlDbType.Time, DbType.Time },
            { SqlDbType.TinyInt, DbType.Byte },
            { SqlDbType.UniqueIdentifier, DbType.Guid },
            { SqlDbType.VarChar, DbType.String },
            { SqlDbType.Xml, DbType.Xml }
        };
    }

    private Dictionary<Type, SqlDbType> PrepareTypeToSqlDbTypeMapConverter()
    {
        return new Dictionary<Type, SqlDbType>
        {
            { typeof(long), SqlDbType.BigInt },
            { typeof(byte[]), SqlDbType.Binary },
            { typeof(bool), SqlDbType.Bit },
            //{typeof(string), SqlDbType.Char},
            { typeof(DateTime), SqlDbType.DateTime },
            { typeof(DateTimeOffset), SqlDbType.DateTimeOffset },
            //{typeof(decimal), SqlDbType.Decimal},
            { typeof(double), SqlDbType.Float },
            { typeof(int), SqlDbType.Int },
            { typeof(decimal), SqlDbType.Money },
            { typeof(string), SqlDbType.NVarChar },
            { typeof(float), SqlDbType.Real },
            { typeof(short), SqlDbType.SmallInt },
            //{typeof(object), SqlDbType.Variant},
            { typeof(TimeSpan), SqlDbType.Time },
            { typeof(Guid), SqlDbType.UniqueIdentifier },
            { typeof(object), SqlDbType.Variant }
        };
    }

    private Dictionary<SqlDbType, Type> PrepareSqlDbTypeToTypeMapConverter()
    {
        return new Dictionary<SqlDbType, Type>
        {
            { SqlDbType.BigInt, typeof(long) },
            { SqlDbType.Binary, typeof(byte[]) },
            { SqlDbType.Bit, typeof(bool) },
            { SqlDbType.Char, typeof(string) },
            { SqlDbType.Date, typeof(DateTime) },
            { SqlDbType.DateTime, typeof(DateTime) },
            { SqlDbType.DateTime2, typeof(DateTime) },
            { SqlDbType.DateTimeOffset, typeof(DateTimeOffset) },
            { SqlDbType.Decimal, typeof(decimal) },
            { SqlDbType.Float, typeof(double) },
            { SqlDbType.Image, typeof(byte[]) },
            { SqlDbType.Int, typeof(int) },
            { SqlDbType.Money, typeof(decimal) },
            { SqlDbType.NChar, typeof(string) },
            { SqlDbType.NText, typeof(string) },
            { SqlDbType.NVarChar, typeof(string) },
            { SqlDbType.Real, typeof(float) },
            { SqlDbType.SmallDateTime, typeof(DateTime) },
            { SqlDbType.SmallInt, typeof(short) },
            { SqlDbType.SmallMoney, typeof(decimal) },
            { SqlDbType.Variant, typeof(object) },
            { SqlDbType.Text, typeof(string) },
            { SqlDbType.Time, typeof(TimeSpan) },
            { SqlDbType.TinyInt, typeof(byte) },
            { SqlDbType.UniqueIdentifier, typeof(Guid) },
            { SqlDbType.VarBinary, typeof(byte[]) },
            { SqlDbType.VarChar, typeof(string) },
            { SqlDbType.Timestamp, typeof(byte[]) }
        };
    }
}
