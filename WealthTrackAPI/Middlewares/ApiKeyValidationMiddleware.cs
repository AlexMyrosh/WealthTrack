namespace WealthTrack.API.Middlewares
{
    public class ApiKeyValidationMiddleware(IConfiguration configuration) : IMiddleware
    {
        private const string ApiKeyHeaderName = "X-Api-Key";
        private readonly string _apiKey = configuration["ApiKey"];

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey) || extractedApiKey != _apiKey)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Unauthorized: Invalid API Key");
                return;
            }

            await next(context);
        }
    }
}
