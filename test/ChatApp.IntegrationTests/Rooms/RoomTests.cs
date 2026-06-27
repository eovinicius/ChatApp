using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using ChatApp.IntegrationTests.Infrastructure;

using FluentAssertions;

namespace ChatApp.IntegrationTests.Rooms;

public class RoomTests(ChatAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateRoom_Autenticado_Deve_Retornar_201_Com_Id()
    {
        var token = await RegisterAndLoginAsync();
        var client = CreateAuthenticatedClient(token);

        var response = await client.PostAsJsonAsync("/api/chatroom", new
        {
            roomName = "Sala de Testes",
            isPrivate = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("id").GetGuid().Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateRoom_Sem_Token_Deve_Retornar_401()
    {
        var response = await Client.PostAsJsonAsync("/api/chatroom", new
        {
            roomName = "Sala",
            isPrivate = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateRoom_Com_Nome_Vazio_Deve_Retornar_400()
    {
        var token = await RegisterAndLoginAsync();
        var client = CreateAuthenticatedClient(token);

        var response = await client.PostAsJsonAsync("/api/chatroom", new
        {
            roomName = "   ",
            isPrivate = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task JoinRoom_Usuario_Valido_Deve_Retornar_200()
    {
        var ownerToken = await RegisterAndLoginAsync();
        var ownerClient = CreateAuthenticatedClient(ownerToken);
        var roomId = await CreateRoomAsync(ownerClient);

        var memberToken = await RegisterAndLoginAsync();
        var memberClient = CreateAuthenticatedClient(memberToken);

        var response = await memberClient.PostAsJsonAsync($"/api/chatroom/{roomId}/join", new { });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task JoinRoom_Ja_Membro_Deve_Retornar_400()
    {
        var token = await RegisterAndLoginAsync();
        var client = CreateAuthenticatedClient(token);
        var roomId = await CreateRoomAsync(client);

        // Tenta entrar novamente (já é membro por ser o criador)
        var response = await client.PostAsJsonAsync($"/api/chatroom/{roomId}/join", new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LeaveRoom_Ultimo_Membro_Deve_Retornar_204()
    {
        var token = await RegisterAndLoginAsync();
        var client = CreateAuthenticatedClient(token);
        var roomId = await CreateRoomAsync(client);

        var response = await client.DeleteAsync($"/api/chatroom/{roomId}/leave");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CreateRoomAnonymous_Deve_Retornar_201()
    {
        var response = await Client.PostAsJsonAsync("/api/chatroom/anonymous", new
        {
            roomName = "Sala Anônima",
            guestName = "Visitante"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("id").GetGuid().Should().NotBe(Guid.Empty);
    }
}
