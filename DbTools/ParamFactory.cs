using System;
using System.Data;

namespace DbTools;

public static class ParamFactory
{
    public static DataParameter CreateParameter(string name)
    {
        var p = new DataParameter(DBNull.Value, name);
        return p;
    }

    public static DataParameter CreateParameter(string name, DBNull value)
    {
        var p = new DataParameter(value, name);
        return p;
    }

    //public static DataParameter CreateGuidListParameter(string name, IEnumerable<Guid> value)
    //{
    //  DataParameter p = new DataParameter(true)
    //  {
    //    Value = value,
    //    ParameterName = name
    //  };
    //  return p;
    //}

    //public static DataParameter CreateParameter(string name, IEnumerable<Guid> values)
    //{
    //  DataParameter p = new DataParameter(true)
    //  {
    //    ParameterName = name,
    //    Value = values
    //  };
    //  return p;
    //}

    public static DataParameter CreateParameter(string name, Type type, object? value, bool checkDefault = true)
    {
        var p = new DataParameter(Converters.Instance.GetDbTypeFromType(type), name);
        if (checkDefault && value == null)
            p.Value = DBNull.Value;
        else
            p.Value = value;
        return p;
    }

    public static DataParameter CreateParameter<T>(string name, T value, bool checkDefault = false)
    {
        var p = new DataParameter(Converters.Instance.GetDbTypeFromType(typeof(T)), name);
        if (checkDefault && Equals(value, default(T)))
            p.Value = DBNull.Value;
        else
            p.Value = value;
        p.ParameterName = name;
        return p;
    }

    //public static DataParameter CreateParameterByBaseTypeName(string name, string baseTypeName)
    //{
    //  SqlDbType sdbt;
    //  if (!Enum.TryParse(baseTypeName, true, out sdbt))
    //    throw new Exception("Invalid Base Type Name");
    //  DataParameter p = new DataParameter
    //  {
    //    DbType = SqlConverters.Instance.GetDbTypeFromSqlDbType(sdbt),
    //    ParameterName = name
    //  };
    //  return p;
    //}

    //მონაცემთა ბაზის ტიპის მიხედვით პარამეტრის ობიექტირ მიღება
    //მონაცემის ტიპის, სახელის მითითებით (მნიშვნელობის გარეშე)
    public static DataParameter CreateParameter(string name, DbType type)
    {
        var p = new DataParameter(type, name);
        return p;
    }

    //მონაცემთა ბაზის ტიპის მიხედვით პარამეტრის ობიექტირ მიღება
    //მონაცემის ტიპის, სახელის  და სიგრძის მითითებით (მნიშვნელობის გარეშე)
    public static DataParameter CreateParameter(string name, DbType type, int length)
    {
        var p = new DataParameter(type, name, length);
        return p;
    }

    public static DataParameter CreateParameter(string name, DbType type, int length, ParameterDirection direction)
    {
        var p = new DataParameter(type, name, length, direction);
        return p;
    }

    public static DataParameter CreateParameter(string name, DbType type, string sourceColumn)
    {
        var p = new DataParameter(type, name, sourceColumn);
        return p;
    }
}