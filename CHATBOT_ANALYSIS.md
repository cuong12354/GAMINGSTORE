# Phân Tích Code Chatbot Tư Vấn Bán Hàng

## I. KIẾN TRÚC TỔNG THỂ
- **Frontend UI**: HTML + CSS (Views/Shared/_Layout.cshtml) + JavaScript (wwwroot/js/site.js)
- **Backend Logic**: SalesChatController.cs (rule-based + keyword matching)
- **Dữ liệu**: Lấy từ repository (IProductRepository, ICategoryRepository)
- **Giao tiếp**: Frontend qua fetch() API → Backend trả JSON

---

## II. PHẦN HTML/CSS (Views/Shared/_Layout.cshtml)

### Container Chính
```html
<div class="sales-chatbot" id="salesChatbot" data-sales-chat-url="@Url.Action("Ask", "SalesChat")">
```
- **id="salesChatbot"**: selector chính cho JS
- **data-sales-chat-url**: chứa URL endpoint API (`/SalesChat/ask`)

### Panel Chat (Ẩn mặc định, hiện khi click nút mở)
```html
<div class="sales-chatbot-panel">
    <div class="sales-chatbot-header">Trợ lý bán hàng</div>
    <div class="sales-chatbot-body" id="salesChatMessages"></div>
    <div class="sales-chat-input-wrap">
        <form class="sales-chat-form" id="salesChatForm">
            <input id="salesChatInput" class="sales-chat-input" />
            <button class="sales-chat-send" type="submit">Send</button>
        </form>
    </div>
</div>
```

### Nút Toggle (Nút tròn cố định dưới góc phải)
```html
<button class="sales-chat-toggle" id="salesChatToggle">💬</button>
```

### CSS Chính
- **.sales-chat-message.bot**: thẻ tin nhắn bot (xanh trái)
- **.sales-chat-message.user**: thẻ tin nhắn user (đỏ phải)
- **.sales-chat-product**: card sản phẩm gợi ý
- **.sales-chat-suggestion**: nút gợi ý nhanh
- **.sales-chat-typing**: hiệu ứng "đang suy nghĩ"

---

## III. PHẦN JAVASCRIPT (wwwroot/js/site.js)

### Khởi Tạo
```javascript
document.addEventListener("DOMContentLoaded", () => {
    const chatbot = document.getElementById("salesChatbot");
    const panel = document.getElementById("salesChatMessages");
    const form = document.getElementById("salesChatForm");
    const input = document.getElementById("salesChatInput");
    const toggle = document.getElementById("salesChatToggle");
    const chatUrl = chatbot.dataset.salesChatUrl; // "/SalesChat/ask"
```

### Các Hàm Chính
1. **addMessage(content, sender)**: Tạo bubble tin nhắn + scroll xuống
2. **addTyping()**: Hiệu ứng 3 chấm animation "đang suy nghĩ"
3. **addSuggestions(suggestions)**: Render nút gợi ý nhanh
4. **addProducts(products)**: Render card sản phẩm từ API
5. **bootChat()**: Lần đầu mở, gọi 1 lần để hiển thị lời chào + gợi ý mẫu

### Luồng Form Submit
```javascript
form.addEventListener("submit", async (event) => {
    const message = input.value.trim();
    addMessage(message, "user");          // Hiện tin nhắn user
    const typingBubble = addTyping();     // Hiện 3 chấm
    
    const response = await fetch(chatUrl, {
        method: "POST",
        body: JSON.stringify({ message })
    });
    
    const payload = await response.json();
    typingBubble.remove();
    
    addMessage(payload.message, "bot");         // Tin nhắn bot
    addProducts(payload.products || []);        // Gợi ý sản phẩm
    addSuggestions(payload.suggestions || []); // Nút gợi ý tiếp
});
```

---

## IV. PHẦN BACKEND (Controllers/SalesChatController.cs)

