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

            // Kiểm tra xem có dữ liệu không, nếu có thì bỏ qua
            if (await context.Categories.AnyAsync())
            {
                return; // Dữ liệu đã tồn tại
            }

            // Tạo danh mục
            var categories = new List<Category>
            {
                new Category { Name = "Laptop Gaming" },
                new Category { Name = "Laptop Văn phòng" },
                new Category { Name = "MacBook" },
                new Category { Name = "PC Gaming" },
                new Category { Name = "PC Đồ họa" },
                new Category { Name = "PC Đồng bộ" },
                new Category { Name = "Màn hình Gaming" },
                new Category { Name = "Màn hình Văn phòng" },
                new Category { Name = "Màn hình Đồ họa" },
                new Category { Name = "Bàn phím cơ" },
                new Category { Name = "Chuột Gaming" },
                new Category { Name = "Tai nghe Gaming" },
                new Category { Name = "Lót chuột" },
                new Category { Name = "Ghế Gaming" },
                new Category { Name = "Loa máy tính" },
                new Category { Name = "CPU" },
                new Category { Name = "VGA" },
                new Category { Name = "Mainboard" },
                new Category { Name = "RAM" },
                new Category { Name = "Ổ cứng SSD" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            // Tạo các sản phẩm mẫu
            var products = new List<Product>
            {
                // Laptop Gaming
                new Product
                {
                    Name = "ASUS TUF Gaming F15 (2024)",
                    Price = 24990000,
                    Description = "Laptop gaming cao cấp với CPU Intel i7-13700H, GPU RTX 4060, RAM 16GB, SSD 512GB. Màn hình 15.6 inch 144Hz, pin 90Wh, trọng lượng 2.2kg.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[0] }
                },
                new Product
                {
                    Name = "MSI Raider GE78 HX",
                    Price = 32000000,
                    Description = "Laptop gaming flagship với CPU i9-13900HX, RTX 4090, RAM 32GB DDR5, SSD 1TB NVMe. Màn hình 17.3 inch 144Hz, hệ thống làm mát cao cấp.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[0] }
                },
                new Product
                {
                    Name = "Lenovo LOQ 15",
                    Price = 18990000,
                    Description = "Laptop gaming mức giá phải chăng với CPU Ryzen 5 7435HS, RTX 4050, RAM 8GB, SSD 512GB. Màn hình 15.6 inch 144Hz, tốt cho gaming entry-level.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[0] }
                },

                // Laptop Văn phòng
                new Product
                {
                    Name = "Dell XPS 13 Plus",
                    Price = 28990000,
                    Description = "Laptop siêu mỏng với CPU Core Ultra 5, RAM 16GB LPDDR5, SSD 512GB. Màn hình OLED 13.4 inch, pin 72Wh, thiết kế hiện đại.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[1] }
                },
                new Product
                {
                    Name = "LG Gram 16",
                    Price = 22990000,
                    Description = "Laptop văn phòng siêu nhẹ 1.2kg, CPU Intel i7, RAM 16GB, SSD 512GB. Màn hình 16 inch WUXGA, pin 80Wh, bàn phím tốt.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[1] }
                },

                // MacBook
                new Product
                {
                    Name = "MacBook Pro 14\" M3 Pro",
                    Price = 34990000,
                    Description = "MacBook Pro 14 inch với chip M3 Pro, RAM 18GB, SSD 512GB. Màn hình Liquid Retina XDR, bàn phím Magic Keyboard, pin tới 18 giờ.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[2] }
                },

                // PC Gaming
                new Product
                {
                    Name = "PC Gaming AMD Ryzen 5 5600X",
                    Price = 16990000,
                    Description = "PC Gaming mức giá tốt với CPU Ryzen 5 5600X, GPU RTX 3060, RAM 16GB DDR4, SSD 512GB. Để chơi game ở mức 1080p high.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[3] }
                },
                new Product
                {
                    Name = "PC Gaming Intel i9-13900K RTX 4090",
                    Price = 52000000,
                    Description = "PC Gaming cao cấp nhất với i9-13900K, RTX 4090, RAM 64GB DDR5, SSD 2TB NVMe. Chơi được mọi game ở 4K ultra settings.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[3] }
                },

                // Màn hình Gaming
                new Product
                {
                    Name = "ASUS ROG Swift 27\" 240Hz",
                    Price = 11990000,
                    Description = "Màn hình gaming 27 inch 1440p 240Hz, panel IPS, response time 1ms, HDR400, RGB lighting. Lý tưởng cho competitive gaming.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[6] }
                },
                new Product
                {
                    Name = "LG UltraGear 32\" 144Hz",
                    Price = 13990000,
                    Description = "Màn hình gaming 32 inch 1440p 144Hz VA panel, HDR, curvature 1500R. Tuyệt vời cho immersive gaming experience.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[6] }
                },

                // Bàn phím cơ
                new Product
                {
                    Name = "Corsair K100 RGB Mechanical",
                    Price = 4990000,
                    Description = "Bàn phím cơ gaming cao cấp với switch Cherry MX Speed, RGB per-key, aluminum frame, kết nối USB Type-C.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[9] }
                },
                new Product
                {
                    Name = "Keychron K8 Pro",
                    Price = 2990000,
                    Description = "Bàn phím cơ wireless với switch Gateron, RGB backlight, pin 72 giờ, hỗ trợ 3 kết nối (USB, 2.4G, Bluetooth).",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[9] }
                },

                // Chuột Gaming
                new Product
                {
                    Name = "Razer DeathAdder V3",
                    Price = 2290000,
                    Description = "Chuột gaming siêu nhẹ 63g với sensor 30000 DPI, switch 90 triệu click, cable siêu mỏng. Ergonomic design cho tay phải.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[10] }
                },
                new Product
                {
                    Name = "Logitech G Pro X Superlight 2",
                    Price = 2590000,
                    Description = "Chuột gaming wireless 60g, sensor HERO 25600 DPI, pin 70 giờ, kết nối 2.4GHz. Được chuyên gia esports lựa chọn.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[10] }
                },

                // Tai nghe Gaming
                new Product
                {
                    Name = "SteelSeries Arctis Nova Pro",
                    Price = 5990000,
                    Description = "Tai nghe gaming cao cấp với driver 40mm, noise cancellation, chat mix dial, hỗ trợ loa kép. Thoải mái 30+ giờ.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[11] }
                },

                // Lót chuột
                new Product
                {
                    Name = "SteelSeries QcK Prism Cloth",
                    Price = 790000,
                    Description = "Lót chuột gaming 320x270mm với bề mặt vải mát, đế cao su chống trượt, RGB lighting. Tốt cho mọi loại chuột.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[12] }
                },

                // Ghế Gaming
                new Product
                {
                    Name = "ASUS ROG Chariot Gaming Chair",
                    Price = 9990000,
                    Description = "Ghế gaming cao cấp với lưng tựa cao, tựa đầu/cổ, độ rộng 80cm, có thể nằm ngả 180°. Chất liệu PU cao cấp.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[13] }
                },
                new Product
                {
                    Name = "Secretlab Titan Evo 2022 Series",
                    Price = 14990000,
                    Description = "Ghế gaming ergonomic từ Singapore với tựa lưng 13D, support tối ưu cho 4-6 giờ ngồi, công nghệ NapUp.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[13] }
                },

                // CPU
                new Product
                {
                    Name = "Intel Core i9-13900K",
                    Price = 10990000,
                    Description = "CPU top flagship 24 cores/32 threads, socket LGA1700, TDP 125W. Hiệu năng tuyệt vời cho gaming và workstation.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[15] }
                },
                new Product
                {
                    Name = "AMD Ryzen 9 7950X",
                    Price = 9990000,
                    Description = "CPU 16 cores/32 threads, socket AM5, TDP 105W. Sinh năng mạnh, tốt cho content creation và gaming.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[15] }
                },

                // GPU (VGA)
                new Product
                {
                    Name = "NVIDIA RTX 4090 Founders Edition",
                    Price = 16990000,
                    Description = "GPU flagship RTX 4090, 24GB GDDR6X, memory bandwidth 936 GB/s. Chơi mọi game 4K Ultra, tất cả ray tracing setting.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[16] }
                },
                new Product
                {
                    Name = "RTX 4070 Super",
                    Price = 9990000,
                    Description = "GPU RTX 4070 Super, 12GB GDDR6X, 384-bit memory bus. Tốt cho 1440p ultra 120+ fps, 4K high 60 fps.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[16] }
                },

                // RAM
                new Product
                {
                    Name = "Corsair Dominator Platinum RGB DDR5 6000MHz 32GB",
                    Price = 3990000,
                    Description = "RAM DDR5 32GB (2x16GB) 6000MHz CAS 30, RGB per-stick, lifetime warranty. Tối ưu cho gaming và workstation.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[18] }
                },

                // SSD
                new Product
                {
                    Name = "Samsung 990 Pro NVMe 1TB",
                    Price = 1990000,
                    Description = "SSD NVMe PCIe 4.0 1TB, tốc độ đọc 7100 MB/s, ghi 6000 MB/s. Bảo hành 5 năm, tốt cho gaming và workstation.",
                    ImageUrl = "/images/no-image.png",
                    Categories = new List<Category> { categories[19] }
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}
