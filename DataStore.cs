using PlantShopAPI.Models;

namespace PlantShopAPI
{
    public static class DataStore
    {
        public static List<Plant> Plants { get; set; } = new List<Plant>
        {
            new Plant { Id = 1, Name = "Cây Kim Tiền", Price = 150000, Quantity = 10, Description = "Cây may mắn", CategoryId = 1 },
            new Plant { Id = 2, Name = "Cây Trầu Bà", Price = 80000, Quantity = 20, Description = "Dễ trồng", CategoryId = 1 }
        };

        public static List<Order> Orders { get; set; } = new List<Order>();
    }
}