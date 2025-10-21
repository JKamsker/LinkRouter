Nice goal — split your life by URL so “work” and “fun” don’t elbow each other in the same browser profile. Below is a practical, implementable plan (C#-friendly) plus a ready-to-use console-app skeleton, config format, registry & deployment notes, and gotchas. I’ll be direct and pragmatic — you can copy, extend and ship this.

---

# High-level approach (how it works)

1. **Create a small "URL router" app** (C# console or single-EXE) that Windows will call when an `http`/`https` link is clicked.
2. **Router inspects the URL** against a mapping ruleset (domains, regex, path rules).
3. **Router launches the chosen browser binary** with arguments that target a specific profile (Chrome/Edge use `--profile-directory` or `--user-data-dir`; Firefox uses `-P` and `-no-remote`).
4. If no rule matches, router launches your default browser or prompts.
5. Install/declare the router as the default handler for `http` & `https` (user has to confirm in Windows Settings).

This keeps all heavy logic in your C# app and uses browser CLIs to choose profiles.

---

# Important Windows notes (practical constraints)

* Since Windows 8/10, the *per-user default* is controlled by Settings and the `UserChoice` registry key; **you cannot silently overwrite** the user default without their action. Your installer can register your app as a candidate; the user must select it in Settings → Apps → Default apps → Web browser (or set by Protocols for http/https).
* Avoid an infinite loop: router must open browsers directly, not call `ShellExecute` on `http(s)` again.
* Validate URLs strictly to avoid turning your machine into an open redirector or malware vector.

---

# Mapping config (JSON example)

Store rules in a JSON file (easy to edit). Example `mappings.json`:

```json
{
  "default": {
    "browser": "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
    "argsTemplate": "--new-window --profile-directory=\"Default\" \"{url}\""
  },
  "rules": [
    {
      "match": "regex",
      "pattern": "^(?:https?://)?(?:www\\.)?mycompany\\.internal",
      "browser": "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
      "argsTemplate": "--new-window --profile-directory=\"Work\" \"{url}\""
    },
    {
      "match": "domain",
      "pattern": "github.com",
      "browser": "C:\\Program Files\\Mozilla Firefox\\firefox.exe",
      "argsTemplate": "-P \"Dev\" -new-window \"{url}\" -no-remote"
    },
    {
      "match": "domain",
      "pattern": "slack.com",
      "browser": "C:\\Program Files\\Microsoft\\Edge\\Application\\msedge.exe",
      "argsTemplate": "--profile-directory=\"Profile 1\" \"{url}\""
    }
  ]
}
```

* `match`: `"domain"` performs a host comparison, `"regex"` applies a regex to the full URL.
* `argsTemplate` contains `{url}` placeholder that your app replaces with the escaped URL.

---

# Example C# router (concise, production-ready skeleton)

All code/comments in English per your preference.

```csharp
// Program.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web;

record Rule(string match, string pattern, string browser, string argsTemplate);
record Config(Rule[] rules, Rule? @default);

class UrlRouter
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
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
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
            catch (Exception) { /* ignore faulty patterns */ }
        }

        var ruleToUse = matched ?? config.@default;
        if (ruleToUse == null)
        {
            Console.Error.WriteLine("No rule and no default configured.");
            return 5;
        }

        // Prepare args: escape URL
        string escapedUrl = uri.ToString(); // keep it raw; wrap in quotes below
        string argsTemplate = ruleToUse.argsTemplate.Replace("{url}", escapedUrl);

        // Launch target browser directly to avoid re-invoking router
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = ruleToUse.browser,
                Arguments = argsTemplate,
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
```

Notes:

* Keep `UseShellExecute=false` so Windows doesn’t re-resolve `http(s)` and loop.
* You may want to `ProcessStartInfo` with `UseShellExecute=true` if launching certain shell-registered apps (but that risks loops).
* Add logging (file-based) for debugging.

---

# Browser CLI specifics (practical examples)

* **Chrome / Edge (Chromium)**

    * `--profile-directory="Profile 1"` (profile name as seen in `chrome://version` under "Profile Path" last folder, e.g. `Profile 1`)
    * Alternative: `--user-data-dir="C:\Users\Jonas\AppData\Local\Google\Chrome\User Data\WorkProfile"` to create/force a separate user data dir (safer isolation).
    * Example args: `--new-window --profile-directory="Work" "https://mycompany.internal"`

* **Firefox**

    * `-P "ProfileName" -new-window "https://example.com" -no-remote`
    * `-no-remote` is required to open an instance with a different profile if another Firefox is running.

* **Edge** uses same flags as Chrome (`--profile-directory="Profile 1"`).

Test the exact flags on your machine — profile folder names and paths can differ.

---

# Registry / Deployment steps (what installer should do)

1. **Place EXE and config** in a stable folder (e.g., `C:\Program Files\UrlRouter\urlrouter.exe` and `mappings.json`).
2. **Register as a URL handler candidate** by creating an app registration (so it appears in Settings → Default apps). Typical registry keys:

    * Under `HKCU\Software\Classes\UrlRouter`:

        * `(Default) = "URL:UrlRouter Protocol"`
        * `URL Protocol = ""`
        * `shell\open\command\ (Default) = "\"C:\\Program Files\\UrlRouter\\urlrouter.exe\" \"%1\""`
    * Then set `HKCU\Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice\ProgId` to your ProgId — *but Windows will block programmatic set of UserChoice without user interaction*. So instead instruct the user to set your app as the default web browser in Settings.
3. **Provide an installer and a small Settings guide** telling the user how to select your router as the default handler for HTTP/HTTPS. The installer can open the Default Apps page automatically (`ms-settings:defaultapps`) to prompt the user.

(You can register under HKLM for machine-wide, but per-user HKCU is safer.)

---

# UX & management ideas

* Add a small GUI to edit `mappings.json` (drag & drop, quick rules).
* Add a “test URL” button and a dry-run mode (simulate rule selection without launching).
* Add logging + “last matched rule” notification tray balloon for debugging.
* Option to show a confirmation dialog (for unknown domains).
* Option to keep a whitelist of work domains and fall back to private browser otherwise.

---

# Security & privacy

* Sanitize input; reject non-http/https schemes (unless explicitly allowed).
* Don’t accept arbitrary command templates from untrusted sources (config should be local and user-owned).
* Consider signed installer if you’ll deploy on company machines.

---

# Quick checklist to get working (concrete next steps)

1. Create the console app from skeleton above. Add better error handling and logging.
2. Create `mappings.json` with your real profile names and browser paths. Test launches manually from command line first.
3. Build an installer (or place files in `Program Files`). Register the protocol handler as described (HKCU).
4. Set the router as default in Windows Settings → Apps → Default apps → choose your router as the Web browser or for HTTP/HTTPS protocols.
5. Test edge cases (existing browser running, special characters, long URLs). Tweak process launch flags (`-no-remote` for Firefox, `--user-data-dir` for Chrome if needed).

---

If you want, I can:

* Produce a slightly more feature-complete C# implementation (logging, GUI editor, pattern testing) — I’ll include unit-test friendly parts and a small WPF config editor (all in English).
* Or give the exact registry `.reg` snippet and an installer script (WiX/NSIS) to register the app and open the Windows Default Apps pane for the user to confirm.

Which of those would be most useful — the full router + GUI (I can stub it out in one file) or the installer/registry bits next?
