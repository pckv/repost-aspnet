using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RepostAspNet.Models;

namespace RepostAspNet
{
    public class ErrorResponseFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Use ErrorResponse as result when the status code is >= 400
            if (context.Result is ObjectResult result && result.StatusCode >= 400 && result.Value is string detail)
            {
                context.Result = new ObjectResult(new ErrorResponse {Detail = detail})
                {
                    StatusCode = result.StatusCode
                };
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }
    }
}