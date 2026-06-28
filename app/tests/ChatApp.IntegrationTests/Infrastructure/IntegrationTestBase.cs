using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ChatApp.IntegrationTests.Infrastructure;

[Collection("Integration")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly ChatAppFactory Factory;

    private static int _userCounter = 0;

    protected IntegrationTestBase(ChatAppFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task<string> RegisterAndLoginAsync(string? username = null, string? password = "Senha@123")
    {
        var id = Interlocked.Increment(ref _userCounter);
        username ??= $"testuser_{id}_{Guid.NewGuid():N}";

        await Client.PostAsJsonAsync("/api/user/register", new
        {
            name = $"Test User {id}",
            username,
            password
        });

        var loginResponse = await Client.PostAsJsonAsync("/api/user/login", new { username, password });
        loginResponse.EnsureSuccessStatusCode();

        var json = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("token").GetString()!;
    }

    protected HttpClient CreateAuthenticatedClient(string token)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected async Task<Guid> CreateRoomAsync(HttpClient client, string roomName = "Sala de Teste")
    {
        var response = await client.PostAsJsonAsync("/api/chatroom", new
        {
            roomName,
            isPrivate = false
        });
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetGuid();
    }

    protected async Task<Guid> SendMessageAsync(HttpClient client, Guid roomId, string content = "Olá mundo")
    {
        var response = await client.PostAsJsonAsync("/api/message", new
        {
            roomId,
            content,
            contentType = "text"
        });
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetGuid();
    }
}

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<ChatAppFactory>;
