using System;

namespace LinkRouter;

public static class UrlNormalizer
{
    public static (bool success, Uri? uri, string? error) NormalizeAndValidate(string rawUrl)
    {
        // Normalize: ensure scheme exists
        if (!rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            rawUrl = "https://" + rawUrl;
        }

        // Basic validation
        if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out var uri))
        {
            return (false, null, "Invalid URL.");
        }

        return (true, uri, null);
    }
}
