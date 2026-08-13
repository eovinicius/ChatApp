using BuildingBlocks.Authentication;
using BuildingBlocks.Messaging;

using Identity.Domain.Repositories;

using SharedKernel;

namespace Identity.Application.UseCases.Users.SearchUsers;

public class SearchUsersQueryHandler : IQueryHandler<SearchUsersQuery, IReadOnlyList<SearchUsersResponse>>
{
    private const int MaxTake = 50;

    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;

    public SearchUsersQueryHandler(IUserRepository userRepository, IUserContext userContext)
    {
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task<Result<IReadOnlyList<SearchUsersResponse>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var take = Math.Clamp(request.Take, 1, MaxTake);

        var users = await _userRepository.Search(request.Term ?? string.Empty, _userContext.UserId, take, cancellationToken);

        IReadOnlyList<SearchUsersResponse> response = users
            .Select(user => new SearchUsersResponse(user.Id, user.Name, user.Username, user.AvatarUrl))
            .ToList();

        return Result.Success(response);
    }
}
