# 🎮 GAMINGSTORE - Hướng Dẫn Sử Dụng

App hiện đang chạy tại: **http://localhost:5190**

---

## 📝 Thông Tin Tài Khoản Admin

**Email:** `admin@gamingstore.com`  
**Password:** `Admin@123!` (mật khẩu mới đã cập nhật)

> **Chú ý:** Mật khẩu cần có tối thiểu 8 ký tự, chứa chữ hoa, chữ thường, số và ký tự đặc biệt.

---

## 🔐 Cách Đăng Nhập

1. Truy cập: **http://localhost:5190**
2. Nhấp **"Đăng nhập"** ở góc phải
3. Nhập email: `admin@gamingstore.com`
4. Nhập password: `Admin@123!`
5. Nhấp **"Đăng nhập"**

---

## 📊 VỀ CÁC TÍNH NĂNG MỚI

### 🔄 Lịch Sử Mua Hàng

#### **Cho Khách Hàng (Customer):**
- Sau khi đăng nhập, nhấp vào **avatar/tên người dùng** ở góc phải
- Chọn **"📦 Lịch Sử Mua Hàng"**
- Xem **tất cả đơn hàng của chính mình**
- Nhấp vào từng đơn để xem **chi tiết, tiến độ, sản phẩm**

#### **Cho Admin:**
- Sau khi đăng nhập admin, nhấp vào **avatar/tên người dùng**
- Chọn **"📊 Tất Cả Đơn Hàng"**
- Xem **danh sách tất cả đơn hàng từ TẤT CẢ khách hàng**
- Có thể:
  - 🔍 **Tìm kiếm** theo tên khách hàng
  - 🏷️ **Lọc** theo trạng thái (Pending, Confirmed, Shipped, Delivered, Cancelled)
  - 📈 **Sắp xếp** theo ngày hoặc giá
  - ✏️ **Cập nhật trạng thái** đơn hàng

---

## 🌐 Đăng Nhập Bên Thứ 3 (OAuth)

### **Hiện Tại:**
- Google Login & Facebook Login **đã được cài đặt** trong code
- Nhưng chưa được **cấu hình credentials**

### **Cách Setup Google Login:**

#### Bước 1: Tạo Google OAuth Credentials
1. Truy cập: https://console.cloud.google.com/
2. Tạo project mới (nếu chưa có)
3. Vào **"APIs & Services" → "Credentials"**
4. Nhấp **"+ Create Credentials" → "OAuth 2.0 Client ID"**
5. Chọn **"Web application"**
6. Thêm **Authorized Redirect URIs:**
   ```
   http://localhost:5190/signin-google
   http://localhost:5190/signin-oidc
   ```
7. Copy **Client ID** & **Client Secret**

#### Bước 2: Lưu Credentials vào User Secrets (Development)
```bash
cd d:\SaveGame\GAMINGSTORE\GAMINGSTORE

# Lệnh 1: Set Google Client ID
dotnet user-secrets set "Authentication:Google:ClientId" "YOUR_CLIENT_ID_HERE.apps.googleusercontent.com"

# Lệnh 2: Set Google Client Secret
dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_CLIENT_SECRET_HERE"
```

#### Bước 3: Restart App
```bash
# Dừng app (Ctrl+C)
# Chạy lại: dotnet run
```

#### Bước 4: Test
- Truy cập: http://localhost:5190/Identity/Account/Login
- Nhấp **"Sign in with Google"**
- Đăng nhập bằng tài khoản Google
- Tài khoản sẽ được **tự động tạo** với role **Customer**

---

### **Cách Setup Facebook Login:**

#### Bước 1: Tạo Facebook App
1. Truy cập: https://developers.facebook.com/
2. Tạo app mới → chọn "Consumer" type
3. Vào **"Settings → Basic"**
4. Copy **App ID** & **App Secret**
5. Vào **"Settings → Basic"** → thêm **App Domains:**
   ```
   localhost:5190
   localhost
   ```
6. Vào **"Apps"** → **"Settings"** → **"0Auth Redirect URIs":**
   ```
   http://localhost:5190/signin-facebook
   ```

#### Bước 2: Lưu Credentials
```bash
cd d:\SaveGame\GAMINGSTORE\GAMINGSTORE

# Lệnh 1: Set Facebook App ID
dotnet user-secrets set "Authentication:Facebook:AppId" "YOUR_APP_ID_HERE"

# Lệnh 2: Set Facebook App Secret
dotnet user-secrets set "Authentication:Facebook:AppSecret" "YOUR_APP_SECRET_HERE"
```

