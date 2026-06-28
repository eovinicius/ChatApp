using System.IdentityModel.Tokens.Jwt;

using ChatApp.Domain.Entities.Users;
using ChatApp.Infrastructure.Authentication;

using FluentAssertions;

using Microsoft.Extensions.Configuration;

namespace ChatApp.UnitTests.Infrastructure.Authentication;

public class AuthenticationServiceTest
{
    private static AuthenticationService CreateService(string? secretKey = "chave-super-secreta-de-teste-com-32chars!!", string? issuer = "ChatApp", string? audience = "ChatApp")
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "JwtSettings:SecretKey", secretKey },
                { "JwtSettings:Issuer", issuer },
                { "JwtSettings:Audience", audience }
            })
            .Build();

        return new AuthenticationService(config);
    }

    [Fact]
    public void GenerateToken_Deveria_Incluir_Claim_NameIdentifier_Com_UserId()
    {
        var user = User.Create("João Silva", "joao", "hash").Value;
        var service = CreateService();

        var token = service.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        // ReadJwtToken usa nomes curtos JWT: "nameid" em vez da URI completa do ClaimTypes.NameIdentifier
        jwt.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == user.Id.ToString());
    }

    [Fact]
    public void GenerateToken_Deveria_Incluir_Claim_Name_Com_Username()
    {
        var user = User.Create("João Silva", "joao", "hash").Value;
        var service = CreateService();

        var token = service.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        // ReadJwtToken usa nomes curtos JWT: "unique_name" em vez de ClaimTypes.Name
        jwt.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == user.Username);
    }

    [Fact]
    public void GenerateToken_Deveria_Expirar_Em_Aproximadamente_15_Minutos()
    {
        var user = User.Create("João Silva", "joao", "hash").Value;
        var service = CreateService();

        var token = service.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void GenerateToken_Deveria_Lancar_Excecao_Quando_SecretKey_Nao_Configurada()
    {
        var user = User.Create("João Silva", "joao", "hash").Value;
        var service = CreateService(secretKey: null);

        var act = () => service.GenerateToken(user);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GenerateToken_Deveria_Lancar_Excecao_Quando_Issuer_Nao_Configurado()
    {
        var user = User.Create("João Silva", "joao", "hash").Value;
        var service = CreateService(issuer: null);

        var act = () => service.GenerateToken(user);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GenerateToken_Deveria_Lancar_Excecao_Quando_Audience_Nao_Configurada()
    {
        var user = User.Create("João Silva", "joao", "hash").Value;
        var service = CreateService(audience: null);

        var act = () => service.GenerateToken(user);

        act.Should().Throw<InvalidOperationException>();
    }
}
