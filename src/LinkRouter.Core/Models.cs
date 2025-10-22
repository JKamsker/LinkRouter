namespace LinkRouter;

public record Profile(
    string? browser,
    string? argsTemplate,
    string? profile,
    string? userDataDir,
    string? workingDirectory,
    bool incognito = false);

public record Rule(
    string match,
    string pattern,
    string? browser,
    string? argsTemplate,
    string? profile,
    string? userDataDir,
    string? workingDirectory,
    string? useProfile,
    bool? incognito = null,
    bool Enabled = true);

public record Config(Rule[] rules, Rule? @default, Dictionary<string, Profile>? profiles);
