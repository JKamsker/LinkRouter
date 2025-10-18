using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkRouter;
using LinkRouter.Settings.Core.Services;
using LinkRouter.Settings.Core.Infrastructure;

namespace LinkRouter.Settings.Core.ViewModels;

public partial class ImportExportViewModel : ObservableObject
{
    private readonly ConfigService _configService = SettingsServiceLocator.ConfigService;
    private readonly ConfigurationState _state = SettingsServiceLocator.ConfigurationState;

    [ObservableProperty]
    private string _importPath = string.Empty;

    [ObservableProperty]
    private string? _diffSummary;

    [ObservableProperty]
    private string? _error;

    [ObservableProperty]
    private string _exportPath = string.Empty;

    public ObservableCollection<ConfigBackup> Backups { get; } = new();

    public ImportExportViewModel()
    {
        RefreshBackups();
        _state.StateChanged += (_, _) => RefreshBackups();
    }

    private void RefreshBackups()
    {
        Backups.Clear();
        if (_state.Document is ConfigDocument doc)
        {
            foreach (var backup in doc.Backups)
            {
                Backups.Add(backup);
            }
        }
    }

    [RelayCommand]
    private async Task AnalyzeImportAsync()
    {
        Error = null;
        DiffSummary = null;

        if (!File.Exists(ImportPath))
        {
            Error = "Select a valid JSON file to import.";
            return;
        }

        try
        {
            var importConfig = await Task.Run(() => ConfigLoader.LoadConfig(ImportPath));
            var existing = _state.BuildConfig();
            DiffSummary = BuildDiff(existing, importConfig);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    [RelayCommand]
    private async Task ReplaceAsync()
    {
        await ImportAsync(merge: false).ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task MergeAsync()
    {
        await ImportAsync(merge: true).ConfigureAwait(false);
    }

    private async Task ImportAsync(bool merge)
    {
        Error = null;
        if (!File.Exists(ImportPath))
        {
            Error = "Select a valid JSON file.";
            return;
        }

        try
        {
            var incoming = await Task.Run(() => ConfigLoader.LoadConfig(ImportPath));
            Config newConfig;
            if (merge)
            {
                newConfig = MergeConfigs(_state.BuildConfig(), incoming);
            }
            else
            {
                newConfig = incoming;
            }

            await _configService.SaveAsync(newConfig);
            var document = await _configService.LoadAsync();
            _state.Load(document);
            DiffSummary = "Import completed.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        Error = null;

        if (string.IsNullOrWhiteSpace(ExportPath))
        {
            Error = "Specify a destination path.";
            return;
        }

        try
        {
            var config = _state.BuildConfig();
            var json = _configService.SerializeToJson(config);
            var directory = Path.GetDirectoryName(ExportPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(ExportPath, json, Encoding.UTF8);
            DiffSummary = $"Exported to {ExportPath}.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    [RelayCommand]
    private async Task RestoreBackupAsync(ConfigBackup? backup)
    {
        if (backup is null)
        {
            return;
        }

        try
        {
            if (!File.Exists(backup.Path))
            {
                Error = "Backup file not found.";
                return;
            }

            var json = await File.ReadAllTextAsync(backup.Path);
            var tempPath = Path.Combine(Path.GetTempPath(), $"LinkRouter_restore_{Guid.NewGuid():N}.json");
            await File.WriteAllTextAsync(tempPath, json);

            var restored = ConfigLoader.LoadConfig(tempPath);
            await _configService.SaveAsync(restored);
            var document = await _configService.LoadAsync();
            _state.Load(document);
            DiffSummary = $"Restored backup {backup.FileName}.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    private static string BuildDiff(Config existing, Config incoming)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Existing rules: {existing.rules.Length}, Incoming rules: {incoming.rules.Length}");

        var existingRuleKeys = existing.rules.Select(r => (r.match, r.pattern)).ToHashSet();
        var incomingRuleKeys = incoming.rules.Select(r => (r.match, r.pattern)).ToHashSet();

        var added = incomingRuleKeys.Except(existingRuleKeys).Count();
        var removed = existingRuleKeys.Except(incomingRuleKeys).Count();

        sb.AppendLine($"Rules added: {added}");
        sb.AppendLine($"Rules removed: {removed}");

        var existingProfiles = existing.profiles?.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var incomingProfiles = incoming.profiles?.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var profileAdded = incomingProfiles.Except(existingProfiles).Count();
        var profileRemoved = existingProfiles.Except(incomingProfiles).Count();
        var profileShared = incomingProfiles.Intersect(existingProfiles).Count();

        sb.AppendLine($"Profiles added: {profileAdded}");
        sb.AppendLine($"Profiles removed: {profileRemoved}");
        sb.AppendLine($"Profiles overlapping: {profileShared}");

        return sb.ToString();
    }

    private static Config MergeConfigs(Config existing, Config incoming)
    {
        var ruleList = existing.rules.ToList();
        foreach (var rule in incoming.rules)
        {
            if (!ruleList.Any(r => r.match.Equals(rule.match, StringComparison.OrdinalIgnoreCase) && r.pattern.Equals(rule.pattern, StringComparison.OrdinalIgnoreCase)))
            {
                ruleList.Add(rule);
            }
        }

        Rule? defaultRule = incoming.@default ?? existing.@default;

        Dictionary<string, Profile>? profiles = null;
        if (existing.profiles is not null)
        {
            profiles = new Dictionary<string, Profile>(existing.profiles, StringComparer.OrdinalIgnoreCase);
        }
        else if (incoming.profiles is not null)
        {
            profiles = new Dictionary<string, Profile>(StringComparer.OrdinalIgnoreCase);
        }

        if (incoming.profiles is not null)
        {
            profiles ??= new Dictionary<string, Profile>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in incoming.profiles)
            {
                profiles[kvp.Key] = kvp.Value;
            }
        }

        return new Config(ruleList.ToArray(), defaultRule, profiles);
    }
}
