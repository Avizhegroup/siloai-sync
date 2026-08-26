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
            // Classic SQL Injection:
            // ' OR 1=1 --
          @"(['""\s]|%27)\s*(or|and)\s+[\w\s'""%]+\s*(=|%3D)\s*[\w'""%]+(\s*(--|#|%23))?",


           // Boolean SQL Injection:
           // OR '1'='1'
           // AND 1=1
          @"\b(or|and)\b\s+['""]?\d+['""]?\s*(=|%3D)\s*['""]?\d+['""]?",


            // SQL comments used for injection:
            // abc'--
            // abc'#
          @"(['""])\s*(or|and)\s+\d+\s*=\s*\d+\s*(--|#|%23)",


            // UNION based injection:
            // UNION SELECT ...
          @"\bunion\b\s+(all\s+)?\bselect\b\s+",


           // Data modification commands:
           // DROP TABLE Users
           // DELETE FROM Users
           // ALTER TABLE Users
           // TRUNCATE TABLE Users
          @"\b(drop|delete|alter|truncate)\b\s+(table|database|schema|view|procedure|function)\s+[\w\[\]]+",


          // INSERT INTO Users
         @"\binsert\s+into\s+[\w\[\]]+",


          // UPDATE Users SET
          @"\bupdate\s+[\w\[\]]+\s+set\b",


          // EXEC dbo.Procedure
          @"\bexec(ute)?\s+(\[?\w+\]?\.)?\[?\w+\]?",


          // DECLARE @x, CAST(), CONVERT()
          // only when used as SQL syntax, not as plain words
          @"\bdeclare\s+@\w+",
          @"\bcast\s*\(",
          @"\bconvert\s*\(",


          // Stacked queries:
          // ; DROP TABLE
          // ; DELETE FROM
          @";\s*(drop|delete|alter|truncate|insert|update)\s+[\w\[\]]+"

          };

        foreach (var pattern in sqlInjectionPatterns)
        {
            if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
