using ChatApp.Domain.Abstractions;

using MediatR;

namespace ChatApp.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}