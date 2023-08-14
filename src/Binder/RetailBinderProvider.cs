using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using WebRetail.Models;


namespace WebRetail.Binder;

public class RetailBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Metadata.ModelType == typeof(IRetailTable))
        {
            return new BinderTypeModelBinder(typeof(IRetailTableEntityBinder));
        }
        return null;
    }
}
