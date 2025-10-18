namespace LinkRouter.Settings.Core.Infrastructure;

public interface ILauncherService
{
    void OpenFolder(string path);
    void OpenFile(string path);
    void OpenUri(string uri);
}
