namespace Web;
using Middleware;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        app.UseMiddleware<CredentialAuthenticator>();
        app.Run(async context =>
        {
            await context.Response.WriteAsync(
                "User details: " + context.Request.HttpContext.Items["userdetails"]
            );
        });
    }
}
