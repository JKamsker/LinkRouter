using System;
using System.Collections.Generic;

namespace LinkRouter;

public static class ProfileResolver
{
    public static Rule ResolveEffectiveRule(Config config, Rule rule)
    {
        // Copy base from profile if referenced
        Profile? prof = null;
        if (!string.IsNullOrWhiteSpace(rule.useProfile) && config.profiles != null)
        {
            config.profiles.TryGetValue(rule.useProfile!, out prof);
        }

        // Merge: rule overrides profile
        string? browser = FirstNonEmpty(rule.browser, prof?.browser);
        string? argsTemplate = FirstNonEmpty(rule.argsTemplate, prof?.argsTemplate);
        string? browserProfile = FirstNonEmpty(rule.profile, prof?.profile);
        string? userDataDir = FirstNonEmpty(rule.userDataDir, prof?.userDataDir);
        string? workingDirectory = FirstNonEmpty(rule.workingDirectory, prof?.workingDirectory);

        if (string.IsNullOrWhiteSpace(browser))
        {
            throw new InvalidOperationException($"No browser specified after resolving profile '{rule.useProfile}'. Provide 'browser' in the rule/default or in the referenced profile.");
        }
        if (string.IsNullOrWhiteSpace(argsTemplate))
        {
            throw new InvalidOperationException($"No argsTemplate specified after resolving profile '{rule.useProfile}'. Provide 'argsTemplate' in the rule/default or in the referenced profile.");
        }

        return new Rule(
            match: rule.match,
            pattern: rule.pattern,
            browser: browser,
            argsTemplate: argsTemplate,
            profile: browserProfile,
            userDataDir: userDataDir,
            workingDirectory: workingDirectory,
            useProfile: rule.useProfile
        );
    }

    private static string? FirstNonEmpty(string? a, string? b)
    {
        return !string.IsNullOrWhiteSpace(a) ? a : (!string.IsNullOrWhiteSpace(b) ? b : null);
    }
}
