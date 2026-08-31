using Newtonsoft.Json;
using SiloAI.Application.Shared;
using System.Text;
using System.Text.RegularExpressions;

public class InvalidContentCheckMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<InvalidContentCheckMiddleware> logger;

    public InvalidContentCheckMiddleware(RequestDelegate next
        , ILogger<InvalidContentCheckMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestContent = context.Request.QueryString.Value ?? "";

        if (context.Request.HasFormContentType)
        {
            foreach (var formValue in context.Request.Form)
            {
                requestContent += formValue;
            }
        }

        if (context.Request.ContentType != null && context.Request.ContentType.Contains("application/json"))
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);

            string? body = await reader.ReadToEndAsync();

            requestContent += body;

            context.Request.Body.Position = 0;
        }

        var path = context.Request.Path.Value;

        if (ContainsSqlInjection(requestContent))
        {
            logger.LogWarning("Possible SQL injection attempt detected: {RequestContent}", requestContent);

            var result = JsonConvert.SerializeObject(new ApiResponse()
            {
                Successful = false,
                Messages = ["Invalid request detected."],
                Value = 4
            });

            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsync(result);

            return;
        }

        await next(context);
    }

    private bool ContainsSqlInjection(string input)
    {
        if (input.HasNoValue())
        {
            return false;
        }

        string[] sqlInjectionPatterns =
        {
            @"(?:'|%27)\s*(?:or|and)(?:\s|%20)+(?:\(?\s*)?(?:\d+|(?:'|%27)[^'\r\n]*?(?:'|%27))\s*(?:=|%3D|<>|!=)\s*(?:\d+|(?:'|%27)[^'\r\n]*?(?:'|%27))",
            @"\bunion(?:\s|%20)+(?:all(?:\s|%20)+)?select\b",
            @"(?:;|%3B)(?:\s|%20)*(?:select\b[^;\r\n]*\bfrom\b|insert(?:\s|%20)+into\b|update\b[^;\r\n]*\bset\b|delete(?:\s|%20)+from\b|(?:drop|alter|create)(?:\s|%20)+(?:table|database)\b|exec(?:ute)?(?:\s|%20)+[\w.\[\]]+|declare(?:\s|%20)+@\w+)",
            @"\bselect\b[^;\r\n]*(?:\s|%20)+from(?:\s|%20)+[\w.\[\]]+(?:[^;\r\n]*(?:--|%2D%2D|#|%23))?",
            @"\b(?:insert(?:\s|%20)+into|update\b[^;\r\n]*\bset|delete(?:\s|%20)+from|(?:drop|alter|create)(?:\s|%20)+(?:table|database)|exec(?:ute)?(?:\s|%20)+[\w.\[\]]+|declare(?:\s|%20)+@\w+)\b",
            @"\bcast\s*\([^)]*\bas\b[^)]*\)|\bconvert\s*\([^,]+,[^)]*\)"
        };

        foreach (var pattern in sqlInjectionPatterns)
        {
            if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                return true;
            }
        }

        return false;
    }
}
