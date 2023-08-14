using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using WebRetail.Models;

namespace WebRetail.Binder;

public class IRetailTableEntityBinder : IModelBinder
{

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        string modelType = bindingContext.ValueProvider.GetValue("modelType").ToString();
        switch (modelType)
        {
            case "ProductGrid":
                CreateObject<ProductGrid>(bindingContext);
                break;
            case "SkuGroup":
                CreateObject<SkuGroup>(bindingContext);
                break;
            case "Check":
                CreateObject<Check>(bindingContext);
                break;
            case "Card":
                CreateObject<Card>(bindingContext);
                break;
            case "PersonalInformation":
                CreateObject<PersonalInformation>(bindingContext);
                break;
            case "Store":
                CreateObject<Store>(bindingContext);
                break;
            case "Transaction":
                CreateObject<Transaction>(bindingContext);
                break;
            case "DateOfAnalysisFormation":
                CreateObject<DateOfAnalysisFormation>(bindingContext);
                break;
        }
        return Task.CompletedTask;
    }

    public void CreateObject<T>(ModelBindingContext bindingContext)
    {
        T obj = (T)Activator.CreateInstance(typeof(T))!;
        try
        {
            var properties = typeof(T).GetProperties().Where(p => !p.GetMethod!.IsVirtual).ToArray();
            foreach (var property in properties)
            {
                var name = property.Name;
                var value = bindingContext.ValueProvider.GetValue(name).ToString();
                property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
            }
        }
        catch (FormatException)
        {
            bindingContext.Result = ModelBindingResult.Failed();
        }

        bindingContext.Result = ModelBindingResult.Success(obj);
    }
}