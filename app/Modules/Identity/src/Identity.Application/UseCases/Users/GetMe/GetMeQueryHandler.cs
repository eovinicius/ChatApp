using BuildingBlocks.Authentication;
using BuildingBlocks.Messaging;

using Identity.Domain.Entities.Users;
using Identity.Domain.Repositories;

using SharedKernel;

namespace Identity.Application.UseCases.Users.GetMe;

public class GetMeQueryHandler : IQueryHandler<GetMeQuery, GetMeResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;

    public GetMeQueryHandler(IUserRepository userRepository, IUserContext userContext)
    {
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetMeResponse>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetById(_userContext.UserId, cancellationToken);

        if (user is null)
        {
            return UserErrors.NotFound;
        }

        return new GetMeResponse(user.Id, user.Name, user.Username, user.AvatarUrl, user.LastSeenAt);
    }
}
