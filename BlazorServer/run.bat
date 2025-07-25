start "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" "http://localhost:5000"
c:
cd /D "%~dp0"
dotnet run -c Release
pause