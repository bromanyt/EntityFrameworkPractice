using System.Text;
using WebRetail.Models;

namespace WebRetail.ViewModels;

public class ExportViewModel
{
    public List<IRetailTable> Table { get; set; } = new List<IRetailTable>();

    public string Delimiter { get; set; } = ",";

    public bool IsEntity { get; set; } = true;

    public string FormExport()
    {
        StringBuilder csvFile = new();
        foreach (var recordItem in Table)
        {
            var propertiesValues = recordItem.GetRows();
            string propertiesValuesStr = string.Join(Delimiter, propertiesValues);
            csvFile.Append(propertiesValuesStr);
            AddNewLine(csvFile);
        }
        RemoveLastNewLine(csvFile);
        return csvFile.ToString();
    }

    private void AddNewLine(StringBuilder csvFile)
    {
        if (IsEntity)
            csvFile.Append('\n');
        else
            csvFile.Append(@"\n");
    }

    private void RemoveLastNewLine(StringBuilder csvFile)
    {
        int quantity = 2;
        if (IsEntity)
            quantity = 1;
        csvFile.Remove(csvFile.Length - quantity, quantity);
    }
}
