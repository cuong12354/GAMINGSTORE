using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using Microsoft.EntityFrameworkCore;

namespace GAMINGSTORE.Seeding
{
    public class DataSeeding
    {
        public static async Task SeedCategoriesAndProducts(ApplicationDbContext context)
        {
            await context.Database.MigrateAsync();

            var definitions = BuildCategoryDefinitions();
            var categoryMap = await EnsureCategoriesAsync(context, definitions.Select(d => d.Name));

            var existingProducts = await context.Products
                .Include(p => p.Categories)
                .ToListAsync();

            var existingProductNames = new HashSet<string>(
                existingProducts
                    .Select(p => p.Name)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(name => name!),
                StringComparer.OrdinalIgnoreCase);

            var productsToAdd = new List<Product>();

            foreach (var definition in definitions)
            {
                var category = categoryMap[definition.Name];
                var currentCount = existingProducts.Count(p => p.Categories.Any(c => c.Id == category.Id));

                if (currentCount >= 1)
                {
                    continue;
                }

                foreach (var productName in definition.ProductNames)
                {
                    if (currentCount >= 1)
                    {
                        break;
                    }

                    if (existingProductNames.Contains(productName))
                    {
                        continue;
                    }

                    var productIndex = currentCount;
                    productsToAdd.Add(new Product
                    {
                        Name = productName,
                        Price = definition.BasePrice + (definition.PriceStep * productIndex),
                        Description = BuildDescription(definition.Name, productName, productIndex),
                        ImageUrl = BuildImageUrl(definition.Name, productName, productIndex + 1),
                        Categories = new List<Category> { category }
                    });

                    existingProductNames.Add(productName);
                    currentCount++;
                }
            }

            if (productsToAdd.Count > 0)
            {
                await context.Products.AddRangeAsync(productsToAdd);
                await context.SaveChangesAsync();
            }

            var catalogProducts = await context.Products
                .Include(p => p.Categories)
                .ToListAsync();

            var hasUpdatedImages = false;

            foreach (var definition in definitions)
            {
                var category = categoryMap[definition.Name];
                var categoryProducts = catalogProducts
                    .Where(p => p.Categories.Any(c => c.Id == category.Id))
                    .OrderBy(p => p.Name)
                    .ToList();

                for (var index = 0; index < categoryProducts.Count; index++)
                {
                    var product = categoryProducts[index];

                    if (!ShouldRefreshGeneratedImage(product.ImageUrl))
                    {
                        continue;
                    }

                    product.ImageUrl = BuildImageUrl(definition.Name, product.Name ?? definition.ImageLabel, index + 1);
                    hasUpdatedImages = true;
                }
            }

            if (hasUpdatedImages)
            {
                await context.SaveChangesAsync();
            }
        }

        private static async Task<Dictionary<string, Category>> EnsureCategoriesAsync(ApplicationDbContext context, IEnumerable<string> categoryNames)
        {
            var categories = await context.Categories.ToListAsync();
            var categoryMap = categories
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .ToDictionary(c => c.Name!, c => c, StringComparer.OrdinalIgnoreCase);

            foreach (var categoryName in categoryNames)
            {
                if (categoryMap.ContainsKey(categoryName))
                {
                    continue;
                }

                var category = new Category { Name = categoryName };
                context.Categories.Add(category);
                categoryMap[categoryName] = category;
            }

            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }

            return categoryMap;
        }

