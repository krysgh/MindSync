using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace MindSync.Tests.IntegrationTests;

public class AuthControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var uniqueEmail = $"integration_{Guid.NewGuid()}@mindsync.com";
        var command = new
        {
            FirstName = "Carlos",
            LastName = "Eduardo",
            Email = uniqueEmail,
            Password = "SenhaSegura123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new
        {
            FirstName = "Carlos",
            LastName = "Eduardo",
            Email = "email-invalido.com",
            Password = "123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk()
    {
        // Arrange
        var uniqueEmail = $"login_test_{Guid.NewGuid()}@mindsync.com";
        var registerCommand = new
        {
            FirstName = "Teste",
            LastName = "Login",
            Email = uniqueEmail,
            Password = "Password123!"
        };
        await _client.PostAsJsonAsync("api/auth/register", registerCommand);

        var loginQuery = new
        {
            Email = uniqueEmail,
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/auth/login", loginQuery);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
