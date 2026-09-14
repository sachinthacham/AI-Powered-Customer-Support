using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SupportIQ.Infrastructure.Persistence.Seed;
using Xunit;

namespace SupportIQ.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class AuthControllerTests
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithSeededAdminCredentials_ReturnsToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = DbSeeder.DefaultAdminEmail,
            password = DbSeeder.DefaultAdminPassword
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("token");
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = DbSeeder.DefaultAdminEmail,
            password = "definitely-wrong-password"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Returns401NotAnInternalError()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "nobody@nowhere.com",
            password = "whatever123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
