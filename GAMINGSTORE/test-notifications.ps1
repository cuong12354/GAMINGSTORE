# Comprehensive Notifications System Test

$baseUrl = "http://localhost:5190"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "NOTIFICATIONS SYSTEM TEST" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: API Endpoint Connectivity
Write-Host "TEST 1: Checking API Connectivity..." -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/notification/unread-count" -Method GET -ErrorAction Stop
    Write-Host "OK - API endpoint accessible (Status: $($response.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "ERROR - API endpoint error" -ForegroundColor Red
    exit 1
}

# Test 2: Database Connection
Write-Host ""
Write-Host "TEST 2: Checking Database..." -ForegroundColor Yellow

$connectionString = "Server=DESKTOP-7HEJ3VI\SQLEXPRESS;Database=GAMINGSTORE;Trusted_Connection=True;TrustServerCertificate=True"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    Write-Host "OK - Database connection successful" -ForegroundColor Green
    
    # Check notification tables
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT (SELECT COUNT(*) FROM CustomerNotifications) as N1, (SELECT COUNT(*) FROM NotificationLogs) as N2, (SELECT COUNT(*) FROM NotificationTemplates) as N3"
    $reader = $cmd.ExecuteReader()
    if ($reader.Read()) {
        $notCount = $reader["N1"]
        $logCount = $reader["N2"]
        $templCount = $reader["N3"]
        Write-Host "  - CustomerNotifications: $notCount records"
        Write-Host "  - NotificationLogs: $logCount records"
        Write-Host "  - NotificationTemplates: $templCount records"
    }
    $reader.Close()
    $connection.Close()
} catch {
    Write-Host "ERROR - Database connection failed" -ForegroundColor Red
    exit 1
}

# Test 3: Service Layer
Write-Host ""
Write-Host "TEST 3: Checking NotificationService Registration..." -ForegroundColor Yellow

try {
    $program = Get-Content "d:\SaveGame\GAMINGSTORE\GAMINGSTORE\Program.cs" -Raw
    if ($program -match "AddScoped.*INotificationService.*NotificationService") {
        Write-Host "OK - INotificationService registered in DI container" -ForegroundColor Green
    } else {
        Write-Host "ERROR - INotificationService NOT found in Program.cs" -ForegroundColor Red
    }
} catch {
    Write-Host "ERROR - Cannot check Program.cs" -ForegroundColor Red
}

# Test 4: Configuration
Write-Host ""
Write-Host "TEST 4: Checking SMTP Configuration..." -ForegroundColor Yellow

try {
    $config = Get-Content "d:\SaveGame\GAMINGSTORE\GAMINGSTORE\appsettings.json" | ConvertFrom-Json
    if ($config.Smtp) {
        Write-Host "OK - SMTP configuration found" -ForegroundColor Green
        Write-Host "  - Server: $($config.Smtp.Server)"
        Write-Host "  - Port: $($config.Smtp.Port)"
        Write-Host "  - SenderName: $($config.Smtp.SenderName)"
        
        if ($config.Smtp.SenderEmail -like "*your-email*") {
            Write-Host "  - WARNING: Email address is placeholder!" -ForegroundColor Yellow
        } else {
            Write-Host "  - SenderEmail: $($config.Smtp.SenderEmail)"
        }
    } else {
        Write-Host "ERROR - SMTP configuration NOT found" -ForegroundColor Red
    }
} catch {
    Write-Host "ERROR - Cannot check appsettings" -ForegroundColor Red
}

# Test 5: UI Components
Write-Host ""
Write-Host "TEST 5: Checking UI Components..." -ForegroundColor Yellow

try {
    $loginPartial = Get-Content "d:\SaveGame\GAMINGSTORE\GAMINGSTORE\Views\Shared\_LoginPartial.cshtml" -Raw
    
    $components = @(
        "bi-bell-fill",
        "notification-dropdown",
        "notification-badge",
        "loadNotifications",
        "viewNotification"
    )
    
    foreach ($comp in $components) {
        if ($loginPartial -match $comp) {
            Write-Host "OK - $comp FOUND" -ForegroundColor Green
        } else {
            Write-Host "ERROR - $comp NOT FOUND" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "ERROR - Cannot check UI components" -ForegroundColor Red
}

# Test 6: Return Service Integration
Write-Host ""
Write-Host "TEST 6: Checking Return Service Integration..." -ForegroundColor Yellow

try {
    $returnController = Get-Content "d:\SaveGame\GAMINGSTORE\GAMINGSTORE\Controllers\ReturnController.cs" -Raw
    
    $checks = @(
        "private readonly INotificationService _notificationService",
        "SendReturnStatusNotificationAsync"
    )
    
    foreach ($check in $checks) {
        if ($returnController -match [regex]::Escape($check)) {
            Write-Host "OK - $check" -ForegroundColor Green
        } else {
            Write-Host "ERROR - $check NOT FOUND" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "ERROR - Cannot check Return controller" -ForegroundColor Red
}

# Test 7: Database Schema
Write-Host ""
Write-Host "TEST 7: Checking Database Schema..." -ForegroundColor Yellow

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    $tables = @("CustomerNotifications", "NotificationLogs", "NotificationTemplates")
    
    foreach ($table in $tables) {
        $cmd = $connection.CreateCommand()
        $cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='$table'"
        $count = $cmd.ExecuteScalar()
        
        if ($count -gt 0) {
            Write-Host "OK - Table '$table' exists ($count columns)" -ForegroundColor Green
        } else {
            Write-Host "ERROR - Table '$table' NOT found" -ForegroundColor Red
        }
    }
    
    $connection.Close()
} catch {
    Write-Host "ERROR - Cannot check schema" -ForegroundColor Red
}

# Test 8: Delivered Orders
Write-Host ""
Write-Host "TEST 8: Checking for Delivered Orders..." -ForegroundColor Yellow

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT TOP 3 Id, CustomerName, Status FROM Orders WHERE Status IN ('Delivered', 'Completed') ORDER BY CreatedDate DESC"
    $reader = $cmd.ExecuteReader()
    
    $count = 0
    while ($reader.Read()) {
        $id = $reader["Id"]
        $name = $reader["CustomerName"]
        $status = $reader["Status"]
        Write-Host "  - Order $id : $name ($status)"
        $count++
    }
    
    if ($count -eq 0) {
        Write-Host "WARNING - No delivered orders found" -ForegroundColor Yellow
    } else {
        Write-Host "OK - Found $count delivered orders" -ForegroundColor Green
    }
    
    $reader.Close()
    $connection.Close()
} catch {
    Write-Host "ERROR - Cannot check orders" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TEST COMPLETE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
