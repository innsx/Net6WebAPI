using Application.Exceptions;
using Application.Wrappers;
using System.Net;
using System.Text.Json;

namespace WebApi.ExceptionHandlers
{
    public class ErrorHandlerInMiddleware
    {
        //RequestDelegate definition: A method that takes HttpContext and returns a Task.
        //RequestDelegate is a standard component in custom ASP.NET Core middleware classes.
        //RequestDelegate represents the next RequestDelegate in the HTTP request pipeline,
        //allowing your middleware to pass control to the next component.
        //It is initialized in the constructor, enabling InvokeAsync to call it. 
        private readonly RequestDelegate _requestDelegateNext;
        private readonly ILogger<ErrorHandlerInMiddleware> _logger;

        public ErrorHandlerInMiddleware(RequestDelegate requestDelegateNext, ILogger<ErrorHandlerInMiddleware> logger)
        {
            _requestDelegateNext = requestDelegateNext;
            _logger = logger;
        }

        //INVOKING HttpContext
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                //Pipeline Flow: await _requestDelegateNext(context); invokes the next middleware.
                //Request Interception: Allows executing code before passing the request to the rest of the pipeline.
                await _requestDelegateNext(httpContext);

                //Response Interception: Allows executing code after _next has finished,
                //                         enabling modification of the response.

                //Terminal Middleware: If the middleware handles the request entirely and does not call _next,
                //                      it is considered "terminal" or "short-circuiting" the pipeline.
            }
            catch (Exception ex)
            {
                var httpResponse = httpContext.Response;

                httpResponse.ContentType = "application/json";

                var apiResponseModel = new CustomizedAPIResponse<string> { Succeed = false, Message  = ex.Message };

                switch (ex)
                {
                    case ApiErrorExceptions e:
                        httpResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;

                    case ValidationErrorExceptions e:
                        httpResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                        apiResponseModel.Errors = e.Errors!;
                        break;

                    default:
                        httpResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                _logger.LogError(ex, ex.Message);

                var serializedResponse = JsonSerializer.Serialize(apiResponseModel);

                await httpResponse.WriteAsync(serializedResponse);
            }
        }
    }
}
