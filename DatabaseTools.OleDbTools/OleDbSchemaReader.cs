using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;

namespace DatabaseTools.OleDbTools;

//MS Access (ACE OLE DB) ბაზის ცხრილებისა და სვეტების სქემის წამკითხავი — Access-ს INFORMATION_SCHEMA არ აქვს,
//ამიტომ გამოიყენება GetOleDbSchemaTable; სვეტები ბრუნდება ORDINAL_POSITION-ის მიხედვით დალაგებული
public static class OleDbSchemaReader
{
#pragma warning disable CA1416
    private static readonly object?[] UserTablesRestrictions = [null, null, null, "TABLE"];

    public static List<(string TableName, List<string> Columns)> ReadTablesAndColumns(string connectionString)
    {
        // ReSharper disable once DisposableConstructor
        // ReSharper disable once using
        using var connection = new OleDbConnection(connectionString);
        connection.Open();

        //მხოლოდ მომხმარებლის ცხრილები (TABLE_TYPE='TABLE') — სისტემური, ბმული ცხრილები და შენახული მოთხოვნები გამოირიცხება
        // ReSharper disable once using
        using DataTable? tablesSchema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, UserTablesRestrictions);

        var tableNames = new List<string>();
        if (tablesSchema is not null)
        {
            //MSys* სისტემური და ~* დროებითი ცხრილები TABLE ტიპითაც შეიძლება დაბრუნდეს
            tableNames.AddRange(tablesSchema.Rows.Cast<DataRow>().Select(r => (string)r["TABLE_NAME"]).Where(n =>
                !n.StartsWith("MSys", StringComparison.OrdinalIgnoreCase) && !n.StartsWith('~')));
        }

        var result = new List<(string TableName, List<string> Columns)>();
        foreach (string tableName in tableNames.OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
        {
            //Columns rowset-ის შეზღუდვების რიგია: TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME
            // ReSharper disable once using
            using DataTable? columnsSchema =
                connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, [null, null, tableName, null]);

            var columns = new List<string>();
            if (columnsSchema is not null)
            {
                //ACE სვეტებს ანბანურად აბრუნებს — ცხრილის რეალური თანმიმდევრობა ORDINAL_POSITION-შია
                columns.AddRange(columnsSchema.Rows.Cast<DataRow>()
                    .OrderBy(r => Convert.ToInt64(r["ORDINAL_POSITION"], CultureInfo.InvariantCulture))
                    .Select(r => (string)r["COLUMN_NAME"]));
            }

            result.Add((tableName, columns));
        }

        return result;
    }
#pragma warning restore CA1416
}
