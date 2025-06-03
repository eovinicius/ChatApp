using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Repositories;

namespace ChatApp.Application.UseCases.Users.Login;

public class LoginCommandHandler : ICommandHandler<LoginCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthenticationService _authenticationService;
    private readonly IHashService _hashService;

    public LoginCommandHandler(IUserRepository userRepository, IAuthenticationService authenticationService, IHashService hashService)
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
            return Result.Failure<string>(Error.NullValue);
        }

        if (!_hashService.Compare(request.Password, user.Password))
        {
            return Result.Failure<string>(Error.NullValue);
        }

        var token = _authenticationService.GenerateToken(user);

        return token;
    }
}