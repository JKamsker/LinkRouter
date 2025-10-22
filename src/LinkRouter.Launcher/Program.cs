using System;
using System.IO;
using LinkRouter;

namespace LinkRouter.Launcher;

class Program
{
    static int Main(string[] args)
    {
        LogArgumentsToFile(args);

        if (args.Length == 0)
        {
            Console.Error.WriteLine("No URL provided. Pass a URL or --register-defaults/--unregister-defaults.");
            return 2;
        }

        // Registration is now controlled via the Settings UI.
        if (args.Length >= 1)
        {
            var cmd = args[0].Trim().ToLowerInvariant();
            if (cmd is "--register-defaults" or "/register" or "-register" or "--unregister-defaults" or "/unregister" or "-unregister")
            {
                Console.Error.WriteLine("Registration commands are managed in LinkRouter Settings. Launch the GUI to register or unregister defaults.");
                return 2;
            }
        }

        string rawUrl = args[0];

        // Normalize and validate URL
        var (success, uri, error) = UrlNormalizer.NormalizeAndValidate(rawUrl);
        if (!success)
        {
            Console.Error.WriteLine(error);
            return 3;
        }

        // Load configuration with fallback to %AppData%\LinkRouter\.config
        string? configPath = null;
        var baseDirectoryCandidate = Path.Combine(AppContext.BaseDirectory, "mappings.json");
        if (File.Exists(baseDirectoryCandidate))
        {
            configPath = baseDirectoryCandidate;
        }
        else
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configRoot = Path.Combine(appData, "LinkRouter");
            var legacyCandidate = Path.Combine(configRoot, "mappings.json");
            var relocatedCandidate = Path.Combine(configRoot, ".config", "mappings.json");

            if (File.Exists(relocatedCandidate))
            {
                configPath = relocatedCandidate;
            }
            else if (File.Exists(legacyCandidate))
            {
                configPath = legacyCandidate;
            }
        }

        if (configPath is null)
        {
            Console.Error.WriteLine("No mappings.json could be found. Launch LinkRouter Settings to create a configuration.");
            return 4;
        }

        Config config;
        try
        {
            config = ConfigLoader.LoadConfig(configPath);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to read config from '{configPath}': {e.Message}");
            return 4;
        }

        // Find matching rule
        var ruleToUse = RuleMatcher.FindMatchingRule(config, uri!);
        if (ruleToUse == null)
        {
            Console.Error.WriteLine("No rule and no default configured.");
            return 5;
        }

        // Resolve profile (if any) and launch browser
        try
        {
            var effectiveRule = ProfileResolver.ResolveEffectiveRule(config, ruleToUse);
            BrowserLauncher.Launch(effectiveRule, uri!);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to start browser: {ex.Message}");
            return 6;
        }
    }

    private static void LogArgumentsToFile(string[] args)
    {
        // Log args to %AppData%\\LinkRouter\\args.log
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "LinkRouter");
            Directory.CreateDirectory(dir);
            var togglePath = Path.Combine(dir, "logging.disabled");
            if (File.Exists(togglePath))
            {
                return;
            }
            var logPath = Path.Combine(dir, "args.log");
            var line = $"{DateTime.Now:O} | {string.Join(" ", args)}";
            File.AppendAllText(logPath, line + Environment.NewLine);
        }
        catch
        {
            // ignore logging errors
        }
    }
}
