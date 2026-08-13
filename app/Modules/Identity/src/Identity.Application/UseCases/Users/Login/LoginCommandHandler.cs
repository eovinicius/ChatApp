using BuildingBlocks.Messaging;

using Identity.Application.Abstractions;
using Identity.Domain.Entities.Users;
using Identity.Domain.Repositories;

using SharedKernel;

namespace Identity.Application.UseCases.Users.Login;

public class LoginCommandHandler : ICommandHandler<LoginCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthenticationService _authenticationService;
    private readonly IHashService _hashService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IAuthenticationService authenticationService,
        IHashService hashService)
    {
        _userRepository = userRepository;
        _authenticationService = authenticationService;
        _hashService = hashService;
    }

    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsername(request.Username, cancellationToken);

        if (user is null)
        {
            return UserErrors.InvalidCredentials;
        }

        if (!_hashService.Compare(request.Password, user.Password))
        {
            return UserErrors.InvalidCredentials;
        }

        var token = _authenticationService.GenerateToken(user);

        return token;
    }
}
