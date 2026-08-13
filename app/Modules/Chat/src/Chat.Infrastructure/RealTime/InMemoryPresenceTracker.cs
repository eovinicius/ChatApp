using System.Collections.Concurrent;

using Chat.Application.Abstractions.RealTime;

namespace Chat.Infrastructure.RealTime;

// ATENÇÃO: estado em memória, portanto correto apenas com uma instância da API.
// Escalar horizontalmente exige um backplane (AddStackExchangeRedis) — ver docs/architecture.md.
public sealed class InMemoryPresenceTracker : IPresenceTracker
{
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _connections = new();

    public Task<bool> Connect(Guid userId, string connectionId)
    {
        var isFirstConnection = false;

        _connections.AddOrUpdate(
            userId,
            _ =>
            {
                isFirstConnection = true;
                return [connectionId];
            },
            (_, existing) =>
            {
                lock (existing)
                {
                    existing.Add(connectionId);
                }

                return existing;
            });

        return Task.FromResult(isFirstConnection);
    }

    public Task<bool> Disconnect(Guid userId, string connectionId)
    {
        if (!_connections.TryGetValue(userId, out var existing))
            return Task.FromResult(false);

        bool isLastConnection;

        lock (existing)
        {
            existing.Remove(connectionId);
            isLastConnection = existing.Count == 0;
        }

        if (isLastConnection)
            _connections.TryRemove(userId, out _);

        return Task.FromResult(isLastConnection);
    }

    public Task<bool> IsOnline(Guid userId)
        => Task.FromResult(_connections.TryGetValue(userId, out var connections) && connections.Count > 0);

    public Task<IReadOnlyList<Guid>> FilterOnline(IReadOnlyCollection<Guid> userIds)
    {
        IReadOnlyList<Guid> online = userIds
            .Where(id => _connections.TryGetValue(id, out var connections) && connections.Count > 0)
            .ToList();

        return Task.FromResult(online);
    }
}
