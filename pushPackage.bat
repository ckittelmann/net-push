@echo off
set /p apiKey="Enter ApiKey: "
nuget push NetPush.0.0.2.nupkg -ApiKey %apiKey% -Source https://www.nuget.org/api/v2/package
PAUSE