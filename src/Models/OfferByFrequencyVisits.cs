using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebRetail.Models;

[Keyless]
public class OfferByFrequencyVisits : IRetailTable
{
    [Column(name: "customer_id")]
    [DisplayName("Customer ID")]
    public long CustomerId { get; set; }

    [Column(name: "start_date")]
    [DisplayName("Period Start Date")]
    public DateOnly StartDate { get; set; }

    [Column(name: "end_date")]
    [DisplayName("Period End Date")]
    public DateOnly EndDate { get; set; }

    [Column(name: "required_transactions_count")]
    [DisplayName("Target Number of Transactions")]
    public decimal? RequiredTransactionsCount { get; set; }

    [Column(name: "group_name")]
    [DisplayName("Offer Group")]
    public string GroupName { get; set; } = null!;

    [Column(name: "offer_discount_depth")]
    [DisplayName("Maximum Discount Depth")]
    public decimal? OfferDiscountDepth { get; set; }

    public List<string> GetRows() => new() { $"{CustomerId}", StartDate.ToString("dd.MM.yyyy"),
        EndDate.ToString("dd.MM.yyyy"), $"{RequiredTransactionsCount}", $"{GroupName}", $"{OfferDiscountDepth}" };
}
