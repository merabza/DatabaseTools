using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DbTools;

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
        var atLeastOneAdded = false;
        foreach (var param in _dbParameters)
        {
            if (atLeastOneAdded)
                sb.Append(';');
            sb.Append(param.ParameterName);
            sb.Append('=');
            sb.Append(param.Value);
            atLeastOneAdded = true;
        }

        return sb.ToString();
    }

    public Tc? GetParameterValue<Tc>(string parameterName)
    {
        if (!_dbParametersByNames.TryGetValue(parameterName, out var value))
            throw new Exception("Invalid parameter Name");
        if (value.Value is not null && value.Value != DBNull.Value)
            return (Tc?)value.Value;
        return default;
    }

    public bool ContainsKey(string parameterName)
    {
        return _dbParametersByNames.ContainsKey(parameterName);
    }

    private static DataParameter CreateParameter<Ts>(string name, Ts value, bool checkDefault = false)
    {
        var p = new DataParameter(Converters.Instance.GetDbTypeFromType(typeof(Ts)), name);
        if (checkDefault && Equals(value, default(Ts)))
            p.Value = DBNull.Value;
        else
            p.Value = value;
        return p;
    }

    public static DataParameter CreateParameter(string name, DbType type, int length, string sourceColumn)
    {
        var p = new DataParameter(type, name, length, sourceColumn);
        return p;
    }

    public void AddParameter<Ts>(string name, Ts value)
    {
        AddParameter(CreateParameter(name, value));
    }

    public void AddParameter(DataParameter parameter)
    {
        if (_dbParameters.Contains(parameter))
            throw new Exception("Parameter is already exists in collection");
        _dbParameters.Add(parameter);
        var parameterName = parameter.ParameterName;
        if (!string.IsNullOrWhiteSpace(parameterName))
            _dbParametersByNames.TryAdd(parameter.ParameterName, parameter);
    }

    public void Clear()
    {
        _dbParameters.Clear();
        _dbParametersByNames.Clear();
    }
}