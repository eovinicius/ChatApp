using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Repositories;

namespace ChatApp.Application.UseCases.Rooms.CreateRoomAsAnonymous;

public sealed class CreateRoomAsAnonymousCommandHandler : ICommandHandler<CreateRoomAsAnonymousCommand, Guid>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatHub _chatHub;

    public CreateRoomAsAnonymousCommandHandler(IChatRoomRepository chatRoomRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, IChatHub chatHub)
    {
        _chatRoomRepository = chatRoomRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _chatHub = chatHub;
    }

    public Task<Result<Guid>> Handle(CreateRoomAsAnonymousCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}