using System;
using System.Globalization;
using MySqlConnector;

namespace DatabaseTools.MariaDbTools;

//MariaDB-ის კავშირის სტრიქონის აწყობა; სერვერის მისამართის ფორმატია "host" ან "host:port"
public static class MariaDbConnectionStringFactory
{
    public static MySqlConnectionStringBuilder Create(string? serverAddress, string? userName, string? password,
        string? databaseName, int connectionTimeOut)
    {
        var builder = new MySqlConnectionStringBuilder();
        if (!string.IsNullOrWhiteSpace(serverAddress))
        {
            (string server, uint? port) = SplitServerAddress(serverAddress.Trim());
            builder.Server = server;
            if (port is not null)
            {
                builder.Port = port.Value;
            }
        }

        if (!string.IsNullOrWhiteSpace(userName))
        {
            builder.UserID = userName;
        }

        if (!string.IsNullOrEmpty(password))
        {
            builder.Password = password;
        }

        if (!string.IsNullOrWhiteSpace(databaseName))
        {
            builder.Database = databaseName;
        }

        //MySqlConnector-ისთვის 0 უსასრულო ლოდინია — ასეთ დროს ბიბლიოთეკის ნაგულისხმევი 15 წამი რჩება
        if (connectionTimeOut > 0)
        {
            builder.ConnectionTimeout = (uint)connectionTimeOut;
        }

        return builder;
    }

    //პორტი გამოიყოფა მხოლოდ ერთი ორწერტილის დროს, რომ IPv6 მისამართი ("::1") პორტად არ გაიჭრას
    private static (string Server, uint? Port) SplitServerAddress(string serverAddress)
    {
        int colonIndex = serverAddress.IndexOf(':', StringComparison.Ordinal);
        if (colonIndex <= 0 || colonIndex != serverAddress.LastIndexOf(':') || !uint.TryParse(
                serverAddress.AsSpan(colonIndex + 1), NumberStyles.None, CultureInfo.InvariantCulture, out uint port))
        {
            return (serverAddress, null);
        }

        return (serverAddress[..colonIndex], port);
    }
}
