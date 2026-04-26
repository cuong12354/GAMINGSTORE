# 🔔 TEST NOTIFICATION SYSTEM - DUAL ACCOUNT PARALLEL TEST

## Setup: Mở 2 Tab/Trình Duyệt

### 📱 Cách 1: Dùng 2 Tab (Đơn giản nhất)
```
1. Mở Chrome/Edge/Firefox
2. Mở tab 1: http://localhost:5190 (Đăng nhập ADMIN)
3. Mở tab 2: http://localhost:5190 (Đăng nhập CUSTOMER)
4. Xếp 2 tab cạnh nhau (Ctrl+Win+Left / Ctrl+Win+Right)
```

### 🖥️ Cách 2: Dùng 2 Trình Duyệt Khác Nhau (Tốt hơn)
```
1. Chrome: http://localhost:5190 (ADMIN)
2. Firefox: http://localhost:5190 (CUSTOMER)
3. Xếp 2 cửa sổ cạnh nhau
```

---

## 📋 TEST SCENARIO - Step by Step

### **BƯỚC 1: ADMIN - Thay đổi Order Status**

**Trên Tab/Cửa sổ ADMIN:**

```
1. Đảm bảo đang login ADMIN
2. Click user dropdown → "📦 Quản lý đơn hàng"
3. Tìm Order #2 (Status: Confirmed)
4. Click "🔍 Chi tiết" (Details)
5. Scroll xuống → "🔧 Cập Nhật Trạng Thái"
6. Dropdown: Chọn "✔️ Đã giao hàng" (Delivered)
7. Click "💾 Cập nhật"
✅ Success message: "✅ Trạng thái đơn hàng đã cập nhật thành: Delivered"
```

### **BƯỚC 2: CUSTOMER - Tạo Return Request**

**Trên Tab/Cửa sổ CUSTOMER (Khác tab):**

```
1. Đảm bảo đang login CUSTOMER
2. Click user dropdown → "📦 Lịch sử mua hàng"
3. Tìm Order #2 (Status bây giờ: ✔️ Delivered - mới thay đổi)
4. Click "Xem chi tiết"
5. Bạn sẽ thấy "🔄 Trả hàng" button (GREEN)
   ← Nó xuất hiện vì admin vừa thay Order #2 thành Delivered!
6. Click "🔄 Trả hàng"
7. Điền form:
   - Reason: Chọn lý do (e.g., "Sản phẩm hỏng")
   - Notes: (optional)
8. Click "Gửi yêu cầu"
✅ Return tạo thành công (Status: ⏳ Đang xem xét)
```

### **BƯỚC 3: ADMIN - Phê Duyệt Return**

**Quay lại Tab/Cửa sổ ADMIN:**

```
1. Click user dropdown → "📋 Quản lý trả hàng" (Manage Returns)
2. Tìm return vừa được tạo (Status: ⏳ Pending)
3. Click "Xem chi tiết"
4. Scroll xuống → Admin section
5. Click "✅ Phê Duyệt" button (GREEN)
   - Optional: Thêm notes
6. Click "Phê Duyệt" confirmation
✅ Success: "✅ Trạng thái đã được cập nhật và thông báo đã được gửi"
```

### **⭐ BƯỚC 4: CUSTOMER - KIỂM TRA NOTIFICATION BELL (CRITICAL)**

**Quay lại Tab/Cửa sổ CUSTOMER:**

**THIS IS THE KEY TEST POINT!** 🔔

```
1. Nhìn vào TOP-RIGHT CORNER của navbar
2. Bạn PHẢI thấy:
   ✅ Bell icon: 🔔
   ✅ Red badge: "1"
   ✅ Location: Right side, trước user dropdown

⭐ NẾU KHÔNG THẤY:
   • Click refresh (F5) hoặc reload
   • Wait 30 seconds (auto-refresh)
   • Kiểm tra browser console (F12)
```

### **BƯỚC 5: CUSTOMER - Xem Notification Dropdown**

**Trên Tab/Cửa sổ CUSTOMER:**

```
1. Click 🔔 Bell icon
2. Dropdown mở ra hiển thị:

   ┌─────────────────────────┐
   │ 🔔 Thông báo           │
   │ Đánh dấu... ✓          │
   │                         │
   │ ✅ Yêu cầu trả...│
   │ Order #2 Refund: ...    │ ← MESSAGE
   │ 14:35                   │
   └─────────────────────────┘

✅ VERIFY:
   • Title: "✅ Yêu cầu trả hàng được phê duyệt"
   • Message: Contains Order #2 + refund amount
   • Status: Blue background (unread)
```

### **BƯỚC 6: CUSTOMER - Click Notification**

**Trên Tab/Cửa sổ CUSTOMER:**

```
1. Click on notification item
2. Navigate to Return Details page
3. URL should be: /Return/Details/{returnId}
4. Return status shows: ✅ Phê Duyệt (Approved)
```

