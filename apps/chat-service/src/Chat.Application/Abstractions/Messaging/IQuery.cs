using Chat.Domain.Abstractions;

using MediatR;

namespace Chat.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}