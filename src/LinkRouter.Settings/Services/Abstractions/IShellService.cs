namespace LinkRouter.Settings.Services.Abstractions;

public interface IShellService
{
    void OpenFolder(string path);
    void OpenFile(string path);
    void OpenUri(string uri);
}