### **BƯỚC 7: VERIFY Database Records**

**Chạy PowerShell:**

```powershell
$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = "Server=DESKTOP-7HEJ3VI\SQLEXPRESS;Database=GAMINGSTORE;Trusted_Connection=True;TrustServerCertificate=True"
$conn.Open()

Write-Host "✅ CustomerNotifications:"
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT TOP 1 Title, Message, IsRead FROM CustomerNotifications ORDER BY CreatedDate DESC"
$reader = $cmd.ExecuteReader()
if ($reader.Read()) {
    Write-Host "  Title: $($reader['Title'])"
    Write-Host "  Message: $($reader['Message'])"
    Write-Host "  IsRead: $($reader['IsRead'])"
}
$reader.Close()

Write-Host ""
Write-Host "✅ NotificationLogs:"
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT TOP 1 NotificationType, Channel, Status FROM NotificationLogs ORDER BY SentDate DESC"
$reader = $cmd.ExecuteReader()
if ($reader.Read()) {
    Write-Host "  Type: $($reader['NotificationType'])"
    Write-Host "  Channel: $($reader['Channel'])"
    Write-Host "  Status: $($reader['Status'])"
}
$reader.Close()

$conn.Close()
```

---

## ✅ SUCCESS CHECKLIST

### Admin Side
- [ ] Order #2 status changed to Delivered
- [ ] Found pending return in dashboard
- [ ] Clicked approve button
- [ ] Saw success message

### Customer Side
- [ ] Order #2 shows as Delivered (real-time update)
- [ ] Can create return on Order #2
- [ ] Return created successfully (Pending status)
- [ ] **Bell 🔔 appears with "1" badge** ← MOST IMPORTANT
- [ ] Dropdown shows notification
- [ ] Title: "✅ Yêu cầu trả hàng được phê duyệt"
- [ ] Message contains order details
- [ ] Can click notification to view details
- [ ] Return shows as Approved

### Database
- [ ] CustomerNotifications: 1 record created
- [ ] NotificationLogs: 1 record created
- [ ] Both have correct data

---

## 🎯 Key Synchronization Points

| Point | Admin | Customer |
|-------|-------|----------|
| 1 | Change Order Status | See updated status |
| 2 | (waiting) | Create return request |
| 3 | Approve return | (waiting) |
| 4 | (waiting) | See notification bell |
| 5 | - | View notification details |

---

## 🐛 Troubleshooting

| Issue | Solution |
|-------|----------|
| Customer doesn't see "Trả hàng" button | Make sure Order status is Delivered (reload page) |
| Admin can't find pending return | Make sure return was created by customer (check database) |
| Bell icon doesn't appear | Reload customer page after admin approves |
| Notification doesn't appear in dropdown | Wait 30 seconds or click refresh |
| Can't navigate from notification | Check browser console for JS errors |

---

## 💡 Pro Tips

1. **Keep both windows visible** - You can see changes in real-time
2. **Use F12 Developer Tools** - Check Network tab for API calls:
   - `/api/notification/recent`
   - `/api/notification/unread-count`
3. **Check Application State** - In DevTools:
   - Local Storage
   - Session Storage
   - Check for any stored notification data

---

## 🚀 Timeline Estimate

```
⏱️ Step 1 (Admin changes status):     1 minute
⏱️ Step 2 (Customer creates return):  2 minutes
⏱️ Step 3 (Admin approves):           1 minute
⏱️ Step 4 (Customer verifies):        2 minutes
⏱️ Step 5 (Database check):           1 minute
──────────────────────────────────────
Total: ~7 minutes
```

---

## 📸 Expected Screenshots

### Admin - Change Order Status
```
Order Management → Order #2 Details
Status dropdown: [Delivered ▼]
💾 Cập nhật button
→ Success message appears
```

### Customer - See Order Updated
```
Order History
Order #2: nvu2446@gmail.com - ✔️ Delivered
🔄 Trả hàng button now appears!
```

### Customer - Return Created
```
Return Index page
⏳ Đang xem xét (Pending status)
Return created successfully
```

### Admin - Manage Returns
```
📋 Quản lý trả hàng
Pending return from customer
✅ Phê Duyệt button
→ Approved
```

### Customer - Notification Bell 🔔
```
TOP-RIGHT NAVBAR:
🔔₁ 👤▼ ← Bell with "1" badge!

Click bell:
┌──────────────────┐
│ 🔔 Thông báo    │
│ Đánh dấu...      │
│                  │
│ ✅ Yêu cầu...    │
│ Order #2...      │
│ 14:35            │
└──────────────────┘
```

---

## 🎯 READY?

**Now you can:**
1. Open 2 tabs/windows
2. Follow steps 1-7 above
3. Verify notification system works in real-time!

**Let me know when you've tested and what you see!** ✅
