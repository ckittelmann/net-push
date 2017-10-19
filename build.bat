nuget restore
dotnet clean -c Release
dotnet build -c Release
nuget pack NetPush.nuspec
PAUSE