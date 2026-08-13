using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Chat.IntegrationTests.Infrastructure;

using FluentAssertions;

namespace Chat.IntegrationTests.Auth;

public class AuthTests(ChatAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Deveria_registrar_usuario_e_devolver_token()
    {
        // Arrange
        var username = $"reg_{Guid.NewGuid():N}"[..20];

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            name = "Novo Usuário",
            username,
            password = "Senha@123"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("token").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Nao_deveria_registrar_username_duplicado()
    {
        // Arrange
        var username = $"dup_{Guid.NewGuid():N}"[..20];
        var payload = new { name = "Usuário", username, password = "Senha@123" };
        (await Client.PostAsJsonAsync("/api/v1/auth/register", payload)).EnsureSuccessStatusCode();

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Nao_deveria_autenticar_com_senha_incorreta()
    {
        // Arrange
        var user = await CreateUserAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            username = user.Username,
            password = "SenhaErrada@1"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Deveria_exigir_autenticacao_nas_conversas()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/conversations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Deveria_encontrar_usuario_na_busca()
    {
        // Arrange
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();

        // Act
        var found = await alice.Client.GetFromJsonAsync<JsonElement>($"/api/v1/users?search={bob.Username}");

        // Assert
        found.EnumerateArray().Select(u => u.GetProperty("id").GetGuid()).Should().Contain(bob.Id);
    }

    [Fact]
    public async Task Nao_deveria_retornar_o_proprio_usuario_na_busca()
    {
        // Arrange
        var alice = await CreateUserAsync();

        // Act
        var found = await alice.Client.GetFromJsonAsync<JsonElement>($"/api/v1/users?search={alice.Username}");

        // Assert
        found.EnumerateArray().Should().BeEmpty();
    }
}
