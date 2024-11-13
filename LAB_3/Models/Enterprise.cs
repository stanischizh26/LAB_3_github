using System;
using System.Collections.Generic;

namespace LAB_3.Models;

public partial class Enterprise
{
    public int EnterpriseId { get; set; }

    public string Name { get; set; } = null!;

    public string DirectorName { get; set; } = null!;

    public string ActivityType { get; set; } = null!;

    public string OwnershipForm { get; set; } = null!;

    public virtual ICollection<ProductionPlan> ProductionPlans { get; set; } = new List<ProductionPlan>();

    public virtual ICollection<SalesPlan> SalesPlans { get; set; } = new List<SalesPlan>();
}
