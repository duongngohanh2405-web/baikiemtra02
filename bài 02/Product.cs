namespace InventoryManager
{
    public class Product
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        // Chỉ lưu đường dẫn ảnh, KHÔNG nạp ảnh vào dòng của DataGridView
        public string ImagePath { get; set; } = "";

        public decimal TotalValue => Price * Quantity;
    }
}
