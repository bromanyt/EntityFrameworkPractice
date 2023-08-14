using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebRetail.Models;

public partial class Card : IRetailTable
{
    [Key]
    [DisplayName("Customer card ID")]
    public long CustomerCardId { get; set; }

    [DisplayName("Customer ID")]
    public long CustomerId { get; set; }

    public virtual PersonalInformation Customer { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public List<string> GetRows() => new() { $"{CustomerCardId}", $"{CustomerId}" };
}
