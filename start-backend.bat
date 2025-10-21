@echo off
echo ============================================
echo   ChillNest Backend - Hot Reload Mode
echo ============================================
echo.
echo Backend dang chay tai: http://0.0.0.0:5182
echo.
echo API URL cho Android:
echo   - Emulator: http://10.0.2.2:5182/api
echo   - Device:   http://[YOUR_IP]:5182/api
echo.
echo Nhan Ctrl+C de stop
echo ============================================
echo.

cd /d "%~dp0BoookingHotels"
dotnet watch run --urls "http://0.0.0.0:5182"

pause
