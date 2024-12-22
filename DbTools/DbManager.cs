using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DbTools;

public sealed class DbManager : IDisposable
{
    private readonly DbKit _kit;
    private IDataReader? _dataReader;
    private DbCommand? _dbCommand;
    private DbTransaction? _dbTransaction;
    private ParametersCollection? _parameters;


    private DbManager(DbKit kit, DbConnection dbConnection, string connectionString = "", int commandTimeout = 0,
        bool fireInfoMessageEventOnUserErrors = false)
    {
        _kit = kit;
        Connection = dbConnection;
        Connection.ConnectionString = connectionString;
        CommandTimeOut = commandTimeout;
        if (fireInfoMessageEventOnUserErrors)
            _kit.InfoMessage += DbKit_InfoMessage;
    }

    private int CommandTimeOut { get; }
    public DbConnection Connection { get; }

    public string ConnectionString => Connection.ConnectionString;
    public string Database => Connection.Database;


    public void Dispose()
    {
        Close();
        // Free other state (managed objects).
        if (_dbCommand != null)
        {
            _dbCommand.Dispose();
            _dbCommand = null;
        }

        if (_dataReader != null)
        {
            _dataReader.Dispose();
            _dataReader = null;
        }

        if (_dbTransaction != null)
        {
            _dbTransaction.Dispose();
            _dbTransaction = null;
        }

        Connection.Dispose();

        GC.Collect();
    }

    public event InfoMessageEventHandler? InfoMessage;

    public static DbManager? Create(DbKit kit, string connectionString = "", int commandTimeout = 0,
        bool fireInfoMessageEventOnUserErrors = false)
    {
        // ReSharper disable once using
        var dbConnection = kit.GetConnection(fireInfoMessageEventOnUserErrors);
        return dbConnection is null
            ? null
            // ReSharper disable once DisposableConstructor
            : new DbManager(kit, dbConnection, connectionString, commandTimeout, fireInfoMessageEventOnUserErrors);
    }

    private void DbKit_InfoMessage(object sender, InfoMessageEventArgs e)
    {
        InfoMessage?.Invoke(sender, e);
    }

    //კავშირი გახსნა თუ გახსნილი არ არის
    public void Open()
    {
        if (Connection.State != ConnectionState.Open)
            Connection.Open();
    }

    //პარამეტრების მასივიდან პარამეტრების მიერთება ბრძანებაზე
    private void AttachParameters()
    {
        if (_dbCommand == null || _parameters == null)
            return;
        foreach (var param in _parameters)
        {
            var p = _kit.GetParameter(param);
            if (p is null)
                continue;
            if (param.Direction is ParameterDirection.InputOutput or ParameterDirection.Input &&
                param.Value == null)
                p.Value = DBNull.Value;
            _dbCommand.Parameters.Add(p);
        }
    }

    private void PrepareCommand(string commandText, CommandType commandType)
    {
        _dbCommand = _kit.GetCommand();
        if (_dbCommand is null)
            throw new Exception("db command does not create");
        _dbCommand.CommandTimeout = CommandTimeOut;
        _dbCommand.Connection = Connection;
        _dbCommand.CommandType = commandType;
        _dbCommand.CommandText = commandText;

        // თუ ვიმყოფებით გახსნილ ტრანზაქციაში მივუთითოთ ის ბრძანებას
        if (_dbTransaction != null)
            _dbCommand.Transaction = _dbTransaction;

        //თუ გაგვაჩნია პარამეტრების მასივი, მივაერთოთ პარამეტრები ბრძანებაზე
        AttachParameters();
    }

    //ბრძანების მიერ დაბრუნებული პირველი ჩანაწერის პირველი ველის მნიშვნელობის მიღება
    public T? ExecuteScalar<T>(string commandText, T? defaultValue = default,
        CommandType commandType = CommandType.Text)
    {
        PrepareCommand(commandText, commandType);
        var retVal = ExecuteScalar();
        _dbCommand?.Parameters.Clear();
        if (retVal != null && retVal != DBNull.Value)
            return (T?)retVal;
        return defaultValue;
    }

    public async Task<T?> ExecuteScalarAsync<T>(string commandText, CancellationToken cancellationToken,
        T? defaultValue = default,
        CommandType commandType = CommandType.Text)
    {
        PrepareCommand(commandText, commandType);
        var retVal = await ExecuteScalarAsync(cancellationToken);
        _dbCommand?.Parameters.Clear();
        if (retVal is not null && retVal != DBNull.Value)
            return (T?)retVal;
        return defaultValue;
    }

