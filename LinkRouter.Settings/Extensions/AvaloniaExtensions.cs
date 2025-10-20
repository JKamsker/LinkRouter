using System.Collections.Generic;
using Avalonia;
using Avalonia.VisualTree;

namespace LinkRouter.Settings.Extensions;

public static class AvaloniaExtensions
{
    // Enumerate all .GetVisualChildren() recursively
    public static IEnumerable<Visual> GetVisualDescendants(this Visual visual)
    {
        foreach (var child in visual.GetVisualChildren())
        {
            yield return child;

            foreach (var descendant in child.GetVisualDescendants())
            {
                yield return descendant;
            }
        }
    }


}
