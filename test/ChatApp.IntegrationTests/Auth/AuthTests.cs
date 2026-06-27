using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using ChatApp.IntegrationTests.Infrastructure;

using FluentAssertions;

namespace ChatApp.IntegrationTests.Auth;

public class AuthTests(ChatAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Register_Com_Dados_Validos_Deve_Retornar_200_Com_Token()
    {
        var username = $"user_{Guid.NewGuid():N}";

        var response = await Client.PostAsJsonAsync("/api/user/register", new
        {
            name = "João Silva",
            username,
            password = "Senha@123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_Com_Username_Duplicado_Deve_Retornar_400()
    {
        var username = $"dup_{Guid.NewGuid():N}";

        await Client.PostAsJsonAsync("/api/user/register", new
        {
            name = "Primeiro",
            username,
            password = "Senha@123"
        });

        var response = await Client.PostAsJsonAsync("/api/user/register", new
        {
            name = "Segundo",
            username,
            password = "Senha@123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_Com_Credenciais_Validas_Deve_Retornar_200_Com_Token()
    {
        var username = $"login_{Guid.NewGuid():N}";
        await Client.PostAsJsonAsync("/api/user/register", new
        {
            name = "Usuário Login",
            username,
            password = "Senha@123"
        });

        var response = await Client.PostAsJsonAsync("/api/user/login", new
        {
            username,
            password = "Senha@123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_Com_Senha_Errada_Deve_Retornar_400()
    {
        var username = $"wrongpwd_{Guid.NewGuid():N}";
        await Client.PostAsJsonAsync("/api/user/register", new
        {
            name = "Usuário",
            username,
            password = "Senha@123"
        });

        var response = await Client.PostAsJsonAsync("/api/user/login", new
        {
            username,
            password = "SenhaErrada"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_Com_Usuario_Inexistente_Deve_Retornar_400()
    {
        var response = await Client.PostAsJsonAsync("/api/user/login", new
        {
            username = $"naoexiste_{Guid.NewGuid():N}",
            password = "Senha@123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
