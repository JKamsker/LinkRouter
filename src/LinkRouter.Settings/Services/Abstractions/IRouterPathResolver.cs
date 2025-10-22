using System.Diagnostics.CodeAnalysis;

namespace LinkRouter.Settings.Services.Abstractions;

public interface IRouterPathResolver
{
    bool TryGetRouterExecutable([NotNullWhen(true)] out string? path);
}
