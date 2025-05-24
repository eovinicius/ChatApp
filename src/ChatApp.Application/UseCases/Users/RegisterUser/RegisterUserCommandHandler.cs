
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Repositories;

namespace ChatApp.Application.UseCases.Users.RegisterUser;

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommnad, string?>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string?>> Handle(RegisterUserCommnad request, CancellationToken cancellationToken)
    {
        var userAlreadyExists = await _userRepository.GetByUsername(request.Username);

        if (userAlreadyExists is not null)
        {
            return Result.Failure<string?>(Error.NullValue);
        }

        // todo: tem que terminar

        return string.Empty;
    }
}
