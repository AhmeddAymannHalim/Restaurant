using Microsoft.AspNetCore.Mvc.Razor;

namespace ResturantPL.Extensions;

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