    private object? ExecuteScalar()
    {
        return _dbCommand?.ExecuteScalar();
    }

    private async Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken = default)
    {
        if (_dbCommand is null)
            return null;
        return await _dbCommand.ExecuteScalarAsync(cancellationToken);
    }

    //მონაცემთა ბაზის ბრძანების შესრულება (ბრძანებისაგან არ ველოდებით მონაცემების დაბრუნებას, უბრალოდ უნდა შესრულდეს ბრძანება)
    public int ExecuteNonQuery(string commandText, CommandType commandType = CommandType.Text)
    {
        PrepareCommand(commandText, commandType);
        var retVal = ExecuteNonQuery();
        SavOutParameters();
        return retVal;
    }

    private void SavOutParameters()
    {
        if (_dbCommand is null || _parameters is null)
            return;
        foreach (var parameter in _dbCommand.Parameters.Cast<DbParameter>()
                     .Where(s => s.Direction != ParameterDirection.Input))
            _parameters[parameter.ParameterName].Value = parameter.Value;
        _dbCommand.Parameters.Clear();
    }

    public async Task<int> ExecuteNonQueryAsync(string commandText, CancellationToken cancellationToken,
        CommandType commandType = CommandType.Text)
    {
        PrepareCommand(commandText, commandType);
        var retVal = await ExecuteNonQueryAsync(cancellationToken);
        SavOutParameters();
        return retVal;
    }

    //ეს არის ის ნაწილი, რომელიც რეალურად ბაზასთან ურთიერთობს, ამიტომ საჭიროა მისი იზოლირება
    private int ExecuteNonQuery()
    {
        return _dbCommand?.ExecuteNonQuery() ?? 0;
    }

    private async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default)
    {
        if (_dbCommand is null)
            return 0;
        return await _dbCommand.ExecuteNonQueryAsync(cancellationToken);
    }


    //ჩანაწერების წამკითხველის გაშვება მითითებული ტიპის მითითებული ბრძანებისათვის
    public IDataReader ExecuteReader(string commandText, CommandType commandType = CommandType.Text)
    {
        PrepareCommand(commandText, commandType);
        _dataReader = ExecuteReader();
        _dbCommand?.Parameters.Clear();
        return _dataReader;
    }

    //ჩანაწერების წამკითხველის გაშვება მითითებული ტიპის მითითებული ბრძანებისათვის
    public async Task<IDataReader> ExecuteReaderAsync(string commandText, CancellationToken cancellationToken,
        CommandType commandType = CommandType.Text)
    {
        PrepareCommand(commandText, commandType);
        _dataReader = await ExecuteReaderAsync(cancellationToken);
        _dbCommand?.Parameters.Clear();
        return _dataReader;
    }

    private async Task<IDataReader> ExecuteReaderAsync(CancellationToken cancellationToken = default)
    {
        if (_dbCommand is null)
            throw new InvalidOperationException();
        return await _dbCommand.ExecuteReaderAsync(cancellationToken);
    }


    private IDataReader ExecuteReader()
    {
        if (_dbCommand is null)
            throw new InvalidOperationException();
        return _dbCommand.ExecuteReader();
    }


    //კავშირის დახურვა თუ დახურული არ არის
    public void Close()
    {
        if (Connection.State != ConnectionState.Closed)
            Connection.Close();
    }

    public void AddParameter<TS>(string name, TS value, bool checkDefault = false)
    {
        AddParameter(ParamFabric.CreateParameter(name, value, checkDefault));
    }

    private void AddParameter(DataParameter iDbDataParameter)
    {
        _parameters ??= new ParametersCollection();
        _parameters.AddParameter(iDbDataParameter);

        //აქ ამის გაკეთება არ შეიძლება, რადგან მერე პარამეტრების რეალურად მომზადების დროს პრობლემაა ითვლება რომ ეს პარამეტრი უკვე გამოყენებულია და ხელი გვეშლება
        //if (_dbCommand != null)
        //{
        //  _dbCommand.Parameters.Add(iDbDataParameter);
        //}
    }

    public void ClearParameters()
    {
        if (_parameters == null)
            return;

        _parameters.Clear();
        _parameters = null;
    }
}