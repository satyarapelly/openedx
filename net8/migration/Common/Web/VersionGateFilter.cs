using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Commerce.Payments.Common.Web;
using System.Threading.Tasks;

public sealed class VersionGateFilter : IAsyncActionFilter
{
    private readonly VersionedControllerResolver _selector;

    public VersionGateFilter(VersionedControllerResolver selector) => _selector = selector;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var allowedType = _selector.ResolveAllowedController(context.HttpContext);
        if (allowedType is null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        var selectedType = context.Controller.GetType();
        if (selectedType != allowedType)
        {
            // A controller was selected by routing, but it's not allowed for this version.
            context.Result = new NotFoundResult();
            return;
        }

        await next();
    }
}
