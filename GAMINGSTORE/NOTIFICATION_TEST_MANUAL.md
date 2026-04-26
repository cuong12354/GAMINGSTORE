# 🔔 NOTIFICATIONS SYSTEM - MANUAL TEST GUIDE

## Application Status
- **URL**: http://localhost:5190
- **Status**: Running ✅
- **Database**: Ready ✅
- **Test Data**: Order #1 (Delivered status) ✅

---

## 📋 TEST SCENARIO OVERVIEW

This test will verify:
1. ✅ Customer can create a return request
2. ✅ Admin can approve/reject returns
3. ✅ Notification bell appears with unread count
4. ✅ Notification dropdown shows message
5. ✅ Clicking notification navigates to Return Details
6. ✅ Database records created correctly

---

## 🚀 PHASE 1: CUSTOMER - CREATE RETURN REQUEST

### Step 1: Open Application
- Go to: http://localhost:5190
- You should see the GAMINGSTORE homepage

### Step 2: Login as Customer
- Click **Đăng nhập** (Login) button in top-right
- Use customer credentials (or register new account)
- After login, you should see navbar with user dropdown

### Step 3: Navigate to Order History
- Click on user dropdown in top-right
- Select **📦 Lịch sử mua hàng** (Order History)
- OR use menu link if available

### Step 4: Find Order #1 (Delivered)
- Look for order in "Delivered" status
- You should see:
  - Order ID: #1
  - Customer: nvu2446@gmail.com
  - Status: **Delivered** ✅
  - Return Status: None (no badge yet)

### Step 5: Click View Details
- Click **Xem chi tiết** button on Order #1
- Order details page should open

### Step 6: Create Return Request
- **IMPORTANT**: Admin should NOT see "Trả hàng" button
- As customer, you SHOULD see green **🔄 Trả hàng** button
- Click the "Trả hàng" button

### Step 7: Fill Return Form
- **Reason**: Select from dropdown (e.g., "Sản phẩm bị hỏng")
- **Notes**: Add optional notes (e.g., "Screen not working")
- **Button**: Click **Gửi yêu cầu** (Submit Request)

### Step 8: Confirm Return Created
- Page should redirect to Return Index page
- You should see your new return with status: **⏳ Đang xem xét** (Pending)
- Success message should appear at top

---

## 🔐 PHASE 2: ADMIN - APPROVE RETURN

### Step 1: Logout
- Click user dropdown
- Click **🚪 Đăng xuất** (Logout)

### Step 2: Login as ADMIN
- Click **Đăng nhập** (Login)
- Use ADMIN account credentials
- After login, admin menu should be visible

### Step 3: Navigate to Return Management
- Click user dropdown
- Look for **📋 Quản lý trả hàng** (Manage Returns)
- Click it

### Step 4: Find Pending Returns
- Page should show "Danh sách yêu cầu trả hàng đang chờ xử lý"
- Should see the return you just created as "Pending"
- Status badge: **⏳ Đang xem xét** (Pending)

### Step 5: View Return Details
- Click **Xem chi tiết** or the return row
- Return details page opens
- You should see:
  - Order ID
  - Return Reason
  - Return Amount
  - Status: Pending
  - **Admin section**: Approve/Reject buttons

### Step 6: Approve Return
- Look for **✅ Phê Duyệt** (Approve) button
- Add optional notes in textarea (e.g., "Return approved - refund will be processed")
- Click **✅ Phê Duyệt** button

### Step 7: Confirm Approval
- Confirmation dialog should appear
- Click **OK** to confirm
- Page should redirect to Pending list
- Success message: "✅ Trạng thái đã được cập nhật và thông báo đã được gửi"

---

## 🔔 PHASE 3: CUSTOMER - VERIFY NOTIFICATION

### Step 1: Logout from Admin
- Click user dropdown
- Click **🚪 Đăng xuất** (Logout)

### Step 2: Login as Customer (Same Account)
- Click **Đăng nhập** (Login)
- Use SAME customer account that created the return

### Step 3: Navigate to Any Page
- Click on **Home** or any navbar link
- **CRITICAL**: Look at TOP-RIGHT corner of navbar

### Step 4: Check Notification Bell 🔔
- **SHOULD SEE**: Bell icon with number badge showing "1"
- Location: Right side of navbar (to the left of user dropdown)
- Icon: 🔔 (bell with notification dot)
- Badge: Red circle with "1" inside

### Step 5: Click Bell Icon
- Click the bell icon to open dropdown
- **SHOULD SEE**: 
  - Header: "🔔 Thông báo"
  - Button: "Đánh dấu tất cả là đã đọc"
  - Notification item with:
    - **Title**: ✅ Yêu cầu trả hàng được phê duyệt
    - **Message**: Order details + refund amount
    - **Time**: Timestamp
    - **Style**: Blue background (unread)

### Step 6: Click Notification Item
- Click on the notification in the dropdown
- **SHOULD**: Navigate to Return Details page
- URL should be: `/Return/Details/{returnId}`
- Return status should now show: ✅ Phê Duyệt (Approved)

### Step 7: Back to Check Bell
- Navigate back to home or any page
- Click bell again
- **SHOULD SEE**: Notification still there
- Click **"Đánh dấu tất cả là đã đọc"** button
- Badge should disappear

---

## ✅ PHASE 4: DATABASE VERIFICATION

### Check Database Records

Run this PowerShell command to verify database records:

