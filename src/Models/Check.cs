using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebRetail.Models;

public partial class Check : IRetailTable
{
    [Key]
    [DisplayName("Transaction ID")]
    public long TransactionId { get; set; }

    [Key]
    [DisplayName("Sku ID")]
    public long SkuId { get; set; }

    [DisplayName("Sku Amount")]
    public decimal SkuAmount { get; set; }

    [DisplayName("Sku summ")]
    public decimal SkuSumm { get; set; }

    [DisplayName("Sku summ paid")]
    public decimal SkuSummPaid { get; set; }

    [DisplayName("Sku discount")]
    public decimal SkuDiscount { get; set; }

    public virtual ProductGrid Sku { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;

    public List<string> GetRows() => new() { $"{TransactionId}", $"{SkuId}", $"{SkuAmount}", $"{SkuSumm}", $"{SkuSummPaid}", $"{SkuDiscount}" };
}
