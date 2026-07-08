using Application.Constants;
using Application.Contracts.Common;
using Application.Exceptions;
using Serilog;
using static Application.Exceptions.CustomException;

namespace WebApi.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (CustomException ex)
            {
                var statusCode = ex switch
                {
                    NotFoundException => StatusCodes.Status404NotFound,
                    ForbiddenException => StatusCodes.Status403Forbidden,
                    BadRequestException => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status500InternalServerError,
                };

                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsJsonAsync(
                    WriteResponse.Failure(ex.ErrorCode, ex.Message)
                );
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error occurred");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(
                    WriteResponse.Failure(ErrorMessages.Common.InternalServerError)
                );
            }
        }
    }
}
