using Microsoft.AspNetCore.Mvc.Razor;

namespace ResturantPL.Extensions;

// Definition controllers live in Controllers/Definitions, so their views live in Views/Definitions/{Controller}.
public class DefinitionsViewLocationExpander : IViewLocationExpander
{
    public void PopulateValues(ViewLocationExpanderContext context)
    {
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        return new[] { "/Views/Definitions/{1}/{0}.cshtml" }.Concat(viewLocations);
    }
}
