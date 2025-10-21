publish:
dotnet publish .\LinkRouter.Launcher\LinkRouter.Launcher.csproj -c Release /p:PublishProfile=NativeAot-win-x64

dotnet publish .\LinkRouter.Launcher\LinkRouter.Launcher.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true
