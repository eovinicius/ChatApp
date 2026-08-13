using System.Net.Http.Json;

using Chat.Application.Abstractions.RealTime;
using Chat.Infrastructure.RealTime;
using Chat.IntegrationTests.Infrastructure;

using FluentAssertions;

using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chat.IntegrationTests.RealTime;

// Estes testes usam um HubConnection real contra o TestServer, forçando WebSocket.
// Isso importa: com SkipNegotiation + transporte WebSocket, o único caminho de
// autenticação é o "access_token" na query string — exatamente o que o handler
// OnMessageReceived do Identity trata, e que o header Authorization mascararia.
public class RealTimeTests(ChatAppFactory factory) : IntegrationTestBase(factory), IAsyncDisposable
{
    private static readonly TimeSpan EventTimeout = TimeSpan.FromSeconds(10);

    private readonly List<HubConnection> _connections = [];

    private async Task<HubConnection> ConnectAsync(string token)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(Factory.Server.BaseAddress, "chatHub"), options =>
            {
                options.Transports = HttpTransportType.WebSockets;
                options.SkipNegotiation = true;

                // O TestServer não fala TCP: o WebSocket precisa ser criado por ele.
                options.WebSocketFactory = async (context, cancellationToken) =>
                {
                    var webSocketClient = Factory.Server.CreateWebSocketClient();

                    var builder = new UriBuilder(context.Uri) { Scheme = "http" };

                    // Deliberadamente pela query string, e não pelo header Authorization:
                    // é o que um navegador faz (JS não seta header em WebSocket) e é o
                    // único caminho que o handler OnMessageReceived cobre.
                    if (!string.IsNullOrEmpty(token))
                    {
                        var existing = builder.Query.TrimStart('?');
                        var separator = string.IsNullOrEmpty(existing) ? string.Empty : "&";
                        builder.Query = $"{existing}{separator}access_token={Uri.EscapeDataString(token)}";
                    }

                    return await webSocketClient.ConnectAsync(builder.Uri, cancellationToken);
                };
            })
            .Build();

        await connection.StartAsync();

        _connections.Add(connection);

        return connection;
    }

    private static (Task<T> Event, IDisposable Subscription) Expect<T>(HubConnection connection, string method)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        var subscription = connection.On<T>(method, payload => tcs.TrySetResult(payload));

        return (tcs.Task, subscription);
    }

    private static (Task<(T1, T2, T3)> Event, IDisposable Subscription) Expect<T1, T2, T3>(HubConnection connection, string method)
    {
        var tcs = new TaskCompletionSource<(T1, T2, T3)>(TaskCreationOptions.RunContinuationsAsynchronously);
        var subscription = connection.On<T1, T2, T3>(method, (a, b, c) => tcs.TrySetResult((a, b, c)));

        return (tcs.Task, subscription);
    }

    private static async Task<T> Await<T>(Task<T> pending)
    {
        var completed = await Task.WhenAny(pending, Task.Delay(EventTimeout));

        completed.Should().BeSameAs(pending, "o evento de tempo real deveria ter chegado dentro do timeout");

        return await pending;
    }

    // Alice e Bob com uma conversa em comum — pré-requisito para presença e digitando.
    private async Task<(TestUser Alice, TestUser Bob, Guid ConversationId)> PairAsync()
    {
        var alice = await CreateUserAsync();
        var bob = await CreateUserAsync();
        var conversationId = await StartDirectAsync(alice, bob.Id);

        return (alice, bob, conversationId);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var connection in _connections)
            await connection.DisposeAsync();

        GC.SuppressFinalize(this);
    }

    // ---------- Autenticação ----------

    [Fact]
    public async Task Deveria_conectar_no_hub_com_token_na_query_string()
    {
        // Arrange
        var user = await CreateUserAsync();

        // Act
        var connection = await ConnectAsync(user.Token);

        // Assert
        connection.State.Should().Be(HubConnectionState.Connected);
    }

    [Fact]
    public async Task Nao_deveria_conectar_no_hub_sem_token()
    {
        // Act
        var connect = async () => await ConnectAsync(string.Empty);

        // Assert — o hub é [Authorize]; sem token o handshake não passa.
        await connect.Should().ThrowAsync<Exception>();
    }

    // ---------- Mensagens ----------

    [Fact]
    public async Task Deveria_entregar_mensagem_sem_o_cliente_entrar_em_nenhum_grupo()
    {
        // Arrange
        var (alice, bob, conversationId) = await PairAsync();
        var bobConnection = await ConnectAsync(bob.Token);
        var (received, _) = Expect<MessageNotification>(bobConnection, nameof(IChatNotifier.MessageReceived));

        // Act — envio por HTTP, nunca pelo hub.
        await SendMessageAsync(alice, conversationId, "olá tempo real");

        // Assert — entrega por Clients.Users, sem JoinGroup em lugar nenhum.
        var notification = await Await(received);
        notification.ConversationId.Should().Be(conversationId);
        notification.SenderId.Should().Be(alice.Id);
        notification.Content.Should().Be("olá tempo real");
        notification.ContentType.Should().Be("text");
    }

    [Fact]
    public async Task Deveria_notificar_edicao_de_mensagem()
    {
        // Arrange
        var (alice, bob, conversationId) = await PairAsync();
        var bobConnection = await ConnectAsync(bob.Token);
        var messageId = await SendMessageAsync(alice, conversationId, "texto original");
        var (edited, _) = Expect<MessageNotification>(bobConnection, nameof(IChatNotifier.MessageEdited));

        // Act
        var response = await alice.Client.PutAsJsonAsync($"/api/v1/messages/{messageId}", new { content = "texto editado" });
        response.EnsureSuccessStatusCode();

        // Assert
        var notification = await Await(edited);
        notification.Id.Should().Be(messageId);
        notification.Content.Should().Be("texto editado");
        notification.EditedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Deveria_notificar_exclusao_de_mensagem()
    {
        // Arrange
        var (alice, bob, conversationId) = await PairAsync();
        var bobConnection = await ConnectAsync(bob.Token);
        var messageId = await SendMessageAsync(alice, conversationId, "vai sumir");

        var tcs = new TaskCompletionSource<(Guid, Guid)>(TaskCreationOptions.RunContinuationsAsynchronously);
        bobConnection.On<Guid, Guid>(nameof(IChatNotifier.MessageDeleted), (c, m) => tcs.TrySetResult((c, m)));

        // Act
        var response = await alice.Client.DeleteAsync($"/api/v1/messages/{messageId}");
        response.EnsureSuccessStatusCode();

        // Assert
        var (notifiedConversationId, notifiedMessageId) = await Await(tcs.Task);
        notifiedConversationId.Should().Be(conversationId);
        notifiedMessageId.Should().Be(messageId);
    }

    // ---------- Recibos de leitura ----------

    [Fact]
    public async Task Deveria_notificar_o_remetente_quando_a_mensagem_for_lida()
    {
        // Arrange
        var (alice, bob, conversationId) = await PairAsync();
        var aliceConnection = await ConnectAsync(alice.Token);
        var messageId = await SendMessageAsync(alice, conversationId, "leia isto");
        var (read, _) = Expect<ReadReceiptNotification>(aliceConnection, nameof(IChatNotifier.MessagesRead));

        // Act
        var response = await bob.Client.PostAsJsonAsync(
            $"/api/v1/conversations/{conversationId}/read",
            new { lastMessageId = messageId });
        response.EnsureSuccessStatusCode();

        // Assert — é o ✓✓ azul chegando em tempo real.
        var receipt = await Await(read);
        receipt.ConversationId.Should().Be(conversationId);
        receipt.UserId.Should().Be(bob.Id);
        receipt.LastReadMessageId.Should().Be(messageId);
    }

    // ---------- Digitando ----------

    [Fact]
    public async Task Deveria_propagar_o_indicador_de_digitando()
    {
        // Arrange
        var (alice, bob, conversationId) = await PairAsync();
        var aliceConnection = await ConnectAsync(alice.Token);
        var bobConnection = await ConnectAsync(bob.Token);
        var (typing, _) = Expect<Guid, Guid, bool>(aliceConnection, nameof(IChatClient.UserTyping));

        // Act
        await bobConnection.InvokeAsync(nameof(ChatHub.Typing), conversationId, true);

        // Assert
        var (notifiedConversationId, notifiedUserId, isTyping) = await Await(typing);
        notifiedConversationId.Should().Be(conversationId);
        notifiedUserId.Should().Be(bob.Id);
        isTyping.Should().BeTrue();
    }

    [Fact]
    public async Task Nao_deveria_propagar_digitando_de_quem_nao_participa()
    {
        // Arrange
        var (alice, _, conversationId) = await PairAsync();
        var intruso = await CreateUserAsync();
        var aliceConnection = await ConnectAsync(alice.Token);
        var intrusoConnection = await ConnectAsync(intruso.Token);

        var received = false;
        aliceConnection.On<Guid, Guid, bool>(nameof(IChatClient.UserTyping), (_, _, _) => received = true);

        // Act — o hub valida a participação antes de propagar.
        await intrusoConnection.InvokeAsync(nameof(ChatHub.Typing), conversationId, true);
        await Task.Delay(TimeSpan.FromSeconds(1));

        // Assert
        received.Should().BeFalse();
    }

    // ---------- Presença ----------

    [Fact]
    public async Task Deveria_notificar_contatos_quando_o_usuario_fica_online()
    {
        // Arrange
        var (alice, bob, _) = await PairAsync();
        var aliceConnection = await ConnectAsync(alice.Token);
        var (presence, _) = Expect<Guid, bool, DateTime?>(aliceConnection, nameof(IChatClient.PresenceChanged));

        // Act
        await ConnectAsync(bob.Token);

        // Assert
        var (userId, isOnline, _) = await Await(presence);
        userId.Should().Be(bob.Id);
        isOnline.Should().BeTrue();
    }

    [Fact]
    public async Task Deveria_notificar_contatos_com_visto_por_ultimo_ao_desconectar()
    {
        // Arrange
        var (alice, bob, _) = await PairAsync();
        var aliceConnection = await ConnectAsync(alice.Token);
        var bobConnection = await ConnectAsync(bob.Token);

        var tcs = new TaskCompletionSource<(Guid, bool, DateTime?)>(TaskCreationOptions.RunContinuationsAsynchronously);
        aliceConnection.On<Guid, bool, DateTime?>(nameof(IChatClient.PresenceChanged), (userId, isOnline, lastSeenAt) =>
        {
            // Ignora o evento de "ficou online" disparado pela conexão do Bob.
            if (userId == bob.Id && !isOnline)
                tcs.TrySetResult((userId, isOnline, lastSeenAt));
        });

        // Act
        await bobConnection.StopAsync();

        // Assert
        var (offlineUserId, online, lastSeen) = await Await(tcs.Task);
        offlineUserId.Should().Be(bob.Id);
        online.Should().BeFalse();
        lastSeen.Should().NotBeNull();
    }

    [Fact]
    public async Task Deveria_persistir_o_visto_por_ultimo_no_identity_apos_desconectar()
    {
        // Arrange
        var (alice, bob, _) = await PairAsync();
        var aliceConnection = await ConnectAsync(alice.Token);
        var bobConnection = await ConnectAsync(bob.Token);

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        aliceConnection.On<Guid, bool, DateTime?>(nameof(IChatClient.PresenceChanged), (userId, isOnline, _) =>
        {
            if (userId == bob.Id && !isOnline)
                tcs.TrySetResult(true);
        });

        // Act
        await bobConnection.StopAsync();
        await Await(tcs.Task);

        // Assert — quem guarda o "visto por último" é o módulo Identity, via IUserPresenceSink.
        var me = await GetDataAsync(bob.Client, "/api/v1/users/me");
        me.GetProperty("lastSeenAt").ValueKind.Should().NotBe(System.Text.Json.JsonValueKind.Null);
    }
}
