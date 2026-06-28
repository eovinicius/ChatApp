using ChatApp.Infrastructure.Services;

using FluentAssertions;

namespace ChatApp.UnitTests.Infrastructure.Services;

public class HashServiceTest
{
    private readonly HashService _service = new();

    [Fact]
    public void Hash_Deveria_Retornar_Valor_Diferente_Do_Input()
    {
        var hash = _service.Hash("minha-senha");

        hash.Should().NotBe("minha-senha");
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Compare_Deveria_Aceitar_Senha_Correta()
    {
        var hash = _service.Hash("senha-certa");

        _service.Compare("senha-certa", hash).Should().BeTrue();
    }

    [Fact]
    public void Compare_Deveria_Rejeitar_Senha_Errada()
    {
        var hash = _service.Hash("senha-certa");

        _service.Compare("senha-errada", hash).Should().BeFalse();
    }

    [Fact]
    public void Hash_Deveria_Gerar_Valores_Diferentes_Para_Mesma_Senha()
    {
        var hash1 = _service.Hash("mesma-senha");
        var hash2 = _service.Hash("mesma-senha");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Compare_Deveria_Aceitar_Hash_Gerado_Por_Outra_Chamada()
    {
        var hash = _service.Hash("minha-senha");

        _service.Compare("minha-senha", hash).Should().BeTrue();
    }
}
