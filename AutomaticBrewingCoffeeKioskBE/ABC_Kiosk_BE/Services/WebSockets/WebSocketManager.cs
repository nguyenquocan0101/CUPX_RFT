using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Services.WebSockets
{
    public class WebSocketManager : IWebSocketManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
        private readonly ILogger<WebSocketManager> _logger;

        public WebSocketManager(ILogger<WebSocketManager> logger)
        {
            _logger = logger;
        }

        public async Task AddConnectionAsync(WebSocket webSocket, string connectionId)
        {
            _connections.TryAdd(connectionId, webSocket);
            _logger.LogInformation($"WebSocket connection added: {connectionId}");

            await SendMessageToConnectionAsync(webSocket, "Chào mừng bạn đến với WebSocket server!");
        }

        public async Task RemoveConnectionAsync(string connectionId)
        {
            if (_connections.TryRemove(connectionId, out var connection))
            {
                if (connection.State == WebSocketState.Open)
                {
                    await connection.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Connection removed",
                        CancellationToken.None);
                }
                _logger.LogInformation($"WebSocket connection removed: {connectionId}");
            }
        }

        public async Task SendMessageToAllAsync(string message)
        {
            var tasks = new List<Task>();
            foreach (var connection in _connections.Values)
            {
                if (connection.State == WebSocketState.Open)
                {
                    tasks.Add(SendMessageToConnectionAsync(connection, message));
                }
            }
            await Task.WhenAll(tasks);
        }

        public async Task SendMessageToConnectionAsync(WebSocket webSocket, string message)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WebSocket message");
            }
        }
    }
}
