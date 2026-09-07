using System;
using System.Collections.Generic;

namespace PlantShopAPI.Models;

public partial class Plant
{
    public int PlantId { get; set; }

    public string PlantName { get; set; } = null!;

    public decimal Price { get; set; }

    public int? CategoryId { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
