@ECHO OFF

dotnet run -c Release --no-restore -- %*
pause