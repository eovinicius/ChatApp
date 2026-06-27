using ChatApp.Domain.Abstractions;

using FluentAssertions;

namespace ChatApp.UnitTests.Domain.Abstractions;

public class EntityTest
{
    private sealed class TestDomainEvent : IDomainEvent;

    private sealed class TestEntity : Entity
    {
        public TestEntity(Guid id) : base(id) { }
        public void Raise(IDomainEvent e) => RaiseDomainEvent(e);
    }

    [Fact]
    public void RaiseDomainEvent_Deveria_Adicionar_Evento_A_Lista()
    {
        var entity = new TestEntity(Guid.NewGuid());
        var domainEvent = new TestDomainEvent();

        entity.Raise(domainEvent);

        entity.GetDomainEvents().Should().ContainSingle().Which.Should().Be(domainEvent);
    }

    [Fact]
    public void ClearDomainEvents_Deveria_Remover_Todos_Os_Eventos()
    {
        var entity = new TestEntity(Guid.NewGuid());
        entity.Raise(new TestDomainEvent());
        entity.Raise(new TestDomainEvent());

        entity.ClearDomainEvents();

        entity.GetDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void GetDomainEvents_Sem_Eventos_Deve_Retornar_Lista_Vazia()
    {
        var entity = new TestEntity(Guid.NewGuid());

        entity.GetDomainEvents().Should().BeEmpty();
    }
}
