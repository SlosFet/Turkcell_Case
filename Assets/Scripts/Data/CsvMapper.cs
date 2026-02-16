using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

public static class CsvMapper
{
    public static List<T> MapRows<T>(string csvText) where T : new()
    {
        var result = new List<T>();
        if (string.IsNullOrWhiteSpace(csvText))
        {
            return result;
        }

        var rows = ParseCsv(csvText);
        if (rows.Count <= 1)
        {
            return result;
        }

        var headers = rows[0];
        var fieldMap = BuildFieldMap(typeof(T));
        var headerToField = new FieldInfo[headers.Length];

        for (var i = 0; i < headers.Length; i++)
        {
            fieldMap.TryGetValue(Normalize(headers[i]), out headerToField[i]);
        }

        for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            if (IsEmptyRow(row))
            {
                continue;
            }

            var item = new T();
            for (var col = 0; col < headers.Length && col < row.Length; col++)
            {
                var field = headerToField[col];
                if (field == null)
                {
                    continue;
                }

                var parsed = ConvertValue(row[col], field.FieldType);
                field.SetValue(item, parsed);
            }

            result.Add(item);
        }

        return result;
    }

    private static Dictionary<string, FieldInfo> BuildFieldMap(Type type)
    {
        var map = new Dictionary<string, FieldInfo>(StringComparer.OrdinalIgnoreCase);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields)
        {
            map[Normalize(field.Name)] = field;
        }

        return map;
    }

    private static object ConvertValue(string rawValue, Type targetType)
    {
        var value = rawValue?.Trim() ?? string.Empty;

        if (targetType == typeof(string))
        {
            return value;
        }

        if (targetType == typeof(int))
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue) ? intValue : 0;
        }

        if (targetType == typeof(long))
        {
            return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue) ? longValue : 0L;
        }

        if (targetType == typeof(float))
        {
            return float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var floatValue) ? floatValue : 0f;
        }

        if (targetType == typeof(double))
        {
            return double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var doubleValue) ? doubleValue : 0d;
        }

        if (targetType == typeof(bool))
        {
            if (bool.TryParse(value, out var boolValue))
            {
                return boolValue;
            }

            if (value == "1")
            {
                return true;
            }

            if (value == "0")
            {
                return false;
            }

            return false;
        }

        return Activator.CreateInstance(targetType);
    }

    private static List<string[]> ParseCsv(string csvText)
    {
        var rows = new List<string[]>();
        var currentRow = new List<string>();
        var currentCell = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < csvText.Length; i++)
        {
            var c = csvText[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < csvText.Length && csvText[i + 1] == '"')
                {
                    currentCell.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (c == ',' && !inQuotes)
            {
                currentRow.Add(currentCell.ToString());
                currentCell.Clear();
                continue;
            }

            if ((c == '\n' || c == '\r') && !inQuotes)
            {
                if (c == '\r' && i + 1 < csvText.Length && csvText[i + 1] == '\n')
                {
                    i++;
                }

                currentRow.Add(currentCell.ToString());
                currentCell.Clear();
                rows.Add(currentRow.ToArray());
                currentRow.Clear();
                continue;
            }

            currentCell.Append(c);
        }

        if (currentCell.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(currentCell.ToString());
            rows.Add(currentRow.ToArray());
        }

        return rows;
    }

    private static bool IsEmptyRow(string[] row)
    {
        for (var i = 0; i < row.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(row[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static string Normalize(string value)
    {
        return (value ?? string.Empty).Replace("_", string.Empty).Replace("-", string.Empty).Trim().ToLowerInvariant();
    }
}
