using MediatR;

using SharedKernel;

namespace BuildingBlocks.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}