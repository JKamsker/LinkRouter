using FluentAvalonia.UI.Controls;

namespace LinkRouter.Settings.ViewModels;

public sealed class NavigationItemViewModel
{
    public NavigationItemViewModel(string key, string title, object content, Symbol icon)
    {
        Key = key;
        Title = title;
        Content = content;
        Icon = icon;
    }

    public string Key { get; }

    public string Title { get; }

    public object Content { get; }

    public Symbol Icon { get; }

    public override string ToString() => Title;
}
