using System;
using System.Diagnostics;
using System.IO;

namespace LinkRouter;

public static class BrowserLauncher
{
    public static void Launch(Rule rule, Uri uri)
    {
        string argsToPass = GetLaunchArguments(rule, uri);

        var startInfo = new ProcessStartInfo
        {
            FileName = rule.browser!,
            Arguments = argsToPass,
            UseShellExecute = false
        };

        // Apply working directory if provided and exists
        if (!string.IsNullOrWhiteSpace(rule.workingDirectory) && Directory.Exists(rule.workingDirectory))
        {
            startInfo.WorkingDirectory = rule.workingDirectory!;
        }

        Process.Start(startInfo);
    }

    public static string GetLaunchArguments(Rule rule, Uri uri)
    {
        string escapedUrl = uri.ToString();
        string argsToPass = rule.argsTemplate!
            .Replace("{url}", escapedUrl)
            .Replace("{profile}", rule.profile ?? string.Empty)
            .Replace("{userDataDir}", rule.userDataDir ?? string.Empty);

        var browserLower = (rule.browser ?? string.Empty).ToLowerInvariant();
        bool isFirefox = browserLower.Contains("firefox");
        bool isChromium = browserLower.Contains("chrome") || browserLower.Contains("msedge") || browserLower.Contains("chromium");
        bool incognito = rule.incognito ?? false;

        if (!string.IsNullOrWhiteSpace(rule.profile))
        {
            argsToPass = AddProfileArguments(argsToPass, rule.profile, isFirefox, isChromium);
        }

        if (isChromium && !string.IsNullOrWhiteSpace(rule.userDataDir))
        {
            argsToPass = AddUserDataDirArgument(argsToPass, rule.userDataDir);
        }

        if (incognito)
        {
            argsToPass = AddIncognitoArgument(argsToPass, browserLower, isFirefox, isChromium);
        }

        return argsToPass;
    }

    private static string AddProfileArguments(string args, string profile, bool isFirefox, bool isChromium)
    {
        if (isFirefox)
        {
            if (!args.Contains("-P ", StringComparison.OrdinalIgnoreCase))
            {
                args = $"-P \"{profile}\" -no-remote " + args;
            }
            else if (!args.Contains("-no-remote", StringComparison.OrdinalIgnoreCase))
            {
                args = "-no-remote " + args;
            }
        }
        else if (isChromium)
        {
            if (!args.Contains("--profile-directory", StringComparison.OrdinalIgnoreCase))
            {
                args = $"--profile-directory=\"{profile}\" " + args;
            }
        }

        return args;
    }

    private static string AddUserDataDirArgument(string args, string userDataDir)
    {
        if (!args.Contains("--user-data-dir", StringComparison.OrdinalIgnoreCase))
        {
            args = $"--user-data-dir=\"{userDataDir}\" " + args;
        }

        return args;
    }

    private static string AddIncognitoArgument(string args, string browserLower, bool isFirefox, bool isChromium)
    {
        if (ContainsIncognitoArgument(args))
        {
            return args;
        }

        if (isFirefox)
        {
            args = RemoveFirefoxNewWindow(args);
            return $"-private-window {args}";
        }

        if (browserLower.Contains("msedge"))
        {
            return $"--inprivate {args}";
        }

        if (isChromium)
        {
            return $"--incognito {args}";
        }

        return $"--incognito {args}";
    }

    private static bool ContainsIncognitoArgument(string args)
    {
        return args.Contains("--incognito", StringComparison.OrdinalIgnoreCase)
               || args.Contains("--inprivate", StringComparison.OrdinalIgnoreCase)
               || args.Contains("-private-window", StringComparison.OrdinalIgnoreCase);
    }

    private static string RemoveFirefoxNewWindow(string args)
    {
        if (string.IsNullOrWhiteSpace(args))
        {
            return args;
        }

        return args
            .Replace("-new-window", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("--new-window", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
    }
}
