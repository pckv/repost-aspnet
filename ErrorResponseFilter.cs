using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RepostAspNet.Models;

namespace RepostAspNet
{
    public class ErrorResponseFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Convert StatusException to ErrorResponse
            if (context.Exception is StatusException exception)
            {
                context.Result = new ObjectResult(new ErrorResponse {Detail = exception.Detail})
                {
                    StatusCode = exception.Status
                };
                context.ExceptionHandled = true;
                return;
            }

            // Use ErrorResponse as result when an ActionResult status code is >= 400
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