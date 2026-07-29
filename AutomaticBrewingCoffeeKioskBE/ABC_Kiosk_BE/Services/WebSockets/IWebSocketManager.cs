using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Services.WebSockets
{
    public interface IWebSocketManager
    {
        Task AddConnectionAsync(WebSocket webSocket, string connectionId);
        Task RemoveConnectionAsync(string connectionId);
        Task SendMessageToAllAsync(string message);
        Task SendMessageToConnectionAsync(WebSocket webSocket, string message);
    }
}
