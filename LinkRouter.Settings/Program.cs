using System;
using Microsoft.UI.Xaml;

namespace LinkRouter.Settings;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();
        Application.Start(p => new App());
    }
}

