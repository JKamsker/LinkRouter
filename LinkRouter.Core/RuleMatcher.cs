using System;
using System.Text.RegularExpressions;

namespace LinkRouter;

public static class RuleMatcher
{
    public static Rule? FindMatchingRule(Config config, Uri uri)
    {
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
                        return rule;
                    }
                }
                else if (rule.match.Equals("regex", StringComparison.OrdinalIgnoreCase))
                {
                    if (Regex.IsMatch(uri.ToString(), rule.pattern, RegexOptions.IgnoreCase))
                    {
                        return rule;
                    }
                }
                else if (rule.match.Equals("contains", StringComparison.OrdinalIgnoreCase))
                {
                    if (uri.ToString().IndexOf(rule.pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return rule;
                    }
                }
            }
            catch (Exception)
            {
                // ignore faulty patterns
            }
        }

        return config.@default is { Enabled: true } ? config.@default : null;
    }

    private static bool MatchesDomain(string host, string pattern)
    {
        return string.Equals(host, pattern, StringComparison.OrdinalIgnoreCase) ||
               host.EndsWith("." + pattern, StringComparison.OrdinalIgnoreCase);
    }
}
