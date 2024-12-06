namespace EndHighUsageTasks.Helpers;

/// <summary>
/// Utility class for reading configurations from appsettings or other configuration providers.
/// Provides methods to read configuration values as different types.
/// </summary>
public class ReadConfigUtility(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    /// <summary>
    /// Caches configuration values for performance optimization.
    /// </summary>
    private readonly Dictionary<string, object> _cache = [];

    /// <summary>
    /// Reads a configuration value as a string by key.
    /// </summary>
    /// <param name="key">The configuration key to read.</param>
    /// <returns>The configuration value as a string, or an empty string if not found.</returns>
    public string ReadConfigByKeyAsString(string key)
    {
        if (_cache.TryGetValue(key, out var cachedValue) && cachedValue is string cachedString)
            return cachedString;

        string result = _configuration?.GetSection(key)?.Value?.ToString() ?? string.Empty;
        _cache[key] = result;
        return result;
    }

    /// <summary>
    /// Reads a configuration value as a boolean by key.
    /// </summary>
    /// <param name="key">The configuration key to read.</param>
    /// <returns>The configuration value as a boolean, or false if not found or invalid.</returns>
    public bool ReadConfigAsBool(string key)
    {
        if (_cache.TryGetValue(key, out var cachedValue) && cachedValue is bool cachedBool)
            return cachedBool;

        bool result = bool.TryParse(_configuration?.GetSection(key)?.Value, out bool parsed) && parsed;
        _cache[key] = result;
        return result;
    }

    /// <summary>
    /// Reads a configuration value as an integer by key.
    /// </summary>
    /// <param name="key">The configuration key to read.</param>
    /// <returns>The configuration value as an integer, or 0 if not found or invalid.</returns>
    public int ReadConfigAsInt(string key)
    {
        if (_cache.TryGetValue(key, out var cachedValue) && cachedValue is int cachedInt)
            return cachedInt;

        int result = int.TryParse(_configuration?.GetSection(key)?.Value, out int parsed) ? parsed : 0;
        _cache[key] = result;
        return result;
    }

    /// <summary>
    /// Reads a configuration value as a long by key.
    /// </summary>
    /// <param name="key">The configuration key to read.</param>
    /// <returns>The configuration value as a long, or 0 if not found or invalid.</returns>
    public long ReadConfigAsLong(string key)
    {
        if (_cache.TryGetValue(key, out var cachedValue) && cachedValue is long cachedLong)
            return cachedLong;

        long result = long.TryParse(_configuration?.GetSection(key)?.Value, out long parsed) ? parsed : 0L;
        _cache[key] = result;
        return result;
    }

    /// <summary>
    /// Reads a configuration value as a double by key.
    /// </summary>
    /// <param name="key">The configuration key to read.</param>
    /// <returns>The configuration value as a double, or 0.0 if not found or invalid.</returns>
    public double ReadConfigAsDouble(string key)
    {
        if (_cache.TryGetValue(key, out var cachedValue) && cachedValue is double cachedDouble)
            return cachedDouble;

        double result = double.TryParse(_configuration?.GetSection(key)?.Value, out double parsed) ? parsed : 0.0;
        _cache[key] = result;
        return result;
    }

    /// <summary>
    /// Reads a configuration section and binds it to a strongly-typed list.
    /// </summary>
    /// <typeparam name="T">The type of objects in the list.</typeparam>
    /// <param name="key">The configuration section key.</param>
    /// <returns>A list of objects, or an empty list if not found or invalid.</returns>
    public List<T> ReadConfigListByKey<T>(string key) where T : new()
    {
        if (_cache.TryGetValue(key, out var cachedValue) && cachedValue is List<T> cachedList)
            return cachedList;

        List<T> result = [];
        _configuration?.GetSection(key)?.Bind(result);
        _cache[key] = result;
        return result;
    }

    /// <summary>
    /// Reads a configuration section and binds it to a strongly-typed object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="key">The configuration section key.</param>
    /// <returns>An object of the specified type, or a new instance if not found or invalid.</returns>
    public T ReadConfigObjectByKey<T>(string key) where T : new()
    {
        if (_cache.TryGetValue(key, out var cachedValue) && cachedValue is T cachedObject)
            return cachedObject;

        T result = new();
        _configuration?.GetSection(key)?.Bind(result);
        _cache[key] = result;
        return result;
    }

    /// <summary>
    /// Reads a comma-separated configuration value as a list of strings.
    /// </summary>
    /// <param name="key">The configuration key to read.</param>
    /// <returns>A list of strings, or an empty list if not found or invalid.</returns>
    public List<string> ReadConfigListOfStringByKey(string key, char? seperator = null)
    {
        if (_cache.TryGetValue(key, out var cachedValue) && cachedValue is List<string> cachedList)
            return cachedList;

        string? value = _configuration?.GetSection(key)?.Value;
        List<string> result = !string.IsNullOrEmpty(value) ? value.Split(seperator ?? ',').Select(s => s.Trim()).ToList() : [];
        _cache[key] = result;
        return result;
    }

    /// <summary>
    /// Reads a connection string by name.
    /// </summary>
    /// <param name="connectionName">The name of the connection string.</param>
    /// <returns>The connection string, or an empty string if not found.</returns>
    public string ReadConnectionString(string connectionName)
    {
        if (_cache.TryGetValue(connectionName, out var cachedValue) && cachedValue is string cachedString)
            return cachedString;

        string result = _configuration?.GetConnectionString(connectionName) ?? string.Empty;
        _cache[connectionName] = result;
        return result;
    }
}

