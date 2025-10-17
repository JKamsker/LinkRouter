using System.Xml.Linq;

namespace LinkRouter.Settings.Tests;

/// <summary>
/// Lightweight XAML validation without bootstrapping WinUI runtime.
/// Ensures files parse and avoid known invalid resources/icons.
/// </summary>
public class XamlPageTests
{
    private static string RepoRoot => FindRepoRoot();
    private static string Views(string file) => Path.Combine(RepoRoot, "LinkRouter.Settings", "Views", file);
    private static string Controls(string file) => Path.Combine(RepoRoot, "LinkRouter.Settings", "Controls", file);

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "LinkRouter.sln")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        // Fallback to test project directory if solution not found
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
    }

    public static IEnumerable<object[]> XamlFiles()
    {
        yield return new object[] { Views("GeneralPage.xaml") };
        yield return new object[] { Views("RulesPage.xaml") };
        yield return new object[] { Views("ProfilesPage.xaml") };
        yield return new object[] { Views("DefaultPage.xaml") };
        yield return new object[] { Views("ImportExportPage.xaml") };
        yield return new object[] { Views("AdvancedPage.xaml") };
        yield return new object[] { Views("AboutPage.xaml") };
        yield return new object[] { Controls("SettingsCard.xaml") };
    }

    [Theory]
    [MemberData(nameof(XamlFiles))]
    public void Xaml_Should_Parse_As_Xml(string path)
    {
        Assert.True(File.Exists(path), $"XAML file missing: {path}");
        var doc = XDocument.Load(path, LoadOptions.SetLineInfo);
        Assert.NotNull(doc.Root);
    }

    [Theory]
    [MemberData(nameof(XamlFiles))]
    public void Xaml_Should_Not_Use_Known_Invalid_Resources(string path)
    {
        var text = File.ReadAllText(path);
        // Invalid icon syntax from earlier iterations
        Assert.DoesNotContain("Icon=\"Up\"", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Icon=\"Down\"", text, StringComparison.OrdinalIgnoreCase);

        // Invalid resource key (replaced by SystemFillColorCriticalBrush)
        Assert.DoesNotContain("TextFillColorCriticalBrush", text, StringComparison.Ordinal);
    }
}
