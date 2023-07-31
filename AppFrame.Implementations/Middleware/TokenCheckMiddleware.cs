using Microsoft.AspNetCore.Http;

namespace AppFrame.Implementations.Middleware
{
    public class APIKeyCheckMiddleware
    {
        private readonly RequestDelegate _next;
        public List<string> KeyList = new() {"this is a system shared k3y! 234gfsgds43"};

        public APIKeyCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Perform the KEY check here
            string apikey = context.Request.Headers["api-key"];
            if (string.IsNullOrEmpty(apikey) || !IsValidAPIKey(apikey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }

        private bool IsValidAPIKey(string apikey)
        {
            // Implement your KEY validation logic here
            // For example, check if the key is valid, not expired, etc.
            // Return true if the KEY is valid, false otherwise
            return KeyList.Contains(apikey);
        }
    }
}