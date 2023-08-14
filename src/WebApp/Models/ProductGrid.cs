using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebRetail.Models;

public partial class ProductGrid : IRetailTable
{
    [Key]
    [DisplayName("Sku ID")]
    public long SkuId { get; set; }

    [DisplayName("Sku name")]
    public string? SkuName { get; set; }

    [DisplayName("Group ID")]
    public long GroupId { get; set; }

    public virtual SkuGroup Group { get; set; } = null!;

    public List<string> GetRows() => new() { $"{SkuId}", $"{SkuName}", $"{GroupId}" };
}
