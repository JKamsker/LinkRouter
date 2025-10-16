using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LinkRouter;

public static class ConfigLoader
{
    public static Config LoadConfig(string configPath)
    {
        var json = File.ReadAllText(configPath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Load profiles map (optional)
        Dictionary<string, Profile>? profiles = null;
        if (root.TryGetProperty("profiles", out var jpRoot) && jpRoot.ValueKind == JsonValueKind.Object)
        {
            profiles = new Dictionary<string, Profile>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in jpRoot.EnumerateObject())
            {
                var val = prop.Value;
                string? pBrowser = val.TryGetProperty("browser", out var jpb) ? jpb.GetString() : null;
                string? pArgsTemplate = val.TryGetProperty("argsTemplate", out var jpat) ? jpat.GetString() : null;
                string? pProfile = val.TryGetProperty("profile", out var jpp) ? jpp.GetString() : null;
                string? pUserDataDir = val.TryGetProperty("userDataDir", out var jpud) ? jpud.GetString() : null;
                string? pWorkingDirectory = val.TryGetProperty("workingDirectory", out var jpwd) ? jpwd.GetString() : null;
                profiles[prop.Name] = new Profile(pBrowser, pArgsTemplate, pProfile, pUserDataDir, pWorkingDirectory);
            }
        }

        // Load rules
        var rules = new List<Rule>();
        if (root.TryGetProperty("rules", out var jr))
        {
            foreach (var el in jr.EnumerateArray())
            {
                string? profile = el.TryGetProperty("profile", out var jp) ? jp.GetString() : null; // browser profile value
                string? userDataDir = el.TryGetProperty("userDataDir", out var jud) ? jud.GetString() : null;
                string? workingDirectory = el.TryGetProperty("workingDirectory", out var jwd) ? jwd.GetString() : null;
                string? useProfile = el.TryGetProperty("useProfile", out var jup) ? jup.GetString() : null; // profile reference
                string? browser = el.TryGetProperty("browser", out var jbr) ? jbr.GetString() : null;
                string? argsTemplate = el.TryGetProperty("argsTemplate", out var jat) ? jat.GetString() : null;
                bool enabled = el.TryGetProperty("enabled", out var jenabled) && jenabled.ValueKind == JsonValueKind.False ? false : true;

                rules.Add(new Rule(
                    match: el.GetProperty("match").GetString()!,
                    pattern: el.GetProperty("pattern").GetString()!,
                    browser: browser,
                    argsTemplate: argsTemplate,
                    profile: profile,
                    userDataDir: userDataDir,
                    workingDirectory: workingDirectory,
                    useProfile: useProfile,
                    Enabled: enabled
                ));
            }
        }

        // Load default
        Rule? def = null;
        if (root.TryGetProperty("default", out var jd))
        {
            string? dProfile = jd.TryGetProperty("profile", out var jdp) ? jdp.GetString() : null; // browser profile value
            string? dUserDataDir = jd.TryGetProperty("userDataDir", out var jdud) ? jdud.GetString() : null;
            string? dWorkingDirectory = jd.TryGetProperty("workingDirectory", out var jdwd) ? jdwd.GetString() : null;
            string? dUseProfile = jd.TryGetProperty("useProfile", out var jdup) ? jdup.GetString() : null;
            string? dBrowser = jd.TryGetProperty("browser", out var jdb) ? jdb.GetString() : null;
            string? dArgsTemplate = jd.TryGetProperty("argsTemplate", out var jda) ? jda.GetString() : null;
            bool dEnabled = jd.TryGetProperty("enabled", out var jden) && jden.ValueKind == JsonValueKind.False ? false : true;
            def = new Rule(
                match: "default",
                pattern: ".*",
                browser: dBrowser,
                argsTemplate: dArgsTemplate,
                profile: dProfile,
                userDataDir: dUserDataDir,
                workingDirectory: dWorkingDirectory,
                useProfile: dUseProfile,
                Enabled: dEnabled
            );
        }

        return new Config(rules.ToArray(), def, profiles);
    }
}
