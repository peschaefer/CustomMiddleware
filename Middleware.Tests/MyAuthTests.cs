using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Web.Middleware;

namespace Middleware.Tests;

public class MyAuthTests : IAsyncLifetime
{
    IHost? host;

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services => { })
                    .Configure(app =>
                    {
                        app.UseMiddleware<CredentialAuthenticator>();
                        app.Run(async context =>
                        {
                            await context.Response.WriteAsync("Authenticated!");
                        });
                    });
            })
            .StartAsync();
    }

    [Fact]
    public async Task MiddlewareTest_FailWhenNotAuthenticated()
    {
        var response = await host.GetTestClient().GetAsync("/");
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("Failed!", result);
    }

    [Fact]
    public async Task MiddlewareTest_Authenticated()
    {
        var response = await host.GetTestClient().GetAsync("/?username=user1&password=password1");
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("Authenticated!", result);
    }

    [Fact]
    public async Task MiddlewareTest_NoPassword()
    {
        var response = await host.GetTestClient().GetAsync("/?username=user1");
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("Failed!", result);
    }

    [Fact]
    public async Task MiddlewareTest_WrongCredentials()
    {
        var response = await host.GetTestClient().GetAsync("/?username=user5&password=password2");
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal("Failed!", result);
    }
}
