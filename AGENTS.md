# Building
dotnet build doent work with winui nicely - use this instead:
```
Import-Module 'C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\Microsoft.VisualStudio.DevShell.dll'; Enter-VsDevShell 3e25b51d; cd C:\Users\Jonas\repos\private\LinkRouter; msbuild LinkRouter.sln /t:Rebuild /p:Configuration=Release
```