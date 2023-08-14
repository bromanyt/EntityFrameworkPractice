using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebRetail.Models;

public partial class Transaction : IRetailTable
{
    [Key]
    [DisplayName("Transaction ID")]
    public long TransactionId { get; set; }

    [DisplayName("Customer card ID")]
    public long CustomerCardId { get; set; }

    [DisplayName("Transaction summ")]
    public decimal TransactionSumm { get; set; }

    [DisplayName("Transaction datetime")]
    public DateTime TransactionDatetime { get; set; }

    [DisplayName("Transaction store ID")]
    public long TransactionStoreId { get; set; }

    public virtual Card CustomerCard { get; set; } = null!;

    public List<string> GetRows() => new() { $"{TransactionId}", $"{CustomerCardId}", $"{TransactionSumm}",
        TransactionDatetime.ToString("dd.MM.yyyy H:mm:ss"), $"{TransactionStoreId}" };
}
