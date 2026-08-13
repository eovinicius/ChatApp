using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Chat.IntegrationTests.Infrastructure;

[Collection("Integration")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly ChatAppFactory Factory;

    private static int _userCounter;

    protected IntegrationTestBase(ChatAppFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    protected sealed record TestUser(Guid Id, string Username, string Token, HttpClient Client);

    protected async Task<TestUser> CreateUserAsync(string? password = "Senha@123")
    {
        var id = Interlocked.Increment(ref _userCounter);
        var username = $"testuser_{id}_{Guid.NewGuid():N}"[..30];

        var registerResponse = await Client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            name = $"Test User {id}",
            username,
            password
        });
        registerResponse.EnsureSuccessStatusCode();

        var token = (await registerResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("token").GetString()!;

        var client = CreateAuthenticatedClient(token);

        var me = await client.GetFromJsonAsync<JsonElement>("/api/v1/users/me");

        return new TestUser(me.GetProperty("id").GetGuid(), username, token, client);
    }

    protected HttpClient CreateAuthenticatedClient(string token)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected static async Task<Guid> StartDirectAsync(TestUser user, Guid targetUserId)
    {
        var response = await user.Client.PostAsJsonAsync("/api/v1/conversations/direct", new { targetUserId });
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetGuid();
    }

    protected static async Task<Guid> CreateGroupAsync(TestUser owner, string name, params Guid[] memberIds)
    {
        var response = await owner.Client.PostAsJsonAsync("/api/v1/conversations/group", new { name, memberIds });
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetGuid();
    }

    protected static async Task<Guid> SendMessageAsync(TestUser user, Guid conversationId, string content = "Olá mundo")
    {
        var response = await user.Client.PostAsJsonAsync(
            $"/api/v1/conversations/{conversationId}/messages",
            new { content, contentType = "text" });
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetGuid();
    }

    protected static async Task<JsonElement> GetConversationsAsync(TestUser user)
        => await user.Client.GetFromJsonAsync<JsonElement>("/api/v1/conversations");
}

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<ChatAppFactory>;
