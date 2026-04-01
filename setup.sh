#!/bin/bash

# GAMINGSTORE - Automated Setup Script
# For Linux & macOS users

set -e

echo ""
echo "============================================"
echo "  GAMINGSTORE - Automated Setup"
echo "============================================"
echo ""

# Check .NET SDK
echo "[1/5] Checking .NET 8.0 SDK..."
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET 8.0 SDK not found!"
    echo "Download from: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi
dotnet_version=$(dotnet --version)
echo "[OK] .NET SDK found: $dotnet_version"
echo ""

# Check SQL Server or use Docker alternative
echo "[2/5] Checking database connectivity..."
if command -v sqlcmd &> /dev/null; then
    sqlcmd -S localhost -Q "SELECT 1" > /dev/null 2>&1
    echo "[OK] SQL Server found"
else
    echo "[WARNING] sqlcmd not found. Make sure SQL Server is running."
    echo "Options:"
    echo "  1. Install SQL Server for Linux/macOS"
    echo "  2. Use Docker: docker run -e ACCEPT_EULA=Y -e SA_PASSWORD=Admin@123 -p 1433:1433 mcr.microsoft.com/mssql/server"
    echo "  3. Use remote SQL Server"
    read -p "Continue anyway? (y/n) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi
echo ""

# Restore NuGet packages
echo "[3/5] Restoring NuGet packages..."
dotnet restore GAMINGSTORE/GAMINGSTORE.csproj
echo "[OK] Packages restored"
echo ""

# Install EF Core CLI
echo "[4/5] Installing Entity Framework Core tools..."
dotnet tool install --global dotnet-ef 2>/dev/null || echo "[OK] dotnet-ef already installed"
echo ""

# Update database
echo "[5/5] Creating and seeding database..."
cd GAMINGSTORE/GAMINGSTORE
if dotnet ef database update; then
    echo "[OK] Database created and seeded"
else
    echo "ERROR: Failed to update database"
    echo "Make sure:"
    echo "  1. SQL Server is running"
    echo "  2. Connection string in appsettings.Development.json is correct"
    echo "  3. Database GAMINGSTORE doesn't already exist"
    exit 1
fi
cd ../..
echo ""

echo "============================================"
echo "  Setup completed successfully!"
echo "============================================"
echo ""
echo "Run application with:"
echo "  cd GAMINGSTORE && dotnet run"
echo ""
echo "Application will be available at:"
echo "  http://localhost:5190"
echo ""

read -p "Start application now? (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    cd GAMINGSTORE
    dotnet run
fi
