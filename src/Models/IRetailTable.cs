namespace WebRetail.Models;

public interface IRetailTable
{
    public string GetTableName() => this.GetType().Name;

    public List<string> GetRows();

    public List<string> GetColumnNames()
    {
        List<string> names = new();
        Type type = this.GetType();
        var properties = type.GetProperties().Where(p => !p.GetMethod!.IsVirtual).ToArray();
        foreach (var property in properties)
        {
            string displayName = property.Name;
            var displayAttribute = property.GetCustomAttributesData().Where(p => p.AttributeType == typeof(System.ComponentModel.DisplayNameAttribute)).ToList();
            if (displayAttribute.Count > 0)
                displayName = displayAttribute.First().ConstructorArguments.First().ToString().Trim('"');
            names.Add(displayName);
        }
        return names;
    }
}
