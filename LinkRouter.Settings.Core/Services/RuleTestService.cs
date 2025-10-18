using System;
using System.Text.RegularExpressions;
using LinkRouter;

namespace LinkRouter.Settings.Core.Services;

public sealed class RuleTestService
{
    private static readonly TimeSpan DefaultRegexTimeout = TimeSpan.FromMilliseconds(250);

    public RuleTestResult Test(Config config, string url, TimeSpan? regexTimeout = null)
    {
        var (normalized, uri, error) = UrlNormalizer.NormalizeAndValidate(url);
        if (!normalized || uri is null)
        {
            return RuleTestResult.Failure(error ?? "Invalid URL");
        }

        var timeout = regexTimeout ?? DefaultRegexTimeout;
        Rule? matchedRule = null;
        Exception? ruleError = null;

        foreach (var rule in config.rules)
        {
            if (!rule.Enabled)
            {
                continue;
            }

            try
            {
                if (rule.match.Equals("domain", StringComparison.OrdinalIgnoreCase))
                {
                    if (MatchesDomain(uri.Host, rule.pattern))
                    {
                        matchedRule = rule;
                        break;
                    }
                }
                else if (rule.match.Equals("contains", StringComparison.OrdinalIgnoreCase))
                {
                    if (uri.ToString().IndexOf(rule.pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        matchedRule = rule;
                        break;
                    }
                }
                else if (rule.match.Equals("regex", StringComparison.OrdinalIgnoreCase))
                {
                    var regex = new Regex(rule.pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, timeout);
                    if (regex.IsMatch(uri.ToString()))
                    {
                        matchedRule = rule;
                        break;
                    }
                }
            }
            catch (RegexMatchTimeoutException ex)
            {
                ruleError = ex;
                return RuleTestResult.Failure($"Regex timeout for pattern '{rule.pattern}'. Consider simplifying the expression.");
            }
            catch (ArgumentException ex)
            {
                ruleError = ex;
                return RuleTestResult.Failure($"Invalid regex pattern '{rule.pattern}': {ex.Message}");
            }
        }

        Rule? fallback = config.@default is { Enabled: true } ? config.@default : null;
        Rule targetRule = matchedRule ?? fallback ?? throw new InvalidOperationException("No matching rule or default rule configured.");

        try
        {
            var effective = ProfileResolver.ResolveEffectiveRule(config, targetRule);
            string launchArgs = BrowserLauncher.GetLaunchArguments(effective, uri);
            return RuleTestResult.SuccessResult(uri.ToString(), matchedRule, effective, launchArgs);
        }
        catch (Exception ex)
        {
            return RuleTestResult.Failure(ex.Message, uri.ToString(), matchedRule);
        }
    }

    private static bool MatchesDomain(string host, string pattern)
    {
        return string.Equals(host, pattern, StringComparison.OrdinalIgnoreCase) ||
               host.EndsWith($".{pattern}", StringComparison.OrdinalIgnoreCase);
    }
}

public sealed record RuleTestResult
{
    private RuleTestResult(bool success, string? error, string? normalizedUrl, Rule? matchedRule, Rule? effectiveRule, string? launchArguments)
    {
        Success = success;
        Error = error;
        NormalizedUrl = normalizedUrl;
        MatchedRule = matchedRule;
        EffectiveRule = effectiveRule;
        LaunchArguments = launchArguments;
    }

    public bool Success { get; }
    public string? Error { get; }
    public string? NormalizedUrl { get; }
    public Rule? MatchedRule { get; }
    public Rule? EffectiveRule { get; }
    public string? LaunchArguments { get; }

    public static RuleTestResult SuccessResult(string normalizedUrl, Rule? matched, Rule effective, string launchArguments) =>
        new(true, null, normalizedUrl, matched, effective, launchArguments);

    public static RuleTestResult Failure(string message, string? normalizedUrl = null, Rule? matched = null) =>
        new(false, message, normalizedUrl, matched, null, null);
}
