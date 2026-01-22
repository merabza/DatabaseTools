using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DatabaseTools.DbTools;

public sealed class ParametersCollection : IEnumerable<DataParameter>
{
    private readonly List<DataParameter> _dbParameters;
    private readonly Dictionary<string, DataParameter> _dbParametersByNames;

    // ReSharper disable once ConvertConstructorToMemberInitializers
    public ParametersCollection()
    {
        _dbParameters = [];
        _dbParametersByNames = new Dictionary<string, DataParameter>();
    }

    public DataParameter this[string parameterName] => _dbParametersByNames[parameterName];

    public int Count => _dbParameters.Count;

    public IEnumerator<DataParameter> GetEnumerator()
    {
        return _dbParameters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        bool atLeastOneAdded = false;
        foreach (DataParameter param in _dbParameters)
        {
            if (atLeastOneAdded)
            {
                sb.Append(';');
            }

            sb.Append(param.ParameterName);
            sb.Append('=');
            sb.Append(param.Value);
            atLeastOneAdded = true;
        }

        return sb.ToString();
    }

    public T? GetParameterValue<T>(string parameterName)
    {
        if (!_dbParametersByNames.TryGetValue(parameterName, out DataParameter? value))
        {
            throw new Exception("Invalid parameter Name");
        }

        if (value.Value is not null && value.Value != DBNull.Value)
        {
            return (T?)value.Value;
        }

        return default;
    }

    public bool ContainsKey(string parameterName)
    {
        return _dbParametersByNames.ContainsKey(parameterName);
    }

    private static DataParameter CreateParameter<T>(string name, T value, bool checkDefault = false)
    {
        var p = new DataParameter(Converters.Instance.GetDbTypeFromType(typeof(T)), name)
        {
            Value = checkDefault && Equals(value, default(T)) ? DBNull.Value : value
        };

        return p;
    }

    public static DataParameter CreateParameter(string name, DbType type, int length, string sourceColumn)
    {
        var p = new DataParameter(type, name, length, sourceColumn);
        return p;
    }

    public void AddParameter<T>(string name, T value)
    {
        AddParameter(CreateParameter(name, value));
    }

    public void AddParameter(DataParameter parameter)
    {
        if (_dbParameters.Contains(parameter))
        {
            throw new Exception("Parameter is already exists in collection");
        }

        _dbParameters.Add(parameter);
        string parameterName = parameter.ParameterName;
        if (!string.IsNullOrWhiteSpace(parameterName))
        {
            _dbParametersByNames.TryAdd(parameter.ParameterName, parameter);
        }
    }

    public void Clear()
    {
        _dbParameters.Clear();
        _dbParametersByNames.Clear();
    }
}
