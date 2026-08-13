using BuildingBlocks.Messaging;

using Identity.Application.Abstractions;
using Identity.Domain.Entities.Users;
using Identity.Domain.Repositories;

using SharedKernel;

namespace Identity.Application.UseCases.Users.RegisterUser;

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, string?>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly IHashService _hashService;
    private readonly IAuthenticationService _authenticationService;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IIdentityUnitOfWork unitOfWork,
        IHashService hashService,
        IAuthenticationService authenticationService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _hashService = hashService;
        _authenticationService = authenticationService;
    }

    public async Task<Result<string?>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByUsername(request.Username, cancellationToken))
        {
            return UserErrors.UsernameAlreadyTaken;
        }

        var passwordHash = _hashService.Hash(request.Password);

        var userResult = User.Create(request.Name, request.Username, passwordHash);
        if (userResult.IsFailure)
            return userResult.Error;

        var user = userResult.Value;

        await _userRepository.Add(user, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        var token = _authenticationService.GenerateToken(user);

        return token;
    }
}
