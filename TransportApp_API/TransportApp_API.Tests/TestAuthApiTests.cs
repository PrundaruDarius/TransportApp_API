using System.Net;
using FluentAssertions;
using Xunit;

namespace TransportApp_API.Tests;

public class TestAuthApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TestAuthApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Public_Endpoint_Should_Return_Ok()
    {
        var response = await _client.GetAsync("/api/TestAuth/public");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Private_Endpoint_Should_Return_Unauthorized_Without_Token()
    {
        var response = await _client.GetAsync("/api/TestAuth/private");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Private_Endpoint_Should_Return_Ok_With_Valid_Token()
    {
        var token = await AuthTestHelper.RegisterAndLoginAsync(_client);
        AuthTestHelper.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/TestAuth/private");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("private works");
    }
}