using System.Text.Json;
using System.Text.Json.Nodes;
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
    private readonly string _settingsPath;
    private readonly string _manifestPath;
    private readonly string _backupFolder;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public ConfigService()
    {
        _rootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LinkRouter");
        _settingsPath = Path.Combine(_rootFolder, "settings.json");
        _manifestPath = Path.Combine(_rootFolder, "mappings.json");
        _backupFolder = Path.Combine(_rootFolder, "backups");
    }

    public string ConfigPath => _settingsPath;
    public string ManifestPath => _manifestPath;
    public string BackupFolder => _backupFolder;

    public async Task<ConfigDocument> LoadAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => LoadInternal(), cancellationToken).ConfigureAwait(false);
    }

    public async Task SaveAsync(SettingsSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        await Task.Run(() => SaveInternal(snapshot), cancellationToken).ConfigureAwait(false);
    }

    public string SerializeToJson(Config config) => SerializeConfig(config);

    private ConfigDocument LoadInternal()
    {
        Directory.CreateDirectory(_rootFolder);
        Directory.CreateDirectory(_backupFolder);

        Config config;
        var profileStates = new Dictionary<string, ProfileUiState>(StringComparer.OrdinalIgnoreCase);
        DateTime? settingsLastModified = null;
        DateTime? manifestLastModified = null;

        if (File.Exists(_settingsPath))
        {
            var loaded = LoadSettingsFile(_settingsPath);
            config = loaded.Config;
            profileStates = loaded.ProfileStates;
            settingsLastModified = File.GetLastWriteTime(_settingsPath);
        }
        else if (File.Exists(_manifestPath))
        {
            config = ConfigLoader.LoadConfig(_manifestPath);
            profileStates = new Dictionary<string, ProfileUiState>(StringComparer.OrdinalIgnoreCase);
        }
        else
        {
            config = CreateEmptyConfig();
        }

        if (File.Exists(_manifestPath))
        {
            manifestLastModified = File.GetLastWriteTime(_manifestPath);
        }

        var backups = Directory.Exists(_backupFolder)
            ? Directory.GetFiles(_backupFolder, "*.json", SearchOption.TopDirectoryOnly)
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .Select(f => new ConfigBackup(f.FullName, f.LastWriteTimeUtc, f.Length))
                .ToList()
            : new List<ConfigBackup>();

        return new ConfigDocument(
            config,
            _settingsPath,
            settingsLastModified,
            _manifestPath,
            manifestLastModified,
            backups,
            profileStates);
    }

    private void SaveInternal(SettingsSnapshot snapshot)
    {
        Directory.CreateDirectory(_rootFolder);
        Directory.CreateDirectory(_backupFolder);

        var settingsJson = SerializeSettings(snapshot);
        File.WriteAllText(_settingsPath, settingsJson);

        string manifestJson = SerializeConfig(snapshot.Config);
        string tempPath = Path.Combine(_rootFolder, $"mappings.{Guid.NewGuid():N}.tmp");
        File.WriteAllText(tempPath, manifestJson);

        if (File.Exists(_manifestPath))
        {
            var backupPath = Path.Combine(_backupFolder, $"mappings_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
            File.Copy(_manifestPath, backupPath, overwrite: false);
        }

        File.Move(tempPath, _manifestPath, true);
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

    private string SerializeSettings(SettingsSnapshot snapshot)
    {
        var configJson = SerializeConfig(snapshot.Config);
        var configNode = JsonNode.Parse(configJson);

        var root = new JsonObject
        {
            ["config"] = configNode
        };

        if (snapshot.ProfileStates.Count > 0)
        {
            var profilesNode = new JsonObject();
            foreach (var kvp in snapshot.ProfileStates)
            {
                if (!kvp.Value.IsAdvanced)
                {
                    continue;
                }

                var profileNode = new JsonObject
                {
                    ["isAdvanced"] = true
                };
                profilesNode[kvp.Key] = profileNode;
            }

            if (profilesNode.Count > 0)
            {
                var uiNode = new JsonObject
                {
                    ["profiles"] = profilesNode
                };
                root["ui"] = uiNode;
            }
        }

        return root.ToJsonString(_serializerOptions);
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

    private (Config Config, Dictionary<string, ProfileUiState> ProfileStates) LoadSettingsFile(string path)
    {
        var json = File.ReadAllText(path);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Config config;
        if (root.TryGetProperty("config", out var configElement))
        {
            config = ConfigLoader.LoadConfig(configElement);
        }
        else
        {
            config = ConfigLoader.LoadConfig(root);
        }

        var profileStates = new Dictionary<string, ProfileUiState>(StringComparer.OrdinalIgnoreCase);
        if (root.TryGetProperty("ui", out var uiNode) && uiNode.ValueKind == JsonValueKind.Object)
        {
            if (uiNode.TryGetProperty("profiles", out var profilesNode) && profilesNode.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in profilesNode.EnumerateObject())
                {
                    bool isAdvanced = prop.Value.TryGetProperty("isAdvanced", out var advancedValue) && advancedValue.ValueKind == JsonValueKind.True;
                    profileStates[prop.Name] = new ProfileUiState(isAdvanced);
                }
            }
        }

        return (config, profileStates);
    }
}

public sealed record ConfigDocument(
    Config Config,
    string SettingsPath,
    DateTime? SettingsLastModified,
    string ManifestPath,
    DateTime? ManifestLastModified,
    IReadOnlyList<ConfigBackup> Backups,
    IReadOnlyDictionary<string, ProfileUiState> ProfileStates);

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

public sealed record ProfileUiState(bool IsAdvanced);

public sealed record SettingsSnapshot(Config Config, IReadOnlyDictionary<string, ProfileUiState> ProfileStates);

