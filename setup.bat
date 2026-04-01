@echo off
REM GAMINGSTORE - Automated Setup Script
REM For Windows users - Run this as Administrator

setlocal enabledelayedexpansion

echo.
echo ============================================
echo  GAMINGSTORE - Automated Setup
echo ============================================
echo.

REM Check .NET SDK
echo [1/5] Checking .NET 8.0 SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET 8.0 SDK not found!
    echo Download from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)
echo [OK] .NET SDK found
echo.

REM Check SQL Server
echo [2/5] Checking SQL Server connection...
sqlcmd -S .\\SQLEXPRESS -Q "SELECT 1" >nul 2>&1
if errorlevel 1 (
    echo WARNING: Cannot connect to SQL Server
    echo Make sure SQL Server is running (services.msc)
    echo Server: .\\SQLEXPRESS
    pause
)
echo [OK] SQL Server accessible
echo.

REM Restore NuGet packages
echo [3/5] Restoring NuGet packages...
dotnet restore GAMINGSTORE\GAMINGSTORE.csproj
if errorlevel 1 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)
echo [OK] Packages restored
echo.

REM Install EF Core CLI
echo [4/5] Installing Entity Framework Core tools...
dotnet tool install --global dotnet-ef >nul 2>&1
if errorlevel 1 (
    echo WARNING: Could not install dotnet-ef (may already be installed)
)
echo [OK] EF Core ready
echo.

REM Update database
echo [5/5] Creating and seeding database...
cd GAMINGSTORE\GAMINGSTORE
dotnet ef database update
if errorlevel 1 (
    echo ERROR: Failed to update database
    echo Make sure:
    echo 1. SQL Server is running
    echo 2. Connection string in appsettings.Development.json is correct
    echo 3. Database GAMINGSTORE doesn't already exist
    cd ..
    pause
    exit /b 1
)
cd ..
echo [OK] Database created and seeded
echo.

echo ============================================
echo  Setup completed successfully!
echo ============================================
echo.
echo Run application with: cd GAMINGSTORE && dotnet run
echo Or press any key to start now...
pause

cd GAMINGSTORE
dotnet run
pause
