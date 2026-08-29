public static partial class Program
{
    public static WebApplication ConfigureAiApi(this WebApplication app)
    {
        app.UseSwagger();

        app.UseSwaggerUI();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        app.UseInfrastructureSharedMiddlewares();

        return app;
    }
}
