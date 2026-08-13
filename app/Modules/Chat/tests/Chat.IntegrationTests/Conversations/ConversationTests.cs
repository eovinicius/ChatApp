using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Chat.IntegrationTests.Infrastructure;

using FluentAssertions;

namespace Chat.IntegrationTests.Conversations;

public class ConversationTests(ChatAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Deveria_devolver_a_mesma_conversa_ao_abrir_o_1x1_duas_vezes()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();

        // Act
        var first = await StartDirectAsync(alice, bob.Id);
        var second = await StartDirectAsync(alice, bob.Id);

        // Assert — idempotência garantida pelo DirectKey + índice único.
        second.Should().Be(first);
    }

    [Fact]
    public async Task Deveria_devolver_a_mesma_conversa_pelo_outro_lado()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();

        // Act
        var fromAlice = await StartDirectAsync(alice, bob.Id);
        var fromBob = await StartDirectAsync(bob, alice.Id);

        // Assert
        fromBob.Should().Be(fromAlice);
    }

    [Fact]
    public async Task Nao_deveria_abrir_conversa_consigo_mesmo()
    {
        // Arrange
        var alice = await CreateUserAsync();

        // Act
        var response = await alice.Client.PostAsJsonAsync("/api/v1/conversations/direct", new { targetUserId = alice.Id });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deveria_listar_conversas_ordenadas_pela_ultima_atividade()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var carol = await CreateUserAsync();

        var comBob = await StartDirectAsync(alice, bob.Id);
        var comCarol = await StartDirectAsync(alice, carol.Id);

        await SendMessageAsync(alice, comBob, "primeira");
        await SendMessageAsync(alice, comCarol, "segunda");
        await SendMessageAsync(alice, comBob, "terceira");

        // Act
        var conversations = await GetConversationsAsync(alice);

        // Assert — a conversa com atividade mais recente vem primeiro.
        var ids = conversations.EnumerateArray().Select(c => c.GetProperty("id").GetGuid()).ToList();
        ids.Should().HaveCountGreaterThanOrEqualTo(2);
        ids[0].Should().Be(comBob);
        ids[1].Should().Be(comCarol);
    }

    [Fact]
    public async Task Deveria_usar_o_nome_do_outro_usuario_como_titulo_do_1x1()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var conversationId = await StartDirectAsync(alice, bob.Id);
        await SendMessageAsync(alice, conversationId);

        // Act
        var conversations = await GetConversationsAsync(alice);

        // Assert — título hidratado via Identity.Contracts, sem join entre schemas.
        var conversation = conversations.EnumerateArray().First(c => c.GetProperty("id").GetGuid() == conversationId);
        conversation.GetProperty("type").GetString().Should().Be("direct");
        conversation.GetProperty("title").GetString().Should().NotBeNullOrWhiteSpace();
        conversation.GetProperty("otherUserId").GetGuid().Should().Be(bob.Id);
    }

    [Fact]
    public async Task Deveria_criar_grupo_com_dono_e_membros()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();

        // Act
        var groupId = await CreateGroupAsync(alice, "Time", bob.Id);
        var details = await alice.Client.GetFromJsonAsync<JsonElement>($"/api/v1/conversations/{groupId}");

        // Assert
        details.GetProperty("type").GetString().Should().Be("group");
        details.GetProperty("title").GetString().Should().Be("Time");
        details.GetProperty("ownerId").GetGuid().Should().Be(alice.Id);
        details.GetProperty("participants").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task Nao_deveria_permitir_que_membro_comum_adicione_participante()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var carol = await CreateUserAsync();
        var groupId = await CreateGroupAsync(alice, "Time", bob.Id);

        // Act
        var response = await bob.Client.PostAsJsonAsync(
            $"/api/v1/conversations/{groupId}/participants",
            new { userIds = new[] { carol.Id } });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Deveria_permitir_que_admin_adicione_participante()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var carol = await CreateUserAsync();
        var groupId = await CreateGroupAsync(alice, "Time", bob.Id);

        // Act
        var response = await alice.Client.PostAsJsonAsync(
            $"/api/v1/conversations/{groupId}/participants",
            new { userIds = new[] { carol.Id } });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var details = await alice.Client.GetFromJsonAsync<JsonElement>($"/api/v1/conversations/{groupId}");
        details.GetProperty("participants").GetArrayLength().Should().Be(3);
    }

    [Fact]
    public async Task Nao_deveria_permitir_que_o_dono_saia_sem_transferir()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var groupId = await CreateGroupAsync(alice, "Time", bob.Id);

        // Act
        var response = await alice.Client.DeleteAsync($"/api/v1/conversations/{groupId}/participants/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Membro_removido_deveria_perder_acesso_a_conversa()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var groupId = await CreateGroupAsync(alice, "Time", bob.Id);

        // Act
        var removal = await alice.Client.DeleteAsync($"/api/v1/conversations/{groupId}/participants/{bob.Id}");
        var access = await bob.Client.GetAsync($"/api/v1/conversations/{groupId}");

        // Assert
        removal.StatusCode.Should().Be(HttpStatusCode.NoContent);
        access.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Nao_deveria_renomear_conversa_direta()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var conversationId = await StartDirectAsync(alice, bob.Id);

        // Act
        var response = await alice.Client.PatchAsJsonAsync(
            $"/api/v1/conversations/{conversationId}",
            new { name = "Nome novo" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
