using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace TransportApp_API.Tests;

public class AuthApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Should_Return_Ok_For_Valid_User()
    {
        var email = $"register_{Guid.NewGuid()}@gmail.com";

        var response = await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            email,
            password = "Test123"
        });

        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine(body);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_Should_Return_BadRequest_For_Duplicate_Email()
    {
        var email = $"duplicate_{Guid.NewGuid()}@gmail.com";

        await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            email,
            password = "Test123"
        });

        var secondResponse = await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            email,
            password = "Test123"
        });

        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_Should_Return_BadRequest_For_Invalid_Password()
    {
        var email = $"weak_{Guid.NewGuid()}@gmail.com";

        var response = await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            email,
            password = "abc"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_Should_Return_Token_For_Valid_Credentials()
    {
        var email = $"login_{Guid.NewGuid()}@gmail.com";

        await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            email,
            password = "Test123"
        });

        var response = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            email,
            password = "Test123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();

        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_Should_Return_Unauthorized_For_Wrong_Password()
    {
        var email = $"wrongpass_{Guid.NewGuid()}@gmail.com";

        await _client.PostAsJsonAsync("/api/Auth/register", new
        {
            email,
            password = "Test123"
        });

        var response = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            email,
            password = "Wrong123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_Should_Return_Unauthorized_For_NonExisting_User()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            email = $"missing_{Guid.NewGuid()}@gmail.com",
            password = "Test123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}