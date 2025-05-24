
using System.Net;

using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Entities.Users;
using ChatApp.Domain.Repositories;

namespace ChatApp.Application.UseCases.Users.RegisterUser;

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, string?>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHashService _hashService;
    private readonly IAuthenticationService _authenticationService;

    public RegisterUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IHashService hashService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _hashService = hashService;
    }

    public async Task<Result<string?>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var userAlreadyExists = await _userRepository.GetByUsername(request.Username, cancellationToken);

        if (userAlreadyExists is not null)
        {
            return Result.Failure<string?>(Error.NullValue);
        }

        var passwordHash = _hashService.Hash(request.Password);

        var user = new User(request.Name, request.Username, passwordHash);

        await _userRepository.Add(user, cancellationToken);

        var token = _authenticationService.GenerateToken(user);

        return Result.Success(token);
    }
}
