using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebRetail.Models;

public partial class Store : IRetailTable
{
    [Key]
    [DisplayName("Transaction store ID")]
    public long TransactionStoreId { get; set; }

    [Key]
    [DisplayName("Sku ID")]
    public long SkuId { get; set; }

    [DisplayName("Sku purchase price")]
    public decimal SkuPurchasePrice { get; set; }

    [DisplayName("Sku retail price")]
    public decimal SkuRetailPrice { get; set; }

    public virtual ProductGrid Sku { get; set; } = null!;

    public List<string> GetRows() => new() { $"{TransactionStoreId}", $"{SkuId}", $"{SkuPurchasePrice}", $"{SkuRetailPrice}" };

}
