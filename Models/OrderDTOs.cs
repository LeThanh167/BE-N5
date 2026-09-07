namespace PlantShopAPI.Models
{
    public class CreateOrderItemDto
    {
        public int PlantId { get; set; }
        public int Quantity { get; set; }
    }

    public class CreateOrderDto
    {
        public string ShippingAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public List<CreateOrderItemDto> Items { get; set; } = new();
        public decimal Discount { get; set; } = 0;
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}