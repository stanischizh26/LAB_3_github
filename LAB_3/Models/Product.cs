using System;
using System.Collections.Generic;

namespace LAB_3.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public string? Characteristics { get; set; }

    public string? Unit { get; set; }

    public byte[]? Photo { get; set; }

    public virtual ICollection<ProductType> ProductTypes { get; set; } = new List<ProductType>();

    public virtual ICollection<ProductionPlan> ProductionPlans { get; set; } = new List<ProductionPlan>();

    public virtual ICollection<SalesPlan> SalesPlans { get; set; } = new List<SalesPlan>();
}