#### Bước 3: Restart App & Test
Tương tự như Google

---

## 🎨 THEME MÀNG ĐẠO

### **Tình Trạng Hiện Tại:**
- Theme hiện tại: **Màu CÓ (Cam đỏ)**
- Theme mong muốn: **Màu ĐỎ & ĐỀN (kiểu ASUS ROG)**

### **Sẽ Cập Nhật:**
✅ Nền **đen đặc** (không màu nhạt)  
✅ Màu **đỏ hoạt động** (mạnh mẽ)  
✅ Viền **sáng** + hiệu ứng **gaming**  
✅ Typography & spacing **kiểu ROG**  

> Sửa đổi này sẽ được cập nhật trong lần chỉnh sửa CSS tiếp theo.

---

## ✅ Danh Sách Tính Năng Hoàn Thành

| Tính Năng | Status | Ghi Chú |
|-----------|--------|---------|
| ✅ Duyệt & tìm kiếm sản phẩm | Hoàn thành | Vietnamese hỗ trợ đầy đủ |
| ✅ Giỏ hàng & Checkout | Hoàn thành | Session-based |
| ✅ Xác thực người dùng | Hoàn thành | ASP.NET Core Identity |
| ✅ Lịch sử mua hàng (Customer) | **NEW** ✨ | Xem riêng đơn hàng của mình |
| ✅ Quản lý đơn hàng (Admin) | **NEW** ✨ | Xem tất cả, update trạng thái |
| ✅ Admin-only quản lý sản phẩm | Hoàn thành | Chỉ Admin có thể Create/Edit/Delete |
| ✅ QR Code thanh toán ngân hàng | Hoàn thành | Tạo QR & payment slip in được |
| ✅ OAuth Google/Facebook | Cài đặt ✅ | Chờ credentials setup |
| ⏳ Theme đen & đỏ (ASUS ROG) | Pending | Sẽ cập nhật tiếp |

---

## 🧪 Cách Test Lịch Sử Đơn Hàng

### Test Cho Customer:
1. **Đăng ký tài khoản mới** (hoặc dùng admin)
2. **Duyệt sản phẩm** → Add to cart
3. **Checkout** → Tạo đơn hàng
4. **Nhấp avatar** → **"📦 Lịch Sử Mua Hàng"**
5. ✅ Thấy đơn hàng vừa tạo

### Test Cho Admin:
1. **Đăng nhập**: admin@gamingstore.com / Admin@123!
2. **Nhấp avatar** → **"📊 Tất Cả Đơn Hàng"**
3. ✅ Thấy **tất cả đơn hàng** từ tất cả khách hàng
4. Thử:
   - 🔍 **Tìm kiếm** khách hàng
   - 🏷️ **Lọc** theo trạng thái
   - ✏️ **Cập nhật trạng thái** (nhấp "Cập Nhật")

---

## ❓ Troubleshooting

### Q: "Tôi không thể đăng nhập được"
**A:** 
- ✅ Kiểm tra email: `admin@gamingstore.com` (chính xác)
- ✅ Kiểm tra password: `Admin@123!` (có dấu chấm than)
- ✅ Database có dữ liệu không? (Chạy app lần đầu sẽ seed admin)

### Q: "Tôi không thấy 'Lịch Sử Mua Hàng' trong menu"
**A:** 
- ✅ Bạn đã đăng nhập chưa? (Chỉ hiển thị khi logged in)
- ✅ Nhấp vào **avatar/tên người dùng** ở góc phải

### Q: "OAuth buttons không xuất hiện"
**A:** 
- ✅ Chưa setup credentials (xem phần **OAuth Setup** ở trên)
- ✅ Sau khi setup, restart app: `dotnet run`

### Q: "Theme vẫn còn màu cam"
**A:** 
- ✅ Theme sẽ được cập nhật trong bước sau
- ✅ Hiện tại đang là placeholder màu cam

---

## 📞 Hỗ Trợ

Nếu có vấn đề, kiểm tra:
1. Database connection string ở `appsettings.json`
2. Cổng 5190 có đang hoạt động?
3. Log của app (xem terminal)

---

**Cập nhật lần cuối:** April 11, 2026  
**Version:** 2.1 (Lịch sử đơn hàng + OAuth + Mật khẩu admin mới)
