using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebRetail.Models;

public partial class PersonalInformation : IRetailTable
{
    [Key]
    [DisplayName("Customer ID")]
    public long CustomerId { get; set; }

    [DisplayName("Customer name")]
    public string CustomerName { get; set; } = null!;

    [DisplayName("Customer surname")]
    public string CustomerSurname { get; set; } = null!;

    [DisplayName("Customer primary email")]
    public string? CustomerPrimaryEmail { get; set; }

    [DisplayName("Customer primary phone")]
    public string? CustomerPrimaryPhone { get; set; }

    public virtual ICollection<Card> Cards { get; set; } = new List<Card>();

    public List<string> GetRows() => new()
        { $"{CustomerId}", $"{CustomerName}", $"{CustomerSurname}", $"{CustomerPrimaryEmail}", $"{CustomerPrimaryPhone}" };
}
