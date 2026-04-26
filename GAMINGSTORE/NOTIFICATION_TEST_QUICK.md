# 🧪 NOTIFICATION SYSTEM TEST - QUICK REFERENCE

## 🎯 What You'll Test

```
Customer Creates Return
        ↓
Admin Approves Return
        ↓
Customer Sees Notification Bell 🔔
        ↓
Notification Dropdown Shows Message
        ↓
Database Records Created ✅
```

---

## ⚡ QUICK TEST STEPS (5-10 minutes)

### 1️⃣ LOGIN AS CUSTOMER
```
URL: http://localhost:5190
Click "Đăng nhập" → Enter customer credentials → Login
```

### 2️⃣ CREATE RETURN
```
User Dropdown → "📦 Lịch sử mua hàng"
↓
Find Order #1 (Status: Delivered)
↓
Click "Xem chi tiết" button
↓
Click "🔄 Trả hàng" button (GREEN button)
↓
Select reason + Click "Gửi yêu cầu"
✅ You should see: "Đang xem xét" (Pending)
```

### 3️⃣ LOGOUT & LOGIN AS ADMIN
```
Click user dropdown → "🚪 Đăng xuất"
↓
Click "Đăng nhập" → Enter ADMIN credentials
```

### 4️⃣ APPROVE RETURN
```
User Dropdown → "📋 Quản lý trả hàng"
↓
Find pending return → Click "Xem chi tiết"
↓
Scroll to admin section
↓
Click "✅ Phê Duyệt" button
↓
Click "OK" to confirm
✅ Success message appears
```

### 5️⃣ LOGOUT & LOGIN AS CUSTOMER
```
Click user dropdown → "🚪 Đăng xuất"
↓
Click "Đăng nhập" → Enter SAME customer account
```

### 6️⃣ ⭐ CHECK NOTIFICATION BELL ⭐
```
🔔 LOOK AT TOP-RIGHT CORNER OF NAVBAR
```

**You should see:**
- Bell icon: 🔔
- Red badge with "1" inside
- Location: Right side, left of user dropdown

### 7️⃣ CLICK BELL TO OPEN DROPDOWN
```
Click 🔔 icon
↓
Dropdown appears with:
  - Title: "✅ Yêu cầu trả hàng được phê duyệt"
  - Message: Order #1 details + refund amount
  - Status: Blue background (unread)
```

### 8️⃣ CLICK NOTIFICATION
```
Click on notification item
↓
Navigate to Return Details page
↓
Return status shows: "Phê Duyệt" ✅
```

---

## ✅ CRITICAL VERIFICATION POINTS

| Step | What to Verify | Location | Status |
|------|----------------|----------|--------|
| 1 | Bell icon visible | Top-right navbar | Should appear |
| 2 | Unread count badge | On bell icon | Should show "1" |
| 3 | Dropdown opens | Click bell | Should display |
| 4 | Notification title | In dropdown | Should have ✅ emoji |
| 5 | Message shows order details | In dropdown | Should include #1 |
| 6 | Click navigates | In dropdown | Should go to /Return/Details |
| 7 | Return shows approved | Details page | Should say "Phê Duyệt" |
| 8 | Database records | SQL query | Should have 1 record |

---

## 🎬 Visual Guide

### When Bell Appears (Top-Right Navbar)
```
┌────────────────────────────────────┐
│  NAVBAR                        🔔₁ 👤▼│
│  Logo  Home  About  Contact  [BELL] [USER]
│                                      
│  When bell clicked ↓
│  ┌─────────────────────────┐
│  │ 🔔 Thông báo           │
│  │ Đánh dấu... ✓          │
│  │                         │
│  │ ✅ Yêu cầu trả...│
│  │ Order #1 Refund: ...   │
│  │ 14:35                  │
│  └─────────────────────────┘
```

---

## 📊 Expected Database State

After successful test:

```sql
-- CustomerNotifications should have 1 record
SELECT * FROM CustomerNotifications
-- Title: "✅ Yêu cầu trả hàng được phê duyệt"
-- Channel: "InApp"
-- IsRead: 0 (or 1 if you marked as read)

-- NotificationLogs should have 1 record
SELECT * FROM NotificationLogs
-- Channel: "Email" 
-- Status: "Failed" (if SMTP not configured) or "Sent"
-- NotificationType: "ReturnApproved"
```

---

## ❓ QUICK TROUBLESHOOTING

| Problem | Solution |
|---------|----------|
| No bell icon | Refresh page after login |
| Bell shows 0 | Make sure admin APPROVED (not just viewed) |
| No dropdown when clicking bell | Check browser console for errors |
| Wrong notification title | Make sure you got APPROVED (not rejected) |
| Can't navigate from notification | Try manual URL: /Return/Details/1 |

---

## 🏁 TEST COMPLETE When:

- ✅ Bell icon visible with "1" badge
- ✅ Notification dropdown opens
- ✅ Title shows "✅ Yêu cầu trả hàng được phê duyệt"
- ✅ Message contains order #1 and refund amount
- ✅ Click navigates to Return Details
- ✅ Database records created

**All above = SUCCESS!** 🎉

---

## 📝 OPTIONAL: Verify With Database Query

```powershell
# Save as verify-notification.ps1
$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = "Server=DESKTOP-7HEJ3VI\SQLEXPRESS;Database=GAMINGSTORE;Trusted_Connection=True;TrustServerCertificate=True"
$conn.Open()

Write-Host "Checking CustomerNotifications..."
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT COUNT(*) as cnt FROM CustomerNotifications"
$count = $cmd.ExecuteScalar()
Write-Host "Records: $count (should be 1+)"

Write-Host ""
Write-Host "Latest Notification:"
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT TOP 1 Title, Message, IsRead FROM CustomerNotifications ORDER BY CreatedDate DESC"
$reader = $cmd.ExecuteReader()
if ($reader.Read()) {
    Write-Host "  Title: $($reader['Title'])"
    Write-Host "  Message: $($reader['Message'])"
    Write-Host "  IsRead: $($reader['IsRead'])"
}
$reader.Close()

$conn.Close()
```

**Run:** `powershell -ExecutionPolicy Bypass -Command "..."` or save to file and run

---

**Now go test! Visit http://localhost:5190** 🚀
