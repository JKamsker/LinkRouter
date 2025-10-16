using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Collections.Generic;

namespace LinkRouter;

record Rule(string match, string pattern, string browser, string argsTemplate);
record Config(Rule[] rules, Rule? @default);

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("No URL provided.");
            return 2;
        }

        string rawUrl = args[0];
        // Normalize: ensure scheme exists
        if (!rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            rawUrl = "https://" + rawUrl;
        }

        // Basic validation
        if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out var uri))
        {
            Console.Error.WriteLine("Invalid URL.");
            return 3;
        }

        string configPath = Path.Combine(AppContext.BaseDirectory, "mappings.json");
        Config config;
        try
        {
            var json = File.ReadAllText(configPath);
            // Using same record for default (quick hack): default stored as single rule with key "default"
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var rules = new List<Rule>();
            if (root.TryGetProperty("rules", out var jr))
            {
                foreach (var el in jr.EnumerateArray())
                {
                    rules.Add(new Rule(
                        match: el.GetProperty("match").GetString()!,
                        pattern: el.GetProperty("pattern").GetString()!,
                        browser: el.GetProperty("browser").GetString()!,
                        argsTemplate: el.GetProperty("argsTemplate").GetString()!
                    ));
                }
            }

            Rule? def = null;
            if (root.TryGetProperty("default", out var jd))
            {
                def = new Rule("default", ".*", jd.GetProperty("browser").GetString()!, jd.GetProperty("argsTemplate").GetString()!);
            }

            config = new Config(rules.ToArray(), def);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to read config: {e.Message}");
            return 4;
        }

        // Choose rule
        Rule? matched = null;
        foreach (var r in config.rules)
        {
            try
            {
                if (r.match.Equals("domain", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(uri.Host, r.pattern, StringComparison.OrdinalIgnoreCase) ||
                        uri.Host.EndsWith("." + r.pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        matched = r;
                        break;
                    }
                }
                else if (r.match.Equals("regex", StringComparison.OrdinalIgnoreCase))
                {
                    if (Regex.IsMatch(uri.ToString(), r.pattern, RegexOptions.IgnoreCase))
                    {
                        matched = r;
                        break;
                    }
                }
                else if (r.match.Equals("contains", StringComparison.OrdinalIgnoreCase))
                {
                    if (uri.ToString().IndexOf(r.pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        matched = r;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                // ignore faulty patterns
            }
        }

        var ruleToUse = matched ?? config.@default;
        if (ruleToUse == null)
        {
            Console.Error.WriteLine("No rule and no default configured.");
            return 5;
        }

        // Prepare args: escape URL (already absolute and safe to pass as quoted string)
        string escapedUrl = uri.ToString();
        string argsToPass = ruleToUse.argsTemplate.Replace("{url}", escapedUrl);

        // Launch target browser directly to avoid re-invoking router
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = ruleToUse.browser,
                Arguments = argsToPass,
                UseShellExecute = false
            };
            Process.Start(startInfo);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to start browser: {ex.Message}");
            return 6;
        }
    }
}