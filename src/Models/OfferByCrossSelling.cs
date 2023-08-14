using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebRetail.Models;

[Keyless]
public class OfferByCrossSelling : IRetailTable
{
    [Column(name: "Customer_ID")]
    [DisplayName("Customer ID")]
    public long CustomerId { get; set; }

    [Column(name: "SKU_Name")]
    [DisplayName("Sku Offers")]
    public string SkuName { get; set; } = null!;

    [Column(name: "Offer_Discount_Depth")]
    [DisplayName("Maximum Discount Depth")]
    public decimal? OfferDiscountDepth { get; set; }

    public List<string> GetRows() => new() { $"{CustomerId}", $"{SkuName}", $"{OfferDiscountDepth}" };
}

