using System.Net.Http.Headers;

using ChatApp.Domain.Entities.Users;

namespace ChatApp.Domain.Entities.ChatRooms;

public sealed class ChatRoom
{
    public Guid Id { get; private set; }
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
    {
        Id = Guid.NewGuid();
        Name = name;
        Password = password ?? string.Empty;
        OwnerId = ownerId;
        IsPrivate = isPrivate;
        CreatedAt = DateTime.UtcNow;
        _members = [];
        MaxMembers = 50;
    }

    public static ChatRoom Create(string name, User user, bool isPrivate, string? password = null)
    {
        var room = new ChatRoom(name, user.Id, isPrivate, password);
        room.Join(user);

        return room;
    }

    public void Join(User user)
    {
        if (IsUserInRoom(user.Id))
            return;

        if (MaxMembersReached())
            return;

        var chatRoomUser = ChatRoomUser.Create(Id, user.Id);

        _members.Add(chatRoomUser);
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

    public bool IsUserInRoom(Guid userId) => _members.Any(x => x.UserId == userId);

    private bool MaxMembersReached() => _members.Count >= MaxMembers;

}