using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace IdentityApi.Filters
{
    /// <summary>
    /// Filter to aggregrate modelstate errors into 1 single message
    /// </summary>
    public class ModelStateFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                // Create array of valid error messages
                var messageList = (from modelState in context.ModelState.Values
                               from error in modelState.Errors
                               select error.ErrorMessage)
                               .ToList();

                // Create single message with all the error messages
                var message = String.Concat(messageList);

                // Create new http result with the new message.
                context.Result = new ObjectResult(new { error = message })
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
    }
}
