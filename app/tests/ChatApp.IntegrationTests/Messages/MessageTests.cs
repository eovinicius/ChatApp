using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using ChatApp.IntegrationTests.Infrastructure;

using FluentAssertions;

namespace ChatApp.IntegrationTests.Messages;

public class MessageTests(ChatAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task SendMessage_Deve_Retornar_201_Com_MessageId()
    {
        var token = await RegisterAndLoginAsync();
        var client = CreateAuthenticatedClient(token);
        var roomId = await CreateRoomAsync(client);

        var response = await client.PostAsJsonAsync("/api/v1/message", new
        {
            roomId,
            content = "Olá, mundo!",
            contentType = "text"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("id").GetGuid().Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task SendMessage_Sem_Autenticacao_Deve_Retornar_401()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/message", new
        {
            roomId = Guid.NewGuid(),
            content = "Olá",
            contentType = "text"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendMessage_Usuario_Nao_Membro_Deve_Retornar_400()
    {
        var ownerToken = await RegisterAndLoginAsync();
        var ownerClient = CreateAuthenticatedClient(ownerToken);
        var roomId = await CreateRoomAsync(ownerClient);

        var outsiderToken = await RegisterAndLoginAsync();
        var outsiderClient = CreateAuthenticatedClient(outsiderToken);

        var response = await outsiderClient.PostAsJsonAsync("/api/v1/message", new
        {
            roomId,
            content = "Não devia entrar",
            contentType = "text"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task EditMessage_Deve_Retornar_204()
    {
        var token = await RegisterAndLoginAsync();
        var client = CreateAuthenticatedClient(token);
        var roomId = await CreateRoomAsync(client);
        var messageId = await SendMessageAsync(client, roomId);

        var response = await client.PutAsJsonAsync($"/api/v1/message/{messageId}", new
        {
            roomId,
            content = "Mensagem editada"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task EditMessage_Por_Outro_Usuario_Deve_Retornar_400()
    {
        var ownerToken = await RegisterAndLoginAsync();
        var ownerClient = CreateAuthenticatedClient(ownerToken);
        var roomId = await CreateRoomAsync(ownerClient);
        var messageId = await SendMessageAsync(ownerClient, roomId);

        var otherToken = await RegisterAndLoginAsync();
        var otherClient = CreateAuthenticatedClient(otherToken);
        await otherClient.PostAsJsonAsync($"/api/v1/chatroom/{roomId}/join", new { });

        var response = await otherClient.PutAsJsonAsync($"/api/v1/message/{messageId}", new
        {
            roomId,
            content = "Tentativa de edição"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteMessage_Deve_Retornar_204()
    {
        var token = await RegisterAndLoginAsync();
        var client = CreateAuthenticatedClient(token);
        var roomId = await CreateRoomAsync(client);
        var messageId = await SendMessageAsync(client, roomId);

        var response = await client.DeleteAsync($"/api/v1/message/{messageId}?roomId={roomId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetMessages_Deve_Retornar_200_Com_Lista()
    {
        var token = await RegisterAndLoginAsync();
        var client = CreateAuthenticatedClient(token);
        var roomId = await CreateRoomAsync(client);
        await SendMessageAsync(client, roomId, "Primeira mensagem");
        await SendMessageAsync(client, roomId, "Segunda mensagem");

        var response = await client.GetAsync($"/api/v1/message?roomId={roomId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var messages = await response.Content.ReadFromJsonAsync<JsonElement>();
        messages.GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetMessages_Usuario_Nao_Membro_Deve_Retornar_400()
    {
        var ownerToken = await RegisterAndLoginAsync();
        var ownerClient = CreateAuthenticatedClient(ownerToken);
        var roomId = await CreateRoomAsync(ownerClient);

        var outsiderToken = await RegisterAndLoginAsync();
        var outsiderClient = CreateAuthenticatedClient(outsiderToken);

        var response = await outsiderClient.GetAsync($"/api/v1/message?roomId={roomId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
