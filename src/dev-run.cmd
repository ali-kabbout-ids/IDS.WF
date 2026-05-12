@echo off
setlocal
cd /d "%~dp0"

echo Stopping Rwd.WF.API if running (fixes MSB3021 file locks)...
taskkill /IM Rwd.WF.API.exe /F >nul 2>&1

echo Building...
dotnet build Rwd.WF.API\Rwd.WF.API.csproj || exit /b 1

echo.
echo Starting API on http://127.0.0.1:5100
echo   Swagger UI:     http://127.0.0.1:5100/
echo   swagger.json:   http://127.0.0.1:5100/swagger/v1/swagger.json
echo   Elsa OpenAPI:   http://127.0.0.1:5100/elsa/swagger
echo   DB check:       http://127.0.0.1:5100/internal/debug/elsa-persistence
echo.
echo Press Ctrl+C to stop.
echo.

dotnet run --project Rwd.WF.API\Rwd.WF.API.csproj --urls "http://127.0.0.1:5100"
