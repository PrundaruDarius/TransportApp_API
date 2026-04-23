using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace TransportApp_API.Tests;

public static class AuthTestHelper
{

    public static async Task<string> RegisterAndLoginAsync(HttpClient client)
    {
        var email = $"test_{Guid.NewGuid()}@gmail.com";
        var password = "Test123";

        var registerResponse = await client.PostAsJsonAsync("/api/Auth/register", new
        {
            email,
            password
        });

        var registerBody = await registerResponse.Content.ReadAsStringAsync();
        Console.WriteLine(registerBody);

        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await client.PostAsJsonAsync("/api/Auth/login", new
        {
            email,
            password
        });

        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        if (loginBody == null || string.IsNullOrWhiteSpace(loginBody.Token))
            throw new Exception("Token was not returned.");

        return loginBody.Token;
    }

    public static void SetBearerToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private sealed class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}