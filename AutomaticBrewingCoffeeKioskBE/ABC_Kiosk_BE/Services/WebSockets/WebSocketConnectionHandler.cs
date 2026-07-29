using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Services.WebSockets
{
    public class WebSocketConnectionHandler
    {
        private readonly IWebSocketManager _webSocketManager;
        private readonly ILogger<WebSocketConnectionHandler> _logger;

        public WebSocketConnectionHandler(IWebSocketManager webSocketManager, ILogger<WebSocketConnectionHandler> logger)
        {
            _webSocketManager = webSocketManager;
            _logger = logger;
        }

        public async Task HandleAsync(WebSocket webSocket, string connectionId)
        {
            await _webSocketManager.AddConnectionAsync(webSocket, connectionId);

            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogInformation($"Received from {connectionId}: {message}");

                        var response = $"Server received at {DateTime.Now:HH:mm:ss}: {message}";
                        await _webSocketManager.SendMessageToAllAsync(response);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
            }
            catch (WebSocketException ex)
            {
                _logger.LogError(ex, $"WebSocket error for {connectionId}");
            }
            finally
            {
                await _webSocketManager.RemoveConnectionAsync(connectionId);
            }
        }
    }
}
