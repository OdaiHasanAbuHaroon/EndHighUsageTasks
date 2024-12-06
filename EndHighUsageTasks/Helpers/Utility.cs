using Newtonsoft.Json;
using System.Data;
using System.Reflection;
using System.Text;

namespace EndHighUsageTasks.Helpers;

/// <summary>
/// A collection of common utility methods for serialization, data conversion, parsing, file handling, and date manipulation.
/// This utility class provides commonly used functions to streamline coding in projects.
/// </summary>
public static class Utility
{
    #region JSON Serialization and Deserialization

    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    /// <typeparam name="T">Type of the input object.</typeparam>
    /// <param name="input">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string JsonSerialize<T>(T input)
    {
        return JsonConvert.SerializeObject(input);
    }

    /// <summary>
    /// Deserializes a JSON string to an object of type T.
    /// </summary>
    /// <typeparam name="T">Type of the output object.</typeparam>
    /// <param name="input">The JSON string to deserialize.</param>
    /// <returns>An object of type T, or null if deserialization fails.</returns>
    public static T? JsonDeserialize<T>(string input)
    {
        return JsonConvert.DeserializeObject<T>(input);
    }

    /// <summary>
    /// Serializes an object to a JSON string with a custom datetime format.
    /// </summary>
    /// <param name="input">The object to serialize.</param>
    /// <param name="datetimeFormat">The datetime format to use.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string JsonSerialize(object input, string datetimeFormat)
    {
        JsonSerializerSettings dateFormatSettings = new()
        {
            DateFormatString = datetimeFormat
        };

        return JsonConvert.SerializeObject(input, dateFormatSettings);
    }

    /// <summary>
    /// Deserializes a JSON string to an object of type T with a custom datetime format.
    /// </summary>
    /// <typeparam name="T">Type of the output object.</typeparam>
    /// <param name="input">The JSON string to deserialize.</param>
    /// <param name="datetimeFormat">The datetime format to use.</param>
    /// <returns>An object of type T, or null if deserialization fails.</returns>
    public static T? JsonDeserialize<T>(string input, string datetimeFormat)
    {
        return JsonConvert.DeserializeObject<T>(input, new JsonSerializerSettings
        {
            DateFormatString = datetimeFormat
        });
    }

    #endregion

    #region File Handling

    /// <summary>
    /// Writes content to a file, creating the directory if it does not exist.
    /// </summary>
    public static void WriteToFile(string filePath, string content)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException($"File path not found in configuration for key: {filePath}");

        string? directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        File.WriteAllText(filePath, content, Encoding.UTF8);
    }

    /// <summary>
    /// Deletes a file if it exists.
    /// </summary>
    public static void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// Reads and returns the content of a file as a string.
    /// </summary>
    public static string GetFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        return File.ReadAllText(filePath, Encoding.UTF8);
    }

    /// <summary>
    /// Checks if a file exists at the given path.
    /// </summary>
    public static bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    #endregion

    #region Parsing and Conversion with Nullable Logic

    public static int? TryNullableIntParse(string input)
    {
        return int.TryParse(input, out int value) ? value : (int?)null;
    }

    public static long? TryNullableLongParse(string input)
    {
        return long.TryParse(input, out long value) ? value : (long?)null;
    }

    public static bool? TryNullableBoolParse(string input)
    {
        return bool.TryParse(input, out bool value) ? value : (bool?)null;
    }

    public static double? TryNullableDoubleParse(string input)
    {
        return double.TryParse(input, out double value) ? value : (double?)null;
    }

    public static float? TryNullableFloatParse(string input)
    {
        return float.TryParse(input, out float value) ? value : (float?)null;
    }

    public static DateTime? TryNullableDateTimeParse(string input)
    {
        return DateTime.TryParse(input, out DateTime value) ? value : (DateTime?)null;
    }

    #endregion

    #region Parsing and Conversion with Not Nullable Logic

    /// <summary>
    /// Parses a string to an integer. Returns 0 if parsing fails.
    /// </summary>
    public static int TryIntParse(string input)
    {
        return int.TryParse(input, out int value) ? value : 0;
    }

    /// <summary>
    /// Parses a string to a long integer. Returns 0 if parsing fails.
    /// </summary>
    public static long TryLongParse(string input)
    {
        return long.TryParse(input, out long value) ? value : 0L;
    }

    /// <summary>
    /// Parses a string to a boolean. Returns false if parsing fails.
    /// </summary>
    public static bool TryBoolParse(string input)
    {
        return bool.TryParse(input, out bool value) && value;
    }

    /// <summary>
    /// Parses a string to a double. Returns 0.0 if parsing fails.
    /// </summary>
    public static double TryDoubleParse(string input)
    {
        return double.TryParse(input, out double value) ? value : 0.0;
    }

    /// <summary>
    /// Parses a string to a float. Returns 0.0 if parsing fails.
    /// </summary>
    public static float TryFloatParse(string input)
    {
        return float.TryParse(input, out float value) ? value : 0.0f;
    }

    /// <summary>
    /// Parses a string to a DateTime. Returns DateTime.MinValue if parsing fails.
    /// </summary>
    public static DateTime TryDateTimeParse(string input)
    {
        return DateTime.TryParse(input, out DateTime value) ? value : DateTime.MinValue;
    }

    #endregion

    #region String and List Conversion

    /// <summary>
    /// Converts a string to a list of type T, using the specified delimiter.
    /// </summary>
    public static List<T> StringToList<T>(string input, char delimiter)
    {
        return input
            .Split(delimiter)
            .Select(s => (T)Convert.ChangeType(s.Trim(), typeof(T)))
            .ToList();
    }

    /// <summary>
    /// Converts a list of type T to a string, using the specified delimiter.
    /// </summary>
    public static string ListToString<T>(IEnumerable<T> list, char delimiter)
    {
        return string.Join(delimiter, list.Select(item => item?.ToString()));
    }

    #endregion

    #region Data Conversion

    public static DataTable ConvertListToDataTable<T>(IList<T> list)
    {
        DataTable table = new();
        if (list == null || list.Count == 0)
            return table;

        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo property in properties)
        {
            table.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
        }

        foreach (T item in list)
        {
            DataRow row = table.NewRow();
            foreach (PropertyInfo property in properties)
            {
                row[property.Name] = property.GetValue(item, null) ?? DBNull.Value;
            }

            table.Rows.Add(row);
        }

        return table;
    }

    #endregion

    #region Date Manipulation

    public static IEnumerable<DateTime> EachDay(DateTime fromDate, DateTime toDate)
    {
        for (var day = fromDate.Date; day.Date <= toDate.Date; day = day.AddDays(1))
        {
            yield return day;
        }
    }

    #endregion
}
