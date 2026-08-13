namespace Identity.Application.Abstractions;

// Tipo próprio (e não um IUnitOfWork compartilhado) de propósito: cada módulo tem o seu
// DbContext, e uma interface única faria as duas registrações de DI colidirem.
public interface IIdentityUnitOfWork
{
    Task Commit(CancellationToken cancellationToken = default);
}
