using System;
using System.Collections.Generic;
using System.Data;

namespace DbTools;

public sealed class Converters
{
    private static Converters? _pInstance;
    private static readonly object SyncRoot = new();
    private readonly Dictionary<DbType, Type> _dbTypeToTypeMap;

    private readonly Dictionary<Type, DbType> _typeToDbTypeMap;

    private Converters()
    {
        _typeToDbTypeMap = PrepareTypeToDbTypeConverter();
        _dbTypeToTypeMap = PrepareDbTypeToTypeConverter();
    }

    public static Converters Instance
    {
        get
        {
            if (_pInstance != null)
                return _pInstance;
            lock (SyncRoot) //thread safe singleton
            {
                _pInstance ??= new Converters();
            }

            return _pInstance;
        }
    }

    public DbType GetDbTypeFromType(Type type)
    {
        return _typeToDbTypeMap[type];
    }

    public Type GetTypeFromDbTypeName(string dbTypeName)
    {
        return GetTypeFromDbType(GetDbTypeByTypeName(dbTypeName));
    }

    private Type GetTypeFromDbType(DbType dbType)
    {
        return _dbTypeToTypeMap[dbType];
    }

    public DbType GetDbTypeByTypeName(string baseTypeName)
    {
        return Enum.TryParse(baseTypeName, true, out DbType toRet) ? toRet : DbType.Object;
    }


    private Dictionary<DbType, Type> PrepareDbTypeToTypeConverter()
    {
        var dbTypeToTypeMap = new Dictionary<DbType, Type>
        {
            { DbType.Byte, typeof(byte) },
            { DbType.SByte, typeof(sbyte) },
            { DbType.Int16, typeof(short) },
            { DbType.UInt16, typeof(ushort) },
            { DbType.Int32, typeof(int) },
            { DbType.UInt32, typeof(uint) },
            { DbType.Int64, typeof(long) },
            { DbType.UInt64, typeof(ulong) },
            { DbType.Single, typeof(float) },
            { DbType.Double, typeof(double) },
            { DbType.Decimal, typeof(decimal) },
            { DbType.Boolean, typeof(bool) },
            { DbType.String, typeof(string) },
            { DbType.StringFixedLength, typeof(char) },
            { DbType.Guid, typeof(Guid) },
            { DbType.DateTime, typeof(DateTime) },
            { DbType.DateTimeOffset, typeof(DateTimeOffset) },
            { DbType.Binary, typeof(byte[]) },
            { DbType.Object, typeof(object) }
        };
        return dbTypeToTypeMap;
    }

    private Dictionary<Type, DbType> PrepareTypeToDbTypeConverter()
    {
        var typeToDbTypeMap = new Dictionary<Type, DbType>
        {
            { typeof(byte), DbType.Byte },
            { typeof(sbyte), DbType.SByte },
            { typeof(short), DbType.Int16 },
            { typeof(ushort), DbType.UInt16 },
            { typeof(int), DbType.Int32 },
            { typeof(uint), DbType.UInt32 },
            { typeof(long), DbType.Int64 },
            { typeof(ulong), DbType.UInt64 },
            { typeof(float), DbType.Single },
            { typeof(double), DbType.Double },
            { typeof(decimal), DbType.Decimal },
            { typeof(bool), DbType.Boolean },
            { typeof(string), DbType.String },
            { typeof(char), DbType.StringFixedLength },
            { typeof(Guid), DbType.Guid },
            { typeof(DateTime), DbType.DateTime },
            { typeof(DateTimeOffset), DbType.DateTimeOffset },
            { typeof(byte[]), DbType.Binary },
            { typeof(byte?), DbType.Byte },
            { typeof(sbyte?), DbType.SByte },
            { typeof(short?), DbType.Int16 },
            { typeof(ushort?), DbType.UInt16 },
            { typeof(int?), DbType.Int32 },
            { typeof(uint?), DbType.UInt32 },
            { typeof(long?), DbType.Int64 },
            { typeof(ulong?), DbType.UInt64 },
            { typeof(float?), DbType.Single },
            { typeof(double?), DbType.Double },
            { typeof(decimal?), DbType.Decimal },
            { typeof(bool?), DbType.Boolean },
            { typeof(char?), DbType.StringFixedLength },
            { typeof(Guid?), DbType.Guid },
            { typeof(DateTime?), DbType.DateTime },
            { typeof(DateTimeOffset?), DbType.DateTimeOffset },
            //{typeof(System.Data.linq.Binary), DbType.Binary},
            { typeof(object), DbType.Object }
        };
        return typeToDbTypeMap;
    }
}