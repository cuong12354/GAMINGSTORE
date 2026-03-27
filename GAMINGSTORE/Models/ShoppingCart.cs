using GAMINGSTORE.Models;

public class ShoppingCart
{
    public List<CartItem> Items { get; set; } = new List<CartItem>();

    // Thêm sản phẩm
    public void AddItem(CartItem item)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            Items.Add(item);
        }
    }

    // Xóa sản phẩm
    public void RemoveItem(int productId)
    {
        Items.RemoveAll(i => i.ProductId == productId);
    }

    // 👉 1. Tăng số lượng
    public void IncreaseQuantity(int productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            item.Quantity++;
        }
    }

    // 👉 2. Giảm số lượng
    public void DecreaseQuantity(int productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            item.Quantity--;
            if (item.Quantity <= 0)
            {
                RemoveItem(productId);
            }
        }
    }

    // 👉 3. Tính tổng tiền
    public decimal GetTotalPrice()
    {
        return Items.Sum(i => i.Price * i.Quantity);
    }

    // 👉 4. Tổng số lượng sản phẩm (hiển thị trên cart)
    public int GetTotalQuantity()
    {
        return Items.Sum(i => i.Quantity);
    }

    // 👉 5. Xóa toàn bộ giỏ hàng
    public void Clear()
    {
        Items.Clear();
    }
}