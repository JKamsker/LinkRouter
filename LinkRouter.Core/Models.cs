namespace LinkRouter;

public record Profile(
    string? browser,
    string? argsTemplate,
    string? profile,
    string? userDataDir,
    string? workingDirectory);

public record Rule(
    string match,
    string pattern,
    string? browser,
    string? argsTemplate,
    string? profile,
    string? userDataDir,
    string? workingDirectory,
    string? useProfile,
    bool Enabled = true);

public record Config(Rule[] rules, Rule? @default, Dictionary<string, Profile>? profiles);
