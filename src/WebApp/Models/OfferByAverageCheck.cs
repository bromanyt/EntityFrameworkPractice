using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebRetail.Models;

[Keyless]
public partial class OfferByAverageCheck : IRetailTable
{
    [Column(name: "Customer_Id")]
    [DisplayName("Customer ID")]
    public long CustomerId { get; set; }

    [Column(name: "Required_Check_Measure")]
    [DisplayName("Average Check Target Value")]
    public decimal? RequiredCheckMeasure { get; set; }

    [Column(name: "Offer_Group")]
    [DisplayName("Offer Group")]
    public string OfferGroup { get; set; } = null!;

    [Column(name: "Offer_Discount_Depth")]
    [DisplayName("Maximum Discount Depth")]
    public decimal? OfferDiscountDepth { get; set; }

    public List<string> GetRows() => new() { $"{CustomerId}", $"{RequiredCheckMeasure}",
        $"{OfferGroup}", $"{OfferDiscountDepth}" };
}

