using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinkRouter;

namespace LinkRouter.Settings.Services;

public sealed class ConfigService
{
    private readonly string _rootFolder;
    private readonly string _configPath;
    private readonly string _backupFolder;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public ConfigService()
    {
        _rootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LinkRouter");
        _configPath = Path.Combine(_rootFolder, "mappings.json");
        _backupFolder = Path.Combine(_rootFolder, "backups");
    }

    public string ConfigPath => _configPath;
    public string BackupFolder => _backupFolder;

    public async Task<ConfigDocument> LoadAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => LoadInternal(), cancellationToken).ConfigureAwait(false);
    }

    public async Task SaveAsync(Config config, CancellationToken cancellationToken = default)
    {
        await Task.Run(() => SaveInternal(config), cancellationToken).ConfigureAwait(false);
    }

    public string SerializeToJson(Config config) => SerializeConfig(config);

    private ConfigDocument LoadInternal()
    {
        Directory.CreateDirectory(_rootFolder);
        Directory.CreateDirectory(_backupFolder);

        Config config;
        DateTime? lastModified = null;

        if (File.Exists(_configPath))
        {
            config = ConfigLoader.LoadConfig(_configPath);
            lastModified = File.GetLastWriteTime(_configPath);
        }
        else
        {
            config = CreateEmptyConfig();
        }

        var backups = Directory.Exists(_backupFolder)
            ? Directory.GetFiles(_backupFolder, "*.json", SearchOption.TopDirectoryOnly)
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .Select(f => new ConfigBackup(f.FullName, f.LastWriteTimeUtc, f.Length))
                .ToList()
            : new List<ConfigBackup>();

        return new ConfigDocument(config, _configPath, lastModified, backups);
    }

    private void SaveInternal(Config config)
    {
        Directory.CreateDirectory(_rootFolder);
        Directory.CreateDirectory(_backupFolder);

        string json = SerializeConfig(config);
        string tempPath = Path.Combine(_rootFolder, $"mappings.{Guid.NewGuid():N}.tmp");
        File.WriteAllText(tempPath, json);

        if (File.Exists(_configPath))
        {
            var backupPath = Path.Combine(_backupFolder, $"mappings_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
            File.Copy(_configPath, backupPath, overwrite: false);
        }

        File.Move(tempPath, _configPath, true);
    }

    private string SerializeConfig(Config config)
    {
        var payload = new Dictionary<string, object?>();

        if (config.rules?.Length > 0)
        {
            payload["rules"] = config.rules.Select(rule => SerializeRule(rule, includeMatchAndPattern: true)).ToArray();
        }
        else
        {
            payload["rules"] = Array.Empty<object>();
        }

        if (config.@default is not null)
        {
            payload["default"] = SerializeRule(config.@default, includeMatchAndPattern: false);
        }

        if (config.profiles is not null && config.profiles.Count > 0)
        {
            payload["profiles"] = config.profiles.ToDictionary(kvp => kvp.Key, kvp => SerializeProfile(kvp.Value));
        }

        return JsonSerializer.Serialize(payload, _serializerOptions);
    }

    private static Dictionary<string, object?> SerializeRule(Rule rule, bool includeMatchAndPattern)
    {
        var map = new Dictionary<string, object?>();

        if (includeMatchAndPattern)
        {
            map["match"] = rule.match;
            map["pattern"] = rule.pattern;
        }

        if (!string.IsNullOrWhiteSpace(rule.browser))
        {
            map["browser"] = rule.browser;
        }
        if (!string.IsNullOrWhiteSpace(rule.argsTemplate))
        {
            map["argsTemplate"] = rule.argsTemplate;
        }
        if (!string.IsNullOrWhiteSpace(rule.profile))
        {
            map["profile"] = rule.profile;
        }
        if (!string.IsNullOrWhiteSpace(rule.userDataDir))
        {
            map["userDataDir"] = rule.userDataDir;
        }
        if (!string.IsNullOrWhiteSpace(rule.workingDirectory))
        {
            map["workingDirectory"] = rule.workingDirectory;
        }
        if (!string.IsNullOrWhiteSpace(rule.useProfile))
        {
            map["useProfile"] = rule.useProfile;
        }
        if (!rule.Enabled)
        {
            map["enabled"] = false;
        }

        return map;
    }

    private static Dictionary<string, object?> SerializeProfile(Profile profile)
    {
        var map = new Dictionary<string, object?>();

        if (!string.IsNullOrWhiteSpace(profile.browser))
        {
            map["browser"] = profile.browser;
        }
        if (!string.IsNullOrWhiteSpace(profile.argsTemplate))
        {
            map["argsTemplate"] = profile.argsTemplate;
        }
        if (!string.IsNullOrWhiteSpace(profile.profile))
        {
            map["profile"] = profile.profile;
        }
        if (!string.IsNullOrWhiteSpace(profile.userDataDir))
        {
            map["userDataDir"] = profile.userDataDir;
        }
        if (!string.IsNullOrWhiteSpace(profile.workingDirectory))
        {
            map["workingDirectory"] = profile.workingDirectory;
        }

        return map;
    }

    private static Config CreateEmptyConfig()
    {
        return new Config(Array.Empty<Rule>(), null, new Dictionary<string, Profile>(StringComparer.OrdinalIgnoreCase));
    }
}

public sealed record ConfigDocument(Config Config, string Path, DateTime? LastModified, IReadOnlyList<ConfigBackup> Backups);

public sealed record ConfigBackup
{
    public ConfigBackup()
    {
        Path = string.Empty;
        TimestampUtc = DateTime.MinValue;
        SizeBytes = 0;
    }

    public ConfigBackup(string path, DateTime timestampUtc, long sizeBytes)
    {
        Path = path;
        TimestampUtc = timestampUtc;
        SizeBytes = sizeBytes;
    }

    public string Path { get; set; }
    public DateTime TimestampUtc { get; set; }
    public long SizeBytes { get; set; }

    public string FileName => System.IO.Path.GetFileName(Path);
    public string TimestampDisplay => TimestampUtc.ToLocalTime().ToString("G");
}

