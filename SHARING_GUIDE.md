# GAMINGSTORE - Setup Guide for Developers

## 🚀 Quick Start Guide for Sharing with Friends

Hướng dẫn này giúp bạn bè clone code và setup database GAMINGSTORE một cách dễ nhất.

---

## 📋 METHOD 1: Using GitHub + SQL Migrations (RECOMMENDED)

**Ưu điểm**: Không cần upload database, automatic migration
**Thời gian setup**: ~5 phút

### Step 1: Clone Repository
```bash
git clone https://github.com/cuong12354/GAMINGSTORE.git
cd GAMINGSTORE/GAMINGSTORE
```

### Step 2: Prerequisites
Bạn bè cần cài:
- **Visual Studio 2022** hoặc **VS Code**
- **.NET 8.0 SDK** (download từ https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server 2019+** hoặc **SQL Server Express**
  - Download: https://www.microsoft.com/sql-server/sql-server-downloads

### Step 3: Database Connection String
Sửa file `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=GAMINGSTORE;Trusted_Connection=true;TrustServerCertificate=true"
  }
}
```

**Tìm SQL Server Name của bạn:**
```bash
# Mở Command Prompt:
sqlcmd -L
# Hoặc dùng SQL Server Management Studio (SSMS)
# để xem server name
```

**Hoặc nếu dùng SQLEXPRESS (default):**
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=GAMINGSTORE;Trusted_Connection=true;TrustServerCertificate=true"
```

### Step 4: Restore Database (Auto-Create)
```bash
# Ở thư mục GAMINGSTORE/GAMINGSTORE

# Update/Create database từ migrations
dotnet ef database update

# Nếu chưa cài EF CLI:
dotnet tool install --global dotnet-ef
dotnet ef database update
```

**Kết quả**: Database `GAMINGSTORE` sẽ được tạo tự động với:
- ✅ All tables (Products, Categories, Orders, etc.)
- ✅ Identity tables (Users, Roles)
- ✅ Sample data (20 categories, 40+ products)

### Step 5: Run Application
```bash
dotnet run
# Hoặc F5 trong Visual Studio
```

**App sẽ chạy tại**: http://localhost:5190

---

## 💾 METHOD 2: Database Backup File (Fastest for Friends)

**Ưu điểm**: Siêu nhanh, không cần chỉnh config
**Thời gian setup**: ~2 phút

### Step 1: Tạo Backup File
```bash
# Trên máy của bạn, mở Command Prompt/PowerShell as Administrator:

# Tìm SQL Server instance path
cd "C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup"

# Hoặc dùng SQL Server Management Studio:
# Right-click "GAMINGSTORE" → Tasks → Back Up
# Save to: C:\Backups\GAMINGSTORE.bak
```

### Step 2: Share File
```powershell
# Copy backup file
Copy-Item "C:\Backups\GAMINGSTORE.bak" "D:\Share\GAMINGSTORE.bak"

# Chia sẻ via:
# - Google Drive
# - OneDrive
# - USB drive
# - Dropbox
```

### Step 3: Friend Restores Database
**Trên máy bạn bè:**

```bash
# Mở SQL Server Management Studio → New Query:

RESTORE DATABASE GAMINGSTORE 
FROM DISK = N'C:\Backups\GAMINGSTORE.bak'
WITH REPLACE;
```

**Hoặc dùng Command Line:**
```bash
sqlcmd -S .\\SQLEXPRESS -Q "RESTORE DATABASE GAMINGSTORE FROM DISK = 'C:\Backups\GAMINGSTORE.bak' WITH REPLACE"
```

### Step 4: Done! Start Application
```bash
dotnet run
```

---

## 🐳 METHOD 3: Docker (Ultimate Sharing Solution)

**Ưu điểm**: Works on any machine, no SQL Server install needed
**Thời gian setup**: ~5 phút (first time), ~30 seconds (next time)

### Step 1: Tạo Dockerfile
Tạo file `Dockerfile` ở root của project:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["GAMINGSTORE/GAMINGSTORE.csproj", "GAMINGSTORE/"]
RUN dotnet restore "GAMINGSTORE/GAMINGSTORE.csproj"

COPY . .
WORKDIR "/src/GAMINGSTORE"
RUN dotnet build "GAMINGSTORE.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GAMINGSTORE.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5190
ENV ASPNETCORE_URLS=http://+:5190
ENTRYPOINT ["dotnet", "GAMINGSTORE.dll"]
```

### Step 2: Tạo docker-compose.yml
```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      ACCEPT_EULA: Y
      SA_PASSWORD: "Admin@123"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  gamingstore:
    build: .
    environment:
      ConnectionStrings__DefaultConnection: "Server=sqlserver;Database=GAMINGSTORE;User ID=sa;Password=Admin@123;TrustServerCertificate=true"
    ports:
      - "5190:5190"
    depends_on:
      - sqlserver
    
volumes:
  sqlserver_data:
```

### Step 3: Share với Bạn Bè
```bash
# Push lên GitHub (cả Dockerfile và docker-compose.yml)
git add Dockerfile docker-compose.yml
git commit -m "Add Docker support for easy setup"
git push
```

### Step 4: Bạn Bè Setup (Ultra Simple)
```bash
# Chỉ cần 2 dòng lệnh!
git clone https://github.com/cuong12354/GAMINGSTORE.git
cd GAMINGSTORE
docker-compose up --build

# Sau 1-2 phút, truy cập: http://localhost:5190
```

---

## 📊 METHOD 4: SQL Script Export (For Sharing Schema Only)

**Ưu điểm**: Bạn bè có thể xem/hiểu schema
**Ích lợi**: Documentation, learning

### Step 1: Generate SQL Script
```bash
# Trong SQL Server Management Studio:
# 1. Right-click GAMINGSTORE database
# 2. Tasks → Generate Scripts
# 3. Select: Schema and Data
# 4. Advanced → Types of Data to Script → Schema and Data
# 5. Save as: GAMINGSTORE_Script.sql
```

**Hoặc dùng Command Line:**
```bash
# PowerShell (bạn bè chạy)
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SMO") | out-null
$srv = New-Object Microsoft.SqlServer.Management.Smo.Server(".")
$db = $srv.Databases["GAMINGSTORE"]
$scrps = New-Object Microsoft.SqlServer.Management.Smo.Scripter($srv)
$scrps.Options.ScriptDrops = $false
$scrps.Options.WithDependencies = $true
$scrps.Options.ScriptData = $true
$scrps.EnumScript($db.Tables) | Out-File -FilePath "GAMINGSTORE_Script.sql"
```

### Step 2: Share SQL Script
```bash
# Upload to GitHub
git add GAMINGSTORE_Script.sql
git commit -m "Add database schema and data export"
git push

# Bạn bè tạo database mới rồi execute script:
sqlcmd -S .\\SQLEXPRESS -i GAMINGSTORE_Script.sql
```

---

## 🔄 Detailed Setup Instructions for Friends

### Complete Setup Checklist

```markdown
# GAMINGSTORE Setup Checklist

## Prerequisites ✅
- [ ] .NET 8.0 SDK installed
  Download: https://dotnet.microsoft.com/download/dotnet/8.0

- [ ] SQL Server installed (2019+ or Express)
  Download: https://www.microsoft.com/sql-server/sql-server-downloads
  
- [ ] Git installed
  Download: https://git-scm.com/

- [ ] Visual Studio Code or Visual Studio 2022
  Download: https://code.visualstudio.com/ or https://visualstudio.microsoft.com/

## Setup Steps

### 1. Clone Repository (2 min)
\`\`\`bash
git clone https://github.com/cuong12354/GAMINGSTORE.git
cd GAMINGSTORE/GAMINGSTORE
\`\`\`

### 2. Check PHP/Nuget Dependencies (1 min)
\`\`\`bash
dotnet restore
\`\`\`

### 3. Update appsettings.Development.json (1 min)
Edit: `GAMINGSTORE/GAMINGSTORE/appsettings.Development.json`

Replace connection string with your SQL Server:
\`\`\`json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=GAMINGSTORE;Trusted_Connection=true;TrustServerCertificate=true"
\`\`\`

**Alternative** (if using named instance):
\`\`\`json
"DefaultConnection": "Server=COMPUTER_NAME\\SQLEXPRESS;Database=GAMINGSTORE;Trusted_Connection=true;TrustServerCertificate=true"
\`\`\`

### 4. Install EF Core CLI (1 min)
\`\`\`bash
dotnet tool install --global dotnet-ef
\`\`\`

### 5. Create & Seed Database (2 min)
\`\`\`bash
dotnet ef database update
\`\`\`

This will:
- ✅ Create GAMINGSTORE database
- ✅ Run all migrations
- ✅ Seed sample data (20 categories, 40+ products)

### 6. Start Application (30 sec)
\`\`\`bash
dotnet run
\`\`\`

Navigate to: http://localhost:5190

### 7. Test Application ✅
- [ ] Homepage loads with products
- [ ] Browse products in grid
- [ ] Filter by category
- [ ] Search products
- [ ] Add products to cart
- [ ] Proceed to checkout

## Troubleshooting

**Issue**: "Cannot connect to SQL Server"
**Solution**: 
1. Check if SQL Server is running
2. Verify server name: \`sqlcmd -L\`
3. Update connection string

**Issue**: "Database update failed"
**Solution**:
1. Delete database manually in SSMS
2. Run \`dotnet ef database update\` again

**Issue**: "Port 5190 already in use"
**Solution**:
1. Change port in Properties/launchSettings.json
2. Or kill process: \`netstat -ano | findstr :5190\`

## Contacts
Questions? Ask in project GitHub issues or contact: [your email]
```

---

## 📦 Package Complete Share Bundle

Create a folder structure to share everything:

```bash
# Create share folder
mkdir GAMINGSTORE_Share
cd GAMINGSTORE_Share

# Copy everything
cp -r ../GAMINGSTORE ./
cp SETUP_GUIDE.md ./

# Create README for quick start
echo "# Quick Start
See SETUP_GUIDE.md for complete instructions.
Or use Docker: docker-compose up --build" > README.md

# Zip it up
# On Windows: Right-click → Send to → Compressed folder
# Or use: Compress-Archive -Path GAMINGSTORE_Share -DestinationPath GAMINGSTORE_Share.zip

# Share via Google Drive, OneDrive, Dropbox, etc.
```

---

## 🌐 Online Sharing Options

### Option 1: GitHub (Recommended - Free & Always Updated)
```bash
git push origin main
# Share: https://github.com/cuong12354/GAMINGSTORE
```

**Bạn bè chỉ cần:**
```bash
git clone https://github.com/cuong12354/GAMINGSTORE.git
```

### Option 2: Google Drive
```bash
# Create folder "GAMINGSTORE"
# Upload:
# - Entire project folder
# - Or just GAMINGSTORE_Share.zip
# Share link with friends
```

### Option 3: OneDrive / Dropbox
Same as Google Drive - upload and share link

### Option 4: USB Drive
Copy entire project folder + this guide

---

## 📝 Quick Reference Commands

```bash
# Navigate to project
cd GAMINGSTORE/GAMINGSTORE

# Restore dependencies
dotnet restore

# Build project
dotnet build

# Update database from migrations
dotnet ef database update

# Create new migration (if schema changes)
dotnet ef migrations add "MigrationName"

# Run application
dotnet run

# Clean build
dotnet clean
dotnet build
```

---

## 🎯 Summary: Best Methods by Scenario

| Scenario | Method | Setup Time |
|----------|--------|-----------|
| GitHub + Automatic Sync | Method 1: Migrations | 5 min |
| Fastest, No Config | Method 2: Backup File | 2 min |
| Any Machine, Any OS | Method 3: Docker | 5 min |
| Learning Schema | Method 4: SQL Script | 3 min |
| All Friends at Once | Method 1 + GitHub | 5 min |
| Quick Demo | Method 3 + Docker | 1 min |

---

## ✨ Pro Tips

1. **Version Control**: Keep project on GitHub for everyone to stay in sync
2. **Connection Strings**: Use `appsettings.Development.json` for local settings (don't commit secrets)
3. **Migrations**: Use EF migrations instead of manual SQL scripts for consistency
4. **Docker**: Best if friends use Windows/Mac/Linux mix
5. **Setup Variables**: Create a `setup.sh` or `setup.bat` script for automation

---

## 🚀 Next Steps

After friends have it running:
- [ ] Everyone forks repository (for contributing)
- [ ] Setup branch protection rules
- [ ] Create pull request workflow
- [ ] Setup CI/CD pipeline
- [ ] Deploy to production (Azure, AWS, Heroku, etc.)

---

## 📞 Support

If friends have issues:
1. Check this guide first
2. Check GitHub Issues
3. Share error message + system info
4. Suggest using Docker if all else fails
