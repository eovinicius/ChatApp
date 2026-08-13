using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Chat.IntegrationTests.Infrastructure;

using FluentAssertions;

namespace Chat.IntegrationTests.Messages;

public class MessageTests(ChatAppFactory factory) : IntegrationTestBase(factory)
{
    private static async Task<JsonElement> GetMessagesAsync(TestUser user, Guid conversationId)
        => await user.Client.GetFromJsonAsync<JsonElement>($"/api/v1/conversations/{conversationId}/messages");

    private static int UnreadFor(JsonElement conversations, Guid conversationId)
        => conversations.EnumerateArray()
            .First(c => c.GetProperty("id").GetGuid() == conversationId)
            .GetProperty("unreadCount").GetInt32();

    [Fact]
    public async Task Deveria_enviar_e_listar_mensagens()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var conversationId = await StartDirectAsync(alice, bob.Id);

        // Act
        await SendMessageAsync(alice, conversationId, "primeira");
        await SendMessageAsync(alice, conversationId, "segunda");
        var messages = await GetMessagesAsync(bob, conversationId);

        // Assert — mais recentes primeiro, e com o Id, que o cliente precisa para editar/apagar.
        var items = messages.EnumerateArray().ToList();
        items.Should().HaveCount(2);
        items[0].GetProperty("content").GetString().Should().Be("segunda");
        items[0].GetProperty("id").GetGuid().Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Deveria_contar_mensagens_nao_lidas_do_destinatario()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var conversationId = await StartDirectAsync(alice, bob.Id);

        // Act
        await SendMessageAsync(alice, conversationId, "1");
        await SendMessageAsync(alice, conversationId, "2");
        await SendMessageAsync(alice, conversationId, "3");

        var bobConversations = await GetConversationsAsync(bob);
        var aliceConversations = await GetConversationsAsync(alice);

        // Assert — quem enviou não tem não-lidas.
        UnreadFor(bobConversations, conversationId).Should().Be(3);
        UnreadFor(aliceConversations, conversationId).Should().Be(0);
    }

    [Fact]
    public async Task Deveria_zerar_nao_lidas_ao_marcar_como_lida()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var conversationId = await StartDirectAsync(alice, bob.Id);
        await SendMessageAsync(alice, conversationId, "1");
        var lastMessageId = await SendMessageAsync(alice, conversationId, "2");

        // Act
        var response = await bob.Client.PostAsJsonAsync(
            $"/api/v1/conversations/{conversationId}/read",
            new { lastMessageId });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        UnreadFor(await GetConversationsAsync(bob), conversationId).Should().Be(0);
    }

    [Fact]
    public async Task Deveria_refletir_a_leitura_no_status_da_mensagem()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var conversationId = await StartDirectAsync(alice, bob.Id);
        var messageId = await SendMessageAsync(alice, conversationId, "oi");

        // Act
        var antes = await GetMessagesAsync(alice, conversationId);
        await bob.Client.PostAsJsonAsync($"/api/v1/conversations/{conversationId}/read", new { lastMessageId = messageId });
        var depois = await GetMessagesAsync(alice, conversationId);

        // Assert — ✓ vira ✓✓ azul.
        antes.EnumerateArray().First().GetProperty("status").GetString().Should().Be("sent");
        depois.EnumerateArray().First().GetProperty("status").GetString().Should().Be("read");
        depois.EnumerateArray().First().GetProperty("readByCount").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task Deveria_mostrar_previa_da_ultima_mensagem_na_listagem()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var conversationId = await StartDirectAsync(alice, bob.Id);
        await SendMessageAsync(alice, conversationId, "mensagem antiga");
        await SendMessageAsync(alice, conversationId, "mensagem mais recente");

        // Act
        var conversations = await GetConversationsAsync(bob);

        // Assert
        var conversation = conversations.EnumerateArray().First(c => c.GetProperty("id").GetGuid() == conversationId);
        conversation.GetProperty("lastMessage").GetProperty("content").GetString().Should().Be("mensagem mais recente");
    }

    [Fact]
    public async Task Nao_deveria_enviar_mensagem_para_conversa_de_terceiros()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var intruso = await CreateUserAsync();
        var conversationId = await StartDirectAsync(alice, bob.Id);

        // Act
        var response = await intruso.Client.PostAsJsonAsync(
            $"/api/v1/conversations/{conversationId}/messages",
            new { content = "invasão", contentType = "text" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Nao_deveria_ler_mensagens_de_conversa_de_terceiros()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var intruso = await CreateUserAsync();
        var conversationId = await StartDirectAsync(alice, bob.Id);
        await SendMessageAsync(alice, conversationId);

        // Act
        var response = await intruso.Client.GetAsync($"/api/v1/conversations/{conversationId}/messages");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Deveria_editar_a_propria_mensagem()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var conversationId = await StartDirectAsync(alice, bob.Id);
        var messageId = await SendMessageAsync(alice, conversationId, "texto original");

        // Act
        var response = await alice.Client.PutAsJsonAsync($"/api/v1/messages/{messageId}", new { content = "texto editado" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var message = (await GetMessagesAsync(alice, conversationId)).EnumerateArray().First();
        message.GetProperty("content").GetString().Should().Be("texto editado");
        message.GetProperty("isEdited").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task Nao_deveria_editar_mensagem_de_outro_usuario()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var conversationId = await StartDirectAsync(alice, bob.Id);
        var messageId = await SendMessageAsync(alice, conversationId);

        // Act
        var response = await bob.Client.PutAsJsonAsync($"/api/v1/messages/{messageId}", new { content = "invadido" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Deveria_apagar_mensagem_de_forma_logica()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var conversationId = await StartDirectAsync(alice, bob.Id);
        var messageId = await SendMessageAsync(alice, conversationId, "vai sumir");

        // Act
        var response = await alice.Client.DeleteAsync($"/api/v1/messages/{messageId}");

        // Assert — a mensagem continua na lista, marcada como apagada.
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var message = (await GetMessagesAsync(bob, conversationId)).EnumerateArray().First();
        message.GetProperty("isDeleted").GetBoolean().Should().BeTrue();
        message.GetProperty("content").GetString().Should().Be("Esta mensagem foi apagada");
    }

    [Fact]
    public async Task Deveria_rejeitar_tipo_de_conteudo_invalido()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var conversationId = await StartDirectAsync(alice, bob.Id);

        // Act
        var response = await alice.Client.PostAsJsonAsync(
            $"/api/v1/conversations/{conversationId}/messages",
            new { content = "x", contentType = "sticker" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