        private static string BuildDescription(string categoryName, string productName, int index)
        {
            var featurePhrases = new[]
            {
                "thiết kế hoàn thiện đẹp, dễ phối với nhiều góc máy hiện đại",
                "hiệu năng ổn định cho nhu cầu sử dụng hằng ngày và tác vụ chuyên sâu",
                "khả năng vận hành mượt, phù hợp cho người dùng muốn trải nghiệm lâu dài",
                "độ hoàn thiện tốt, tối ưu cho cả công việc lẫn giải trí",
                "cấu hình cân bằng giữa tốc độ, độ bền và trải nghiệm thực tế",
                "trải nghiệm sử dụng thoải mái với khả năng đáp ứng nhanh và ổn định",
                "định hướng rõ cho người dùng cần hiệu năng, độ ổn định và tính thẩm mỹ",
                "khả năng đáp ứng tốt cho nhu cầu nâng cấp góc setup hoặc hệ thống hiện tại",
                "mức hoàn thiện phù hợp cho khách hàng cần sản phẩm đáng tiền trong tầm giá",
                "trải nghiệm thực tế tốt cho cả người dùng phổ thông lẫn người dùng yêu cầu cao"
            };

            var usagePhrases = new[]
            {
                "Phù hợp để sử dụng lâu dài, dễ kết hợp với các thiết bị khác trong hệ sinh thái gaming.",
                "Đây là lựa chọn đáng cân nhắc nếu bạn muốn mua nhanh một cấu hình hoặc phụ kiện đang dễ dùng trên web.",
                "Sản phẩm hướng đến người dùng muốn hiệu năng thực tế tốt và giao diện mua sắm đơn giản, rõ ràng.",
                "Khách hàng mới hoặc người dùng nâng cấp từ cấu hình cũ đều có thể tiếp cận sản phẩm này dễ dàng.",
                "Phù hợp cho nhu cầu học tập, làm việc, giải trí hoặc nâng cấp góc máy tại nhà.",
                "Nếu bạn đang cần một lựa chọn cân bằng giữa ngân sách và trải nghiệm, đây là mẫu rất dễ tiếp cận.",
                "Sản phẩm mang lại cảm giác sử dụng ổn định và phù hợp để đưa vào danh sách cân nhắc khi mua sắm.",
                "Đây là mẫu phù hợp cho người dùng muốn chốt đơn nhanh với thông số và định hướng sử dụng rõ ràng.",
                "Có thể dùng tốt trong nhiều kịch bản thực tế từ giải trí, học tập đến nhu cầu nâng cấp hệ thống.",
                "Mẫu này đặc biệt phù hợp cho khách hàng muốn một lựa chọn rõ ràng, dễ hiểu và có tính ứng dụng cao."
            };

            var categoryContext = categoryName switch
            {
                "Laptop Gaming" => "cấu hình thiên về chơi game, màn hình tần số quét cao và thiết kế hầm hố",
                "Laptop Văn phòng" => "kiểu dáng gọn gàng, hiệu năng ổn cho công việc văn phòng và tính di động cao",
                "MacBook" => "thiết kế cao cấp, thời lượng pin tốt và độ đồng bộ mạnh trong hệ sinh thái Apple",
                "PC Gaming" => "hiệu năng chơi game tốt, khả năng nâng cấp linh hoạt và luồng gió tối ưu",
                "PC Đồ họa" => "định hướng cho dựng hình, render, chỉnh sửa ảnh video và tác vụ sáng tạo nội dung",
                "PC Đồng bộ" => "khả năng vận hành ổn định, dễ triển khai cho văn phòng hoặc gia đình",
                "Màn hình Gaming" => "tốc độ phản hồi tốt, hình ảnh mượt và cảm giác chơi game rõ nét hơn",
                "Màn hình Văn phòng" => "không gian hiển thị dễ chịu, phù hợp làm việc lâu và xử lý đa nhiệm",
                "Màn hình Đồ họa" => "khả năng hiển thị màu sắc tốt, phù hợp cho chỉnh sửa hình ảnh và thiết kế",
                "Bàn phím cơ" => "cảm giác gõ rõ ràng, độ nảy tốt và phù hợp cho cả làm việc lẫn chơi game",
                "Chuột Gaming" => "độ nhạy cao, form cầm tối ưu và thao tác nhanh trong quá trình sử dụng",
                "Tai nghe Gaming" => "âm thanh định hướng rõ, đeo thoải mái và phù hợp cho cả game lẫn giải trí",
                "Lót chuột" => "bề mặt rê mượt, ổn định và hỗ trợ chuột tốt trong nhiều kiểu sử dụng",
                "Ghế Gaming" => "khả năng ngồi lâu thoải mái, hỗ trợ cột sống và phù hợp với góc setup gaming",
                "Loa máy tính" => "âm lượng ổn, chất âm rõ ràng và dễ dùng cho nhu cầu giải trí tại bàn máy",
                "CPU" => "hiệu năng xử lý mạnh, phù hợp cho nâng cấp dàn máy chơi game hoặc làm việc",
                "VGA" => "khả năng xử lý đồ họa tốt, phù hợp cho gaming và công việc liên quan hình ảnh",
                "Mainboard" => "khả năng kết nối linh hoạt, nền tảng ổn định cho việc build máy mới",
                "RAM" => "tăng khả năng đa nhiệm, cải thiện độ mượt tổng thể của hệ thống",
                "Ổ cứng SSD" => "tốc độ truy xuất nhanh, hỗ trợ mở máy và tải ứng dụng mượt hơn",
                _ => "định hướng rõ ràng cho nhu cầu nâng cấp và sử dụng thực tế"
            };

            return $"{productName} thuộc nhóm {categoryName}, nổi bật với {categoryContext}. Sản phẩm có {featurePhrases[index % featurePhrases.Length]}. {usagePhrases[index % usagePhrases.Length]}";
        }