### Endpoint
```csharp
[HttpPost("ask")]
public async Task<IActionResult> Ask([FromBody] SalesChatRequest request)
```
- Route: `/SalesChat/ask` (POST)
- Input: JSON `{ "message": "laptop gaming dưới 20 triệu" }`
- Output: JSON `SalesChatResponse` với message, products, suggestions

### Bước 1: Chuẩn Hóa Input
```csharp
var normalizedMessage = Normalize(message);
// Bỏ dấu, lowercase: "Laptop Gaming" → "laptop gaming"
```

### Bước 2: Lấy Toàn Bộ Dữ Liệu
```csharp
var products = (await _productRepository.GetAllAsync()).ToList();
var categories = (await _categoryRepository.GetAllAsync()).ToList();
// ← Đây là dòng khiến bot "đọc được" tất cả sản phẩm
```

### Bước 3: Xử Lý Intent Cố Định (Rule-Based)
```csharp
if (ContainsAny(normalizedMessage, "xin chao", "chao", "hello", "hi"))
    return Json(greeting_response);

if (ContainsAny(normalizedMessage, "danh muc", "ban gi", "shop co gi"))
    return Json(list_categories_response);

if (ContainsAny(normalizedMessage, "ship", "giao hang", "van chuyen"))
    return Json(shipping_info_response);

if (ContainsAny(normalizedMessage, "thanh toan", "payment", "tra truoc"))
    return Json(payment_info_response);
```

### Bước 4: Tìm Ngân Sách (Regex)
```csharp
var budget = ExtractBudget(normalizedMessage);
// "dưới 20 triệu" → 20000000
// "laptop 15tr" → 15000000
```

### Bước 5: Chấm Điểm & Lọc Sản Phẩm
```csharp
var matchedProducts = ScoreProducts(products, normalizedMessage, budget);

// Hàm ScoreProducts duyệt từng sản phẩm:
foreach (var product in products) {
    var score = CalculateScore(product, normalizedMessage, keywords, budget);
}

// Hàm CalculateScore đọc:
var productName = Normalize(product.Name);        // Tên sản phẩm
var description = Normalize(product.Description); // Mô tả
var categoryNames = product.Categories?.Select(c => Normalize(c.Name));

// Match keyword với tên (+5 điểm), mô tả (+2), danh mục (+4)
// Nếu có ngân sách và giá ≤ ngân sách → +3
// Nếu có từ "rẻ" và giá ≤ 20 triệu → +2
```

### Bước 6: Fallback & Lưu Ý
```csharp
// Nếu không tìm thấy sản phẩm nào:
if (!matchedProducts.Any())
    return Json(CreateFallbackResponse("Mình chưa bắt đúng ý lắm..."));

// Nếu hỏi "sản phẩm bán chạy":
if (ContainsAny(normalizedMessage, "ban chay", "goi y", "tot nhat"))
    matchedProducts = products.OrderByDescending(p => p.Price).Take(3);
```

### Bước 7: Trả JSON Cho Frontend
```csharp
var productCards = matchedProducts.Take(3).Select(MapProduct).ToList();

return Json(new SalesChatResponse {
    Message = "Mình tìm được vài sản phẩm phù hợp...",
    Products = productCards,  // Array 3 sản phẩm
    Suggestions = BuildFollowUpSuggestions(...)  // Gợi ý tiếp
});
```

---

## V. HELPER FUNCTIONS

### Normalize(text)
- Bỏ dấu tiếng Việt: "Laptop" → "laptop", "đ" → "d"
- Lowercase tất cả

### ExtractBudget(text)
- Regex: `(\d+[\.,]?\d*)\s*(tr|trieu|m|k|...)`
- "20 triệu" → 20000000
- "15tr" → 15000000

### CalculateScore(product, message, keywords, budget)
- Loop từng keyword
- Nếu keyword ⊂ tên → +5
- Nếu keyword ⊂ mô tả → +2
- Nếu keyword ⊂ danh mục → +4
- Nếu có ngân sách & giá ≤ ngân sách → +3
- Trả về điểm cao nhất ở trên cùng

