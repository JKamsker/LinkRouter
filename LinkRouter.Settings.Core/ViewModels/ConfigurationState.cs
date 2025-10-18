using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using LinkRouter;
using LinkRouter.Settings.Services;

namespace LinkRouter.Settings.ViewModels;

public sealed class ConfigurationState : ObservableObject
{
    private bool _isDirty;
    private ConfigDocument? _document;

    public ObservableCollection<RuleEditorViewModel> Rules { get; } = new();
    public ObservableCollection<ProfileEditorViewModel> Profiles { get; } = new();
    public DefaultRuleViewModel DefaultRule { get; } = new();

    public ConfigDocument? Document
    {
        get => _document;
        private set => SetProperty(ref _document, value);
    }

    public bool HasUnsavedChanges
    {
        get => _isDirty;
        private set => SetProperty(ref _isDirty, value);
    }

    public event EventHandler? StateChanged;

    public void Load(ConfigDocument document)
    {
        Unsubscribe(Rules);
        Unsubscribe(Profiles);

        Rules.Clear();
        Profiles.Clear();

        foreach (var rule in document.Config.rules)
        {
            var vm = new RuleEditorViewModel(rule);
            vm.PropertyChanged += OnChildPropertyChanged;
            Rules.Add(vm);
        }

        if (document.Config.profiles is not null)
        {
            foreach (var kvp in document.Config.profiles)
            {
                var vm = new ProfileEditorViewModel(kvp.Key, kvp.Value);
                vm.PropertyChanged += OnChildPropertyChanged;
                Profiles.Add(vm);
            }
        }

        DefaultRule.PropertyChanged -= OnChildPropertyChanged;
        DefaultRule.Load(document.Config.@default);
        DefaultRule.PropertyChanged += OnChildPropertyChanged;

        Document = document;
        HasUnsavedChanges = false;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public Config BuildConfig()
    {
        var rules = Rules.Select(rule => rule.ToRule()).ToArray();
        var defaultRule = DefaultRule.ToRuleOrNull();
        Dictionary<string, Profile>? profiles = null;
        if (Profiles.Count > 0)
        {
            profiles = Profiles
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .ToDictionary(p => p.Name, p => p.ToProfile(), StringComparer.OrdinalIgnoreCase);
        }

        return new Config(rules, defaultRule, profiles);
    }

    public void MarkSaved()
    {
        HasUnsavedChanges = false;
    }

    public void MarkDirty()
    {
        HasUnsavedChanges = true;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddRule(RuleEditorViewModel rule)
    {
        rule.PropertyChanged += OnChildPropertyChanged;
        Rules.Add(rule);
        MarkDirty();
    }

    public void RemoveRule(RuleEditorViewModel rule)
    {
        rule.PropertyChanged -= OnChildPropertyChanged;
        Rules.Remove(rule);
        MarkDirty();
    }

    public void MoveRule(int oldIndex, int newIndex)
    {
        if (oldIndex == newIndex || oldIndex < 0 || newIndex < 0 || oldIndex >= Rules.Count || newIndex >= Rules.Count)
        {
            return;
        }

        var item = Rules[oldIndex];
        Rules.RemoveAt(oldIndex);
        Rules.Insert(newIndex, item);
        MarkDirty();
    }

    public void AddProfile(ProfileEditorViewModel profile)
    {
        profile.PropertyChanged += OnChildPropertyChanged;
        Profiles.Add(profile);
        MarkDirty();
    }

    public void RemoveProfile(ProfileEditorViewModel profile)
    {
        profile.PropertyChanged -= OnChildPropertyChanged;
        Profiles.Remove(profile);
        MarkDirty();
    }

    private void OnChildPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        HasUnsavedChanges = true;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Unsubscribe<T>(IEnumerable<T> items) where T : INotifyPropertyChanged
    {
        foreach (var item in items)
        {
            item.PropertyChanged -= OnChildPropertyChanged;
        }
    }
}
