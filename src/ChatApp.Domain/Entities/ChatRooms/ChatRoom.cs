using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Entities.Users;

namespace ChatApp.Domain.Entities.ChatRooms;

public sealed class ChatRoom : AggregateRoot
{
    public string Name { get; private set; }
    public string Password { get; private set; }
    public Guid OwnerId { get; private set; }
    public bool IsPrivate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int MaxMembers { get; private set; }
    private readonly List<ChatRoomUser> _members;
    public IReadOnlyCollection<ChatRoomUser> Members => _members.AsReadOnly();

    private ChatRoom() { }

    public ChatRoom(string name, Guid ownerId, bool isPrivate, string? password = null)
        : base(Guid.NewGuid())
    {
        Name = name;
        Password = password ?? string.Empty;
        OwnerId = ownerId;
        IsPrivate = isPrivate;
        CreatedAt = DateTime.UtcNow;
        _members = [];
        MaxMembers = 50;
    }

    public static Result<ChatRoom> Create(string name, User user, bool isPrivate, string? password = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<ChatRoom>(ChatRoomErrors.EmptyName);

        if (isPrivate && string.IsNullOrWhiteSpace(password))
            return Result.Failure<ChatRoom>(ChatRoomErrors.PrivateRoomRequiresPassword);

        var room = new ChatRoom(name, user.Id, isPrivate, password);
        _ = room.Join(user);

        return Result.Success(room);
    }

    public static Result<ChatRoom> CreateAnonymous(string name, string guestName)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<ChatRoom>(ChatRoomErrors.EmptyName);

        if (string.IsNullOrWhiteSpace(guestName))
            return Result.Failure<ChatRoom>(ChatRoomErrors.EmptyGuestName);

        var room = new ChatRoom(name, Guid.Empty, isPrivate: false);
        room.JoinAnonymously(guestName);
        return Result.Success(room);
    }

    public Result Join(User user)
    {
        if (IsUserInRoom(user))
            return Result.Failure(ChatRoomErrors.AlreadyMember);

        if (MaxMembersReached())
            return Result.Failure(ChatRoomErrors.RoomFull);

        var chatRoomUser = ChatRoomUser.Create(Id, user.Id);

        _members.Add(chatRoomUser);
        return Result.Success();
    }

    public void JoinAnonymously(string guestName)
    {
        _members.Add(ChatRoomUser.CreateAnonymous(Id, guestName));
    }

    public void Leave(User user)
    {
        var chatRoomUser = Members.FirstOrDefault(x => x.UserId == user.Id);
        if (chatRoomUser is null) return;

        _members.Remove(chatRoomUser);
    }

    public void SetMemberAsAdministrator(User user)
    {
        var chatRoomUser = Members.FirstOrDefault(x => x.UserId == user.Id);
        if (chatRoomUser is null) return;

        chatRoomUser.PromoteToAdmin();
    }

    public bool IsEmpty() => !_members.Any();

    public bool ValidatePassword(string? password) => Password == password;

    public bool IsUserInRoom(User user) => _members.Any(x => x.UserId == user.Id);

    private bool MaxMembersReached() => _members.Count >= MaxMembers;

}