### MapProduct(product)
- Lấy ảnh đầu tiên từ `ImageUrl` (split bằng `;`)
- Return: `{ id, name, price, imageUrl, categories, url }`

---

## VI. STOP WORDS (Từ Bỏ Qua)
```csharp
"toi", "muon", "tim", "san", "pham", "shop", "cua", "hang", "co", "khong",
"gi", "nao", "de", "giup", "tu", "van", "cho", "minh", "voi", "la", "mot",
"nhung", "cac", "gaming", "store", "giong", "nhe", "a", "ah", "em", "anh"
```
→ Từ này bị loại khi tách keyword từ tin nhắn

---

## VII. DATA MODELS

### SalesChatRequest (input)
```csharp
public string Message { get; set; }
```

### SalesChatResponse (output)
```csharp
public string Message { get; set; }
public List<string> Suggestions { get; set; }
public List<SalesChatProductCard> Products { get; set; }
```

### SalesChatProductCard
```csharp
public int Id { get; set; }
public string Name { get; set; }
public decimal Price { get; set; }
public string ImageUrl { get; set; }
public string Url { get; set; }
public List<string> Categories { get; set; }
```

---

## VIII. LUỒNG HOÀN CHỈNH

```
User click nút chat
  ↓
bootChat() ghi lời chào + 4 gợi ý mẫu
  ↓
User nhập "laptop gaming dưới 20 triệu" & Enter
  ↓
form.addEventListener("submit")
  - addMessage(user_text, "user")
  - addTyping() (3 chấm animation)
  ↓
fetch("/SalesChat/ask", { message: "laptop gaming dưới 20 triệu" })
  ↓
SalesChatController.Ask()
  - Normalize text
  - GetAllAsync() lấy ~200 sản phẩm
  - ExtractBudget() → 20000000
  - ScoreProducts() → duyệt, chấm điểm
  - MapProduct() → chọn top 3
  ↓
return Json({ message, products[], suggestions[] })
  ↓
Frontend nhận JSON
  - typingBubble.remove()
  - addMessage(payload.message, "bot")
  - addProducts(payload.products) → render 3 card
  - addSuggestions(payload.suggestions) → render nút gợi ý
```

---

## IX. CÁC ĐIỂM YẾU & ĐỂ ĐÁNH GIÁ HIỆN TẠI

### ✅ Điểm Mạnh
- Phản hồi ngay tức khắc (không phụ thuộc LLM)
- Tư vấn đúng target (sản phẩm thực tế)
- Xử lý ngân sách tốt (regex)
- Xử lý intent cố định (chào, danh mục, vận chuyển, thanh toán)

### ❌ Điểm Yếu
- Không hiểu ngôn ngữ tự nhiên → chỉ keyword matching
- Nếu user hỏi ngoài 4 intent cố định → phải match keyword trong tên/mô tả/danh mục
- Không có context memory (mỗi lần chat là độc lập)
- Nếu user hỏi "đó là cái gì" (để hỏi về sản phẩm vừa gợi ý) → bot không hiểu

### 🎯 Cách Nâng Cấp
1. Thêm nhiều intent cố định hơn (viết review, bảo hành, so sánh sản phẩm)
2. Integrare LLM cho semantic search thay keyword matching
3. Lưu lịch chat & context để bot nhớ cuộc trò chuyện
4. Thêm hỏi xác nhận khi bot không chắc
5. Phân tích logs để thêm intent mới dựa trên câu hỏi thực

---

## X. FILE LƯU TRỮ

- **Views**: `Views/Shared/_Layout.cshtml` (lines 271-520 ~ CSS/HTML)
- **Frontend JS**: `wwwroot/js/site.js` (lines 1-152)
- **Backend**: `Controllers/SalesChatController.cs` (lines 1+)
- **Không có**: Separate database table cho chat history (sử dụng session/client-side)
