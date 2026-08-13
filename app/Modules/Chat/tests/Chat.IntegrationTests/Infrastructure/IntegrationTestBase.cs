using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using FluentAssertions;

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

    // Toda resposta com corpo vem no envelope { data, error, meta }. Estes três
    // helpers são o único lugar dos testes que conhece esse formato.
    // O corpo só pode ser lido uma vez: quem precisar de data e meta na mesma
    // resposta lê o envelope inteiro e navega nele.
    protected static async Task<JsonElement> EnvelopeAsync(HttpResponseMessage response)
        => await response.Content.ReadFromJsonAsync<JsonElement>();

    protected static async Task<JsonElement> DataAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var envelope = await EnvelopeAsync(response);

        envelope.GetProperty("error").ValueKind.Should().Be(JsonValueKind.Null);

        return envelope.GetProperty("data");
    }

    protected static async Task<JsonElement> GetDataAsync(HttpClient client, string url)
        => await DataAsync(await client.GetAsync(url));

    protected static async Task<JsonElement> ErrorAsync(HttpResponseMessage response)
    {
        var envelope = await EnvelopeAsync(response);

        envelope.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);

        return envelope.GetProperty("error");
    }

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
        var token = (await DataAsync(registerResponse)).GetProperty("token").GetString()!;

        var client = CreateAuthenticatedClient(token);

        var me = await GetDataAsync(client, "/api/v1/users/me");

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

        return (await DataAsync(response)).GetProperty("id").GetGuid();
    }

    protected static async Task<Guid> CreateGroupAsync(TestUser owner, string name, params Guid[] memberIds)
    {
        var response = await owner.Client.PostAsJsonAsync("/api/v1/conversations/group", new { name, memberIds });

        return (await DataAsync(response)).GetProperty("id").GetGuid();
    }

    protected static async Task<Guid> SendMessageAsync(TestUser user, Guid conversationId, string content = "Olá mundo")
    {
        var response = await user.Client.PostAsJsonAsync(
            $"/api/v1/conversations/{conversationId}/messages",
            new { content, contentType = "text" });

        return (await DataAsync(response)).GetProperty("id").GetGuid();
    }

    protected static async Task<JsonElement> GetConversationsAsync(TestUser user)
        => await GetDataAsync(user.Client, "/api/v1/conversations");
}

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<ChatAppFactory>;
