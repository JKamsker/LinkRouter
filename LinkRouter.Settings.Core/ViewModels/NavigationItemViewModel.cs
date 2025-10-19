namespace LinkRouter.Settings.ViewModels;

public sealed class NavigationItemViewModel
{
    public NavigationItemViewModel(string key, string title, object content, string? icon = null)
    {
        Key = key;
        Title = title;
        Content = content;
        Icon = icon;
    }

    public string Key { get; }

    public string Title { get; }

    public object Content { get; }

    public string? Icon { get; }
}
