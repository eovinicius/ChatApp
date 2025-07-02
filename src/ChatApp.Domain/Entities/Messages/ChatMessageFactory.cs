namespace ChatApp.Domain.Entities.Messages;

public class ChatMessageFactory
{
    public static ChatMessage CreateTextMessage(Guid chatRoomId, Guid senderId, string text, DateTime sentAt)
    {
        var content = new TextContent(text);
        return new ChatMessage(chatRoomId, senderId, content, sentAt);
    }

    public static ChatMessage CreateImageMessage(Guid chatRoomId, Guid senderId, string imageUrl, DateTime sentAt)
    {
        var content = new ImageContent(imageUrl);
        return new ChatMessage(chatRoomId, senderId, content, sentAt);
    }

    public static ChatMessage CreateAudioMessage(Guid chatRoomId, Guid senderId, string audioUrl, DateTime sentAt)
    {
        var content = new AudioContent(audioUrl);
        return new ChatMessage(chatRoomId, senderId, content, sentAt);
    }

    public static ChatMessage CreateVideoMessage(Guid chatRoomId, Guid senderId, string videoUrl, DateTime sentAt)
    {
        var content = new VideoContent(videoUrl);
        return new ChatMessage(chatRoomId, senderId, content, sentAt);
    }
}