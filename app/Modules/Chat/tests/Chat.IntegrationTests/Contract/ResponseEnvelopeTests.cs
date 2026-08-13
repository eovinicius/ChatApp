using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Chat.IntegrationTests.Infrastructure;

using FluentAssertions;

namespace Chat.IntegrationTests.Contract;

// Blinda o contrato { data, error, meta }: é o que o cliente assume em todo
// endpoint, inclusive nos erros que o pipeline HTTP produz sozinho.
public class ResponseEnvelopeTests(ChatAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Erro_de_dominio_deveria_vir_no_envelope_com_codigo_e_tipo()
    {
        // Arrange
        var alice = await CreateUserAsync();

        // Act
        var response = await alice.Client.GetAsync($"/api/v1/conversations/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var error = await ErrorAsync(response);
        error.GetProperty("code").GetString().Should().Be("Conversation.NotFound");
        error.GetProperty("type").GetString().Should().Be("NotFound");
        error.GetProperty("message").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Erro_de_validacao_deveria_listar_os_campos_em_details()
    {
        // Act — username abaixo do mínimo, barrado pelo ValidationFilter.
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            username = "ab",
            password = "Senha@123"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await ErrorAsync(response);
        error.GetProperty("code").GetString().Should().Be("Validation.Failed");
        error.GetProperty("type").GetString().Should().Be("Validation");

        var fields = error.GetProperty("details")
            .EnumerateArray()
            .Select(detail => detail.GetProperty("field").GetString())
            .ToList();

        fields.Should().Contain("Username");
    }

    [Fact]
    public async Task Requisicao_sem_token_deveria_devolver_envelope_e_nao_corpo_vazio()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/conversations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var error = await ErrorAsync(response);
        error.GetProperty("code").GetString().Should().Be("Auth.Unauthorized");
    }

    [Fact]
    public async Task Rota_inexistente_deveria_devolver_envelope()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/rota-que-nao-existe");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var error = await ErrorAsync(response);
        error.GetProperty("code").GetString().Should().Be("Http.NotFound");
    }

    [Fact]
    public async Task Comando_sem_retorno_deveria_continuar_204_sem_corpo()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var conversationId = await StartDirectAsync(alice, bob.Id);
        var messageId = await SendMessageAsync(alice, conversationId);

        // Act
        var response = await bob.Client.PostAsJsonAsync(
            $"/api/v1/conversations/{conversationId}/read",
            new { lastMessageId = messageId });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await response.Content.ReadAsStringAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Criacao_deveria_devolver_201_com_envelope()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();

        // Act
        var response = await alice.Client.PostAsJsonAsync(
            "/api/v1/conversations/direct",
            new { targetUserId = bob.Id });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await DataAsync(response)).GetProperty("id").GetGuid().Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Lista_paginada_deveria_expor_o_cursor_em_meta()
    {
        // Arrange — duas conversas, uma página de tamanho 1.
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var carol = await CreateUserAsync();

        var comBob = await StartDirectAsync(alice, bob.Id);
        var comCarol = await StartDirectAsync(alice, carol.Id);

        await SendMessageAsync(alice, comCarol, "primeira");
        await SendMessageAsync(alice, comBob, "segunda");

        // Act
        var first = await EnvelopeAsync(await alice.Client.GetAsync("/api/v1/conversations?take=1"));

        var firstIds = first.GetProperty("data").EnumerateArray()
            .Select(item => item.GetProperty("id").GetGuid()).ToList();

        var pagination = first.GetProperty("meta").GetProperty("pagination");
        var cursor = pagination.GetProperty("nextCursor").GetDateTime();

        var second = await alice.Client.GetAsync($"/api/v1/conversations?take=1&before={cursor:O}");
        var secondIds = (await DataAsync(second)).EnumerateArray()
            .Select(item => item.GetProperty("id").GetGuid()).ToList();

        // Assert — o cursor avança sem repetir nem pular item.
        firstIds.Should().Equal(comBob);
        pagination.GetProperty("hasMore").GetBoolean().Should().BeTrue();
        secondIds.Should().Equal(comCarol);
    }

    [Fact]
    public async Task Lista_nao_paginada_deveria_omitir_meta()
    {
        // Arrange
        var alice = await CreateUserAsync();

        // Act
        var response = await alice.Client.GetAsync("/api/v1/users?search=zzz-inexistente");
        var envelope = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Assert
        envelope.TryGetProperty("meta", out _).Should().BeFalse();
    }
}