```powershell
$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = "Server=DESKTOP-7HEJ3VI\SQLEXPRESS;Database=GAMINGSTORE;Trusted_Connection=True;TrustServerCertificate=True"
$conn.Open()

# Check CustomerNotifications
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT Id, UserId, Title, Message, IsRead, CreatedDate FROM CustomerNotifications ORDER BY CreatedDate DESC"
$reader = $cmd.ExecuteReader()

Write-Host "CustomerNotifications Records:"
while ($reader.Read()) {
    $id = $reader["Id"]
    $userId = $reader["UserId"]
    $title = $reader["Title"]
    $isRead = $reader["IsRead"]
    $date = $reader["CreatedDate"]
    Write-Host "  ID: $id | User: $userId | Title: $title | Read: $isRead | Date: $date"
}
$reader.Close()

# Check NotificationLogs
Write-Host ""
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT Id, UserId, NotificationType, Channel, Status, SentDate FROM NotificationLogs ORDER BY SentDate DESC"
$reader = $cmd.ExecuteReader()

Write-Host "NotificationLogs Records:"
while ($reader.Read()) {
    $id = $reader["Id"]
    $userId = $reader["UserId"]
    $type = $reader["NotificationType"]
    $channel = $reader["Channel"]
    $status = $reader["Status"]
    $date = $reader["SentDate"]
    Write-Host "  ID: $id | User: $userId | Type: $type | Channel: $channel | Status: $status | Date: $date"
}
$reader.Close()

$conn.Close()
```

**Expected Results:**
- ✅ 1 record in CustomerNotifications (in-app notification)
- ✅ 1 record in NotificationLogs (email attempt - may show "Failed" if SMTP not configured)
- ✅ Title: "✅ Yêu cầu trả hàng được phê duyệt"
- ✅ Message contains order details and refund amount

---

## 🧪 TEST REJECTION (Optional)

To test rejection workflow:

1. **Create another return** from same or different order
2. **Admin login** → Manage Returns
3. **Click reject button** (❌ Từ Chối) instead of approve
4. **Customer login** → Check notification
5. **SHOULD SEE**: Red notification with title "❌ Yêu cầu trả hàng bị từ chối"

---

## ✅ TEST CHECKLIST

### Phase 1: Create Return
- [ ] Logged in as customer
- [ ] Navigated to Order History
- [ ] Found Order #1 (Delivered status)
- [ ] Clicked "Trả hàng" button
- [ ] Filled and submitted return form
- [ ] Saw success message
- [ ] Return shows as "Pending"

### Phase 2: Admin Approve
- [ ] Logged in as ADMIN
- [ ] Navigated to "Quản lý trả hàng"
- [ ] Found pending return
- [ ] Clicked details
- [ ] Clicked "Phê Duyệt" button
- [ ] Confirmed approval
- [ ] Saw success message

### Phase 3: Verify Notification
- [ ] Logged back in as customer
- [ ] **Bell icon visible** in top-right navbar ✅ CRITICAL
- [ ] **Badge shows "1"** on bell ✅ CRITICAL
- [ ] Clicked bell
- [ ] **Dropdown opened** with notification ✅ CRITICAL
- [ ] **Title shows**: "✅ Yêu cầu trả hàng được phê duyệt" ✅ CRITICAL
- [ ] **Message contains**: Order details + refund amount ✅ CRITICAL
- [ ] Clicked notification
- [ ] **Navigated to Return Details** ✅ CRITICAL
- [ ] Status shows "Approved"

### Phase 4: Database Verification
- [ ] Ran SQL query
- [ ] Found CustomerNotification record
- [ ] Found NotificationLog record
- [ ] Titles and messages correct

---

## 🎯 SUCCESS CRITERIA

**All of the following must be TRUE for test to PASS:**

1. ✅ Return request created successfully
2. ✅ Admin can approve/reject returns
3. ✅ Bell icon appears with unread count badge
4. ✅ Bell shows exactly "1" unread notification
5. ✅ Dropdown displays notification with correct title
6. ✅ Notification title includes "✅" emoji (approved status)
7. ✅ Message contains order ID and refund amount
8. ✅ Clicking notification navigates to Return Details
9. ✅ Return status shows "Approved" on details page
10. ✅ Database records created (CustomerNotifications, NotificationLogs)
11. ✅ Mark as read functionality works
12. ✅ Badge disappears after marking as read

---

## 🐛 TROUBLESHOOTING

### Issue: Bell icon not visible
- **Check**: Logged in? Bell only shows for authenticated users
- **Check**: Refreshed page after login?
- **Check**: Browser dev tools console for errors

### Issue: No unread count badge
- **Check**: Did return get approved? (not just pending)
- **Check**: Logged in as SAME customer account?
- **Check**: Try refreshing page

### Issue: Notification doesn't appear in dropdown
- **Check**: Wait 30 seconds (auto-refresh interval)
- **Check**: Browser dev tools Network tab for `/api/notification/recent` calls
- **Check**: Check database for CustomerNotifications records

### Issue: Can't navigate to Return Details from notification
- **Check**: Check browser console for JavaScript errors
- **Check**: Try clicking notification again
- **Check**: Manually navigate to `/Return/Details/{returnId}`

---

## 📞 NEXT STEPS

**After successful test:**
1. ✅ Feature #3 verified as complete
2. ✅ Move to Feature #4: Audit Logs System
3. ✅ Track all admin actions (Create/Update/Delete)

---

**Ready to test? Visit http://localhost:5190 and follow the steps above!**
