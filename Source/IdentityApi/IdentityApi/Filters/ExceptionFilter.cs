﻿using IdentityApi.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace IdentityApi.Filters
{
    /// <summary>
    /// Exception filter to convert a exception into a http error response.
    /// </summary>
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<ExceptionFilter> _logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            var exception = context.Exception;

            switch (context.Exception)
            {
                case AccountIsNotVerifiedException:
                case Required2FAException:
                case IpBlockedException:
                case UserIncorrectLoginException:
                case PasswordLeakedException:
                case AccountLockedException:
                    context.Result = GenerateExceptionResult(exception.Message, HttpStatusCode.Forbidden);
                    context.ExceptionHandled = true;
                    break;
                case UserAlreadyExistsException:
                    context.Result = GenerateExceptionResult(exception.Message, HttpStatusCode.Conflict);
                    context.ExceptionHandled = true;
                    break;              
                case SendMessageIssueException:
                    context.Result = GenerateExceptionResult(exception.Message, HttpStatusCode.InternalServerError);
                    context.ExceptionHandled = true;
                    break;
                default:
                    context.Result = GenerateExceptionResult("Unexpected error occurred", HttpStatusCode.InternalServerError);
                    _logger.LogError(exception, "Unexpected error occurred");
                    break;
            }
        }

        /// <summary>
        /// Generates a exception response which can be understood by the http protocol.
        /// </summary>
        /// <param name="message">The message to send as response.</param>
        /// <param name="code">The status code to send as response.</param>
        private ObjectResult GenerateExceptionResult(string message, HttpStatusCode code)
        {
            return new ObjectResult(new { error = message })
            {
                StatusCode = (int)code
            };
        }
    }
}
