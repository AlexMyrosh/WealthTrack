namespace WealthTrack.API.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                KeyNotFoundException    => StatusCodes.Status404NotFound,
                _                       => StatusCodes.Status400BadRequest
            };

            var response = new
            {
                error = ex.Message,
                type  = ex.GetType().Name
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}