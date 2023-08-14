using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WebRetail.Models;

public partial class DateOfAnalysisFormation : IRetailTable
{
    [Key]
    [DisplayName("Analysis date")]
    public DateTime AnalysisFormation { get; set; }

    public List<string> GetRows() => new() { AnalysisFormation.ToString("dd.MM.yyyy H:mm:ss") };
}
