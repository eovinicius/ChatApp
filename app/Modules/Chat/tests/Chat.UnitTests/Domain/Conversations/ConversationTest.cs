using Chat.Domain.Conversations;

using FluentAssertions;

namespace Chat.UnitTests.Domain.Conversations;

public class ConversationTest
{
    private static readonly DateTime Now = new(2026, 8, 12, 10, 0, 0, DateTimeKind.Utc);

    private static Conversation Group(out Guid ownerId, out Guid memberId)
    {
        ownerId = Guid.NewGuid();
        memberId = Guid.NewGuid();

        return Conversation.CreateGroup("Time", ownerId, [memberId], Now).Value;
    }

    // ---------- Direct ----------

    [Fact]
    public void Deveria_gerar_a_mesma_chave_para_o_par_invertido()
    {
        // Arrange
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        // Act
        var keyAb = Conversation.BuildDirectKey(userA, userB);
        var keyBa = Conversation.BuildDirectKey(userB, userA);

        // Assert — é o que torna a abertura de conversa 1x1 idempotente.
        keyAb.Should().Be(keyBa);
    }

    [Fact]
    public void Deveria_criar_conversa_direta_com_dois_participantes()
    {
        // Arrange
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        // Act
        var result = Conversation.CreateDirect(userA, userB, Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(ConversationType.Direct);
        result.Value.Participants.Should().HaveCount(2);
        result.Value.DirectKey.Should().Be(Conversation.BuildDirectKey(userA, userB));
        result.Value.Name.Should().BeNull();
        result.Value.OwnerId.Should().BeNull();
    }

    [Fact]
    public void Nao_deveria_criar_conversa_direta_consigo_mesmo()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = Conversation.CreateDirect(userId, userId, Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.DirectWithSelf");
    }

    [Fact]
    public void Nao_deveria_alterar_participantes_de_conversa_direta()
    {
        // Arrange
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        var conversation = Conversation.CreateDirect(userA, userB, Now).Value;

        // Act
        var add = conversation.AddParticipant(userA, Guid.NewGuid(), Now);
        var remove = conversation.RemoveParticipant(userA, userB, Now);
        var rename = conversation.Rename(userA, "Novo nome");

        // Assert
        add.Error.Code.Should().Be("Conversation.DirectIsImmutable");
        remove.Error.Code.Should().Be("Conversation.DirectIsImmutable");
        rename.Error.Code.Should().Be("Conversation.DirectIsImmutable");
    }

    // ---------- Group ----------

    [Fact]
    public void Deveria_criar_grupo_com_dono_e_membros()
    {
        // Act
        var conversation = Group(out var ownerId, out var memberId);

        // Assert
        conversation.Type.Should().Be(ConversationType.Group);
        conversation.OwnerId.Should().Be(ownerId);
        conversation.DirectKey.Should().BeNull();
        conversation.FindParticipant(ownerId)!.Role.Should().Be(ParticipantRole.Owner);
        conversation.FindParticipant(memberId)!.Role.Should().Be(ParticipantRole.Member);
    }

    [Fact]
    public void Nao_deveria_criar_grupo_sem_nome()
    {
        // Act
        var result = Conversation.CreateGroup("   ", Guid.NewGuid(), [], Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.EmptyGroupName");
    }

    [Fact]
    public void Nao_deveria_permitir_que_membro_comum_adicione_participante()
    {
        // Arrange
        var conversation = Group(out _, out var memberId);

        // Act
        var result = conversation.AddParticipant(memberId, Guid.NewGuid(), Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.RequiresAdmin");
    }

    [Fact]
    public void Deveria_permitir_que_admin_adicione_participante()
    {
        // Arrange
        var conversation = Group(out var ownerId, out var memberId);
        conversation.PromoteToAdmin(ownerId, memberId);
        var novo = Guid.NewGuid();

        // Act
        var result = conversation.AddParticipant(memberId, novo, Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        conversation.IsActiveParticipant(novo).Should().BeTrue();
    }

    [Fact]
    public void Nao_deveria_permitir_que_o_dono_saia_sem_transferir()
    {
        // Arrange
        var conversation = Group(out var ownerId, out _);

        // Act
        var result = conversation.RemoveParticipant(ownerId, ownerId, Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.OwnerMustTransferFirst");
    }

    [Fact]
    public void Deveria_permitir_que_o_dono_saia_apos_transferir()
    {
        // Arrange
        var conversation = Group(out var ownerId, out var memberId);

        // Act
        var transfer = conversation.TransferOwnership(ownerId, memberId);
        var leave = conversation.RemoveParticipant(ownerId, ownerId, Now);

        // Assert
        transfer.IsSuccess.Should().BeTrue();
        leave.IsSuccess.Should().BeTrue();
        conversation.OwnerId.Should().Be(memberId);
        conversation.IsActiveParticipant(ownerId).Should().BeFalse();
    }

    [Fact]
    public void Deveria_permitir_que_membro_saia_sozinho()
    {
        // Arrange
        var conversation = Group(out _, out var memberId);

        // Act
        var result = conversation.RemoveParticipant(memberId, memberId, Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        conversation.IsActiveParticipant(memberId).Should().BeFalse();
        conversation.ActiveParticipantIds().Should().NotContain(memberId);
    }

    [Fact]
    public void Deveria_reativar_participante_que_havia_saido()
    {
        // Arrange
        var conversation = Group(out var ownerId, out var memberId);
        conversation.RemoveParticipant(memberId, memberId, Now);

        // Act
        var result = conversation.AddParticipant(ownerId, memberId, Now.AddMinutes(5));

        // Assert — reaproveita a linha, sem duplicar o participante.
        result.IsSuccess.Should().BeTrue();
        conversation.Participants.Count(p => p.UserId == memberId).Should().Be(1);
        conversation.IsActiveParticipant(memberId).Should().BeTrue();
    }

    [Fact]
    public void Nao_deveria_adicionar_participante_ja_ativo()
    {
        // Arrange
        var conversation = Group(out var ownerId, out var memberId);

        // Act
        var result = conversation.AddParticipant(ownerId, memberId, Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.AlreadyParticipant");
    }

    [Fact]
    public void Nao_deveria_permitir_postar_para_quem_nao_participa()
    {
        // Arrange
        var conversation = Group(out _, out _);

        // Act
        var result = conversation.EnsureCanPost(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.NotParticipant");
    }

    // ---------- Atividade e leitura ----------

    [Fact]
    public void Deveria_avancar_a_ultima_atividade_apenas_para_frente()
    {
        // Arrange
        var conversation = Group(out _, out _);

        // Act
        conversation.RegisterActivity(Now.AddMinutes(10));
        conversation.RegisterActivity(Now.AddMinutes(-10));

        // Assert
        conversation.LastActivityAt.Should().Be(Now.AddMinutes(10));
    }

    [Fact]
    public void Nao_deveria_marcar_leitura_de_quem_nao_participa()
    {
        // Arrange
        var conversation = Group(out _, out _);

        // Act
        var result = conversation.MarkRead(Guid.NewGuid(), Guid.NewGuid(), Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.NotParticipant");
    }
}
