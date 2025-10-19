namespace LinkRouter.Settings.Avalonia.ViewModels;

public sealed class NavigationItemViewModel
{
    public NavigationItemViewModel(string key, string title, object content)
    {
        Key = key;
        Title = title;
        Content = content;
    }

    public string Key { get; }

    public string Title { get; }

    public object Content { get; }
}
