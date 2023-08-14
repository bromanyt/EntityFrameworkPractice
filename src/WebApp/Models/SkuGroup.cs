using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebRetail.Models;

public partial class SkuGroup : IRetailTable
{
    [Key]
    [DisplayName("Group ID")]
    public long GroupId { get; set; }

    [DisplayName("Group name")]
    public string? GroupName { get; set; }

    public virtual ICollection<ProductGrid> ProductGrids { get; set; } = new List<ProductGrid>();

    public List<string> GetRows() => new() { $"{GroupId}", $"{GroupName}" };
}
