using SharedKernel;

using MediatR;

namespace BuildingBlocks.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}