        private static bool ShouldRefreshGeneratedImage(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return true;
            }

            return imageUrl.Contains("placehold.co", StringComparison.OrdinalIgnoreCase)
                || imageUrl.Contains("/images/no-image", StringComparison.OrdinalIgnoreCase)
                || imageUrl.StartsWith("data:image/svg+xml", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildImageUrl(string categoryName, string productName, int imageIndex)
        {
            var visual = GetCategoryVisual(categoryName);
            var productTitle = LimitText(productName, 32);
            var categoryTitle = LimitText(categoryName, 24).ToUpperInvariant();
            var productCode = $"SKU-{imageIndex:00}";

            var svg = $"""
<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 900 700'>
  <defs>
    <linearGradient id='bg' x1='0%' y1='0%' x2='100%' y2='100%'>
      <stop offset='0%' stop-color='#{visual.StartColor}' />
      <stop offset='100%' stop-color='#{visual.EndColor}' />
    </linearGradient>
    <linearGradient id='card' x1='0%' y1='0%' x2='100%' y2='100%'>
      <stop offset='0%' stop-color='rgba(255,255,255,0.28)' />
      <stop offset='100%' stop-color='rgba(255,255,255,0.08)' />
    </linearGradient>
  </defs>
  <rect width='900' height='700' fill='url(#bg)' rx='36' />
  <circle cx='760' cy='130' r='170' fill='rgba(255,255,255,0.10)' />
  <circle cx='120' cy='620' r='180' fill='rgba(255,255,255,0.08)' />
  <rect x='54' y='58' width='792' height='584' rx='36' fill='url(#card)' stroke='rgba(255,255,255,0.18)' />
  <text x='76' y='120' fill='#FFFFFF' font-family='Segoe UI, Arial, sans-serif' font-size='28' font-weight='700' letter-spacing='3'>{EscapeXml(categoryTitle)}</text>
  <text x='76' y='210' fill='#FFFFFF' font-family='Segoe UI, Arial, sans-serif' font-size='96' font-weight='800'>{EscapeXml(visual.IconText)}</text>
  <text x='76' y='330' fill='#FFFFFF' font-family='Segoe UI, Arial, sans-serif' font-size='44' font-weight='700'>{EscapeXml(productTitle)}</text>
  <text x='76' y='385' fill='rgba(255,255,255,0.92)' font-family='Segoe UI, Arial, sans-serif' font-size='24'>{EscapeXml(visual.Tagline)}</text>
  <rect x='76' y='456' width='204' height='58' rx='29' fill='rgba(255,255,255,0.18)' stroke='rgba(255,255,255,0.24)' />
  <text x='178' y='493' text-anchor='middle' fill='#FFFFFF' font-family='Consolas, monospace' font-size='24' font-weight='700'>{EscapeXml(productCode)}</text>
  <rect x='640' y='468' width='146' height='146' rx='28' fill='rgba(15,23,42,0.18)' stroke='rgba(255,255,255,0.24)' />
  <text x='713' y='556' text-anchor='middle' fill='#FFFFFF' font-family='Segoe UI, Arial, sans-serif' font-size='56' font-weight='800'>{EscapeXml(visual.BadgeText)}</text>
  <text x='76' y='604' fill='rgba(255,255,255,0.70)' font-family='Segoe UI, Arial, sans-serif' font-size='18'>Generated by GAMINGSTORE</text>
</svg>
""";

            return $"data:image/svg+xml;charset=utf-8,{Uri.EscapeDataString(svg)}";
        }

        private static string LimitText(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value[..(maxLength - 3)].TrimEnd() + "...";
        }

        private static string EscapeXml(string value)
        {
            return value
                .Replace("&", "&amp;", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal)
                .Replace(">", "&gt;", StringComparison.Ordinal)
                .Replace("\"", "&quot;", StringComparison.Ordinal)
                .Replace("'", "&apos;", StringComparison.Ordinal);
        }

        private static CategoryVisual GetCategoryVisual(string categoryName)
        {
            return categoryName switch
            {
                "Laptop Gaming" => new("111827", "2563eb", "LAPTOP", "FPS and high refresh focus", "LG"),
                "Laptop Văn phòng" => new("0f172a", "0891b2", "OFFICE", "Portable daily workflow", "VP"),
                "MacBook" => new("1f2937", "6d28d9", "MAC", "Clean premium ecosystem", "MB"),
                "PC Gaming" => new("111827", "dc2626", "PC", "High frame desktop build", "PG"),
                "PC Đồ họa" => new("1e293b", "7c3aed", "3D", "Render and creator ready", "DH"),
                "PC Đồng bộ" => new("334155", "0f766e", "SYNC", "Stable home and office setup", "DB"),
                "Màn hình Gaming" => new("0f172a", "ea580c", "HZ", "Fast panel for esports", "MG"),
                "Màn hình Văn phòng" => new("1e293b", "0284c7", "VIEW", "Comfort screen for work", "MV"),
                "Màn hình Đồ họa" => new("312e81", "db2777", "COLOR", "Wide gamut creator display", "MD"),
                "Bàn phím cơ" => new("3f3f46", "ca8a04", "KEY", "Tactile custom typing", "BP"),
                "Chuột Gaming" => new("111827", "16a34a", "AIM", "Precision control and speed", "CG"),
                "Tai nghe Gaming" => new("172554", "9333ea", "AUDIO", "Immersive directional sound", "TN"),
                "Lót chuột" => new("1f2937", "4f46e5", "PAD", "Smooth tracking surface", "LC"),
                "Ghế Gaming" => new("3f3f46", "be123c", "SEAT", "Comfort for long sessions", "GG"),
                "Loa máy tính" => new("0f172a", "0d9488", "SOUND", "Desk audio with clean output", "LOA"),
                "CPU" => new("111827", "f59e0b", "CPU", "Processing power upgrade", "CPU"),
                "VGA" => new("111827", "ef4444", "GPU", "Graphics power for games", "VGA"),
                "Mainboard" => new("1f2937", "14b8a6", "BOARD", "Foundation for your build", "MBD"),
                "RAM" => new("312e81", "06b6d4", "RAM", "Memory boost for multitasking", "RAM"),
                "Ổ cứng SSD" => new("0f172a", "22c55e", "SSD", "Fast storage and load times", "SSD"),
                _ => new("1f2937", "2563eb", "TECH", "Storefront generated artwork", "GS")
            };
        }

        private static List<CategorySeedDefinition> BuildCategoryDefinitions()
        {
            return new List<CategorySeedDefinition>
            {
                new("Laptop Gaming", 18990000m, 1300000m, "Laptop Gaming", new[] { "ASUS ROG Strix G16 RTX Edition", "Acer Predator Helios Neo 16", "MSI Katana 15 Performance", "Lenovo Legion 5 Storm", "HP Victus 16 Turbo", "Gigabyte G5 Battle Ready", "Dell G15 Nitro Frame", "ASUS TUF Gaming A15 Pro", "Lenovo LOQ 15 RapidPlay", "MSI Sword 16 RGB" }),
                new("Laptop Văn phòng", 12990000m, 900000m, "Laptop Van Phong", new[] { "Dell Inspiron 14 Smart Office", "HP Pavilion 14 AirView", "ASUS Zenbook 14 Slimbook", "Lenovo ThinkBook 14 Flow", "Acer Swift Go 14 WorkMate", "MSI Modern 15 Quiet Pro", "LG Gram 14 Mobility", "Huawei MateBook D16 Office", "Dell Latitude 5440 Business", "HP ProBook 450 Balance" }),
                new("MacBook", 24990000m, 1800000m, "MacBook", new[] { "MacBook Air 13 M2 Midnight", "MacBook Air 15 M2 Starlight", "MacBook Pro 13 M2 Creator", "MacBook Pro 14 M3 Pro", "MacBook Pro 16 M3 Pro Studio", "MacBook Air 13 M3 Silver", "MacBook Pro 14 M3 Max Graphite", "MacBook Air 15 M3 Sky", "MacBook Pro 16 M3 Max Titan", "MacBook Pro 14 M2 Pro Motion" }),
                new("PC Gaming", 15990000m, 1600000m, "PC Gaming", new[] { "PC Gaming Ryzen 5 RTX 4060", "PC Gaming Intel i5 Esports", "PC Gaming White Build RGB", "PC Gaming Blackout Performance", "PC Gaming Streaming Ready", "PC Gaming 2K Ultra Frame", "PC Gaming Compact Airflow", "PC Gaming Ryzen 7 PowerBox", "PC Gaming Intel i7 ProPlay", "PC Gaming RTX Master Tower" }),
                new("PC Đồ họa", 20990000m, 1700000m, "PC Do Hoa", new[] { "PC Đồ Họa Render Core", "PC Đồ Họa Studio Pro", "PC Đồ Họa Visual Creator", "PC Đồ Họa 3D Workstation", "PC Đồ Họa Motion Lab", "PC Đồ Họa Editor Suite", "PC Đồ Họa Adobe Master", "PC Đồ Họa Blender Node", "PC Đồ Họa Color Workflow", "PC Đồ Họa Production X" }),
                new("PC Đồng bộ", 8990000m, 850000m, "PC Dong Bo", new[] { "PC Đồng Bộ Office Mini", "PC Đồng Bộ Home Basic", "PC Đồng Bộ Study Station", "PC Đồng Bộ Admin Desk", "PC Đồng Bộ Core Office", "PC Đồng Bộ Work Standard", "PC Đồng Bộ Family Box", "PC Đồng Bộ Compact Plus", "PC Đồng Bộ Business Lite", "PC Đồng Bộ Daily Use" }),
                new("Màn hình Gaming", 4290000m, 700000m, "Man Hinh Gaming", new[] { "Màn Hình Gaming 24 Inch 180Hz", "Màn Hình Gaming 27 Inch 240Hz", "Màn Hình Gaming 32 Inch QHD", "Màn Hình Gaming Curved 165Hz", "Màn Hình Gaming Fast IPS 27", "Màn Hình Gaming HDR Combat", "Màn Hình Gaming Esports 25", "Màn Hình Gaming UltraWide 34", "Màn Hình Gaming RGB Base", "Màn Hình Gaming Speed View" }),
                new("Màn hình Văn phòng", 2490000m, 420000m, "Man Hinh Van Phong", new[] { "Màn Hình Văn Phòng 24 Inch", "Màn Hình Văn Phòng 27 Inch", "Màn Hình Văn Phòng Viền Mỏng", "Màn Hình Văn Phòng Eye Care", "Màn Hình Văn Phòng USB-C", "Màn Hình Văn Phòng Full HD", "Màn Hình Văn Phòng WorkView", "Màn Hình Văn Phòng MultiTask", "Màn Hình Văn Phòng Comfort Panel", "Màn Hình Văn Phòng Daily Desk" }),
                new("Màn hình Đồ họa", 5990000m, 880000m, "Man Hinh Do Hoa", new[] { "Màn Hình Đồ Họa 2K IPS", "Màn Hình Đồ Họa 4K Creator", "Màn Hình Đồ Họa Color Pro", "Màn Hình Đồ Họa Designer View", "Màn Hình Đồ Họa Studio 27", "Màn Hình Đồ Họa UltraColor", "Màn Hình Đồ Họa Print Match", "Màn Hình Đồ Họa HDR Artist", "Màn Hình Đồ Họa Precision Lab", "Màn Hình Đồ Họa Wide Gamut" }),
                new("Bàn phím cơ", 890000m, 290000m, "Ban Phim Co", new[] { "Bàn Phím Cơ TKL RGB Fire", "Bàn Phím Cơ Fullsize Ice", "Bàn Phím Cơ Hotswap Pro", "Bàn Phím Cơ Wireless Flex", "Bàn Phím Cơ Gasket Mount", "Bàn Phím Cơ Silent Office", "Bàn Phím Cơ Custom Base", "Bàn Phím Cơ 75 Layout", "Bàn Phím Cơ Aluminum Case", "Bàn Phím Cơ Stream Deck Style" }),
                new("Chuột Gaming", 490000m, 210000m, "Chuot Gaming", new[] { "Chuột Gaming Lightweight 58g", "Chuột Gaming Wireless Hero", "Chuột Gaming Esports Grip", "Chuột Gaming RGB Shooter", "Chuột Gaming Ultra Sensor", "Chuột Gaming Ergonomic Right", "Chuột Gaming Symmetric Aim", "Chuột Gaming Battery Pro", "Chuột Gaming Speed Click", "Chuột Gaming Tournament Ready" }),
                new("Tai nghe Gaming", 790000m, 330000m, "Tai Nghe Gaming", new[] { "Tai Nghe Gaming 7.1 Surround", "Tai Nghe Gaming Wireless Nova", "Tai Nghe Gaming RGB Mic Pro", "Tai Nghe Gaming Dual Chamber", "Tai Nghe Gaming Esports Voice", "Tai Nghe Gaming Comfort Foam", "Tai Nghe Gaming USB-C Arena", "Tai Nghe Gaming Bass Impact", "Tai Nghe Gaming Noise Shield", "Tai Nghe Gaming Stream Ready" }),
                new("Lót chuột", 199000m, 65000m, "Lot Chuot", new[] { "Lót Chuột Gaming Speed Pad", "Lót Chuột Gaming Control Pad", "Lót Chuột Gaming XL Deskmat", "Lót Chuột Gaming RGB Edge", "Lót Chuột Gaming Anti Slip", "Lót Chuột Gaming Soft Glide", "Lót Chuột Gaming Esports Base", "Lót Chuột Gaming Smooth Track", "Lót Chuột Gaming Carbon Style", "Lót Chuột Gaming Large Surface" }),
                new("Ghế Gaming", 3590000m, 860000m, "Ghe Gaming", new[] { "Ghế Gaming Ergo Support", "Ghế Gaming Recline Pro", "Ghế Gaming RGB Accent", "Ghế Gaming Carbon Black", "Ghế Gaming Titan Comfort", "Ghế Gaming Lumbar Fit", "Ghế Gaming Elite Seat", "Ghế Gaming Wide Frame", "Ghế Gaming Air Fabric", "Ghế Gaming Premium PU" }),
                new("Loa máy tính", 690000m, 240000m, "Loa May Tinh", new[] { "Loa Máy Tính Stereo Core", "Loa Máy Tính RGB Soundbar", "Loa Máy Tính Sub Bass 2.1", "Loa Máy Tính Bluetooth Desk", "Loa Máy Tính USB Compact", "Loa Máy Tính Cinema Box", "Loa Máy Tính Gaming Blast", "Loa Máy Tính Minimal Set", "Loa Máy Tính Clear Voice", "Loa Máy Tính Home Setup" }),
                new("CPU", 2890000m, 920000m, "CPU", new[] { "CPU Intel Core i5 Performance", "CPU Intel Core i7 Creator", "CPU Intel Core i9 Flagship", "CPU AMD Ryzen 5 Gaming", "CPU AMD Ryzen 7 Multitask", "CPU AMD Ryzen 9 Studio", "CPU Intel Core Ultra Desktop", "CPU AMD Ryzen X3D Boost", "CPU Intel Overclock Edition", "CPU Workstation Power Chip" }),
                new("VGA", 4690000m, 1550000m, "VGA", new[] { "VGA RTX Entry Frame", "VGA RTX 4060 Twin Fan", "VGA RTX 4070 OC Edition", "VGA RTX 4080 Creator", "VGA Radeon 7600 XT", "VGA Radeon 7700 XT", "VGA Radeon 7800 XT", "VGA White Edition RGB", "VGA Triple Fan Master", "VGA 4K Ultra Series" }),
                new("Mainboard", 2390000m, 420000m, "Mainboard", new[] { "Mainboard B760 Gaming WiFi", "Mainboard Z790 Performance", "Mainboard H610 Office Base", "Mainboard B650 Ryzen Ready", "Mainboard X670 Creator", "Mainboard mATX Compact Hub", "Mainboard ATX RGB Sync", "Mainboard DDR5 Future Build", "Mainboard PCIe 5 Upgrade", "Mainboard Workstation Core" }),
                new("RAM", 690000m, 180000m, "RAM", new[] { "RAM DDR4 16GB Gaming Kit", "RAM DDR4 32GB Office Set", "RAM DDR5 16GB Starter", "RAM DDR5 32GB RGB Pro", "RAM DDR5 64GB Creator Pack", "RAM Low Profile Stable", "RAM RGB Sync Edition", "RAM Performance XMP Kit", "RAM Laptop SO-DIMM Pack", "RAM Workstation Memory Set" }),
                new("Ổ cứng SSD", 790000m, 230000m, "SSD", new[] { "SSD NVMe 500GB Speed", "SSD NVMe 1TB Pro", "SSD NVMe 2TB Creator", "SSD SATA 1TB Daily", "SSD SATA 2TB Storage", "SSD PCIe 4 Gaming", "SSD PCIe 4 Workstation", "SSD Compact M.2 Boost", "SSD High Endurance Drive", "SSD System Upgrade Kit" })
            };
        }

        private sealed record CategorySeedDefinition(string Name, decimal BasePrice, decimal PriceStep, string ImageLabel, IReadOnlyList<string> ProductNames);

        private sealed record CategoryVisual(string StartColor, string EndColor, string IconText, string Tagline, string BadgeText);
    }
}
