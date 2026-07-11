using BuildingBlocks.Messaging;

using Chat.Application.Abstractions.Authentication;
using Chat.Application.Abstractions.Data;
using Chat.Application.Abstractions.Services;
using Chat.Domain.Entities.Users;
using Chat.Domain.Repositories;

using SharedKernel;

namespace Chat.Application.UseCases.Users.RegisterUser;

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, string?>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHashService _hashService;
    private readonly IAuthenticationService _authenticationService;

    public RegisterUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IHashService hashService, IAuthenticationService authenticationService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _hashService = hashService;
        _authenticationService = authenticationService;
    }

    public async Task<Result<string?>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var userAlreadyExists = await _userRepository.GetByUsername(request.Username, cancellationToken);

        if (userAlreadyExists is not null)
        {
            return Result.Failure<string?>(UserErrors.UsernameAlreadyTaken);
        }

        var passwordHash = _hashService.Hash(request.Password);

        var userResult = User.Create(request.Name, request.Username, passwordHash);
        if (userResult.IsFailure)
            return Result.Failure<string?>(userResult.Error);

        var user = userResult.Value;

        await _userRepository.Add(user, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        var token = _authenticationService.GenerateToken(user);

        return Result.Success(token);
    }
}
