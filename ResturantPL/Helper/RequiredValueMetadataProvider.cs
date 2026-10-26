using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace ResturantPL.Helper;

// .NET marks every int/decimal/date/enum as required with an English message; this gives it our translated one.
public class RequiredValueMetadataProvider : IValidationMetadataProvider
{
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        var type = context.Key.ModelType;
        if (!type.IsValueType || Nullable.GetUnderlyingType(type) != null)
        {
            return;
        }

        if (context.ValidationMetadata.ValidatorMetadata.OfType<RequiredAttribute>().Any())
        {
            return;
        }

        context.ValidationMetadata.ValidatorMetadata.Add(new RequiredAttribute { ErrorMessage = "Val.RequiredField" });
    }
}
