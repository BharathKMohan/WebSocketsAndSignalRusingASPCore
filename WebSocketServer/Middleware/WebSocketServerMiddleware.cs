using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace WebSocketServer.Middleware
{
    public class WebSocketServerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketServerConnectionManager _manager;

        public WebSocketServerMiddleware(RequestDelegate next, WebSocketServerConnectionManager manager)
        {
            _next = next;
            _manager = manager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket websocket = await context.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine("Websocket connected");

                string ConnID = _manager.AddSocket(websocket);
                await SendConnectionIdAsync(websocket, ConnID);

                await ReceiveMessage(websocket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        Console.WriteLine("Message Received");
                        Console.WriteLine($"Message : {Encoding.UTF8.GetString(buffer, 0, result.Count)}");
                        await RouteJsonMessageAsync(Encoding.UTF8.GetString(buffer, 0, result.Count));
                        return;
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        string id = _manager.GetAllSockets().FirstOrDefault(s => s.Value == websocket).Key;
                        Console.WriteLine("Received Close Message");
                        _manager.GetAllSockets().TryRemove(id, out WebSocket sock);
                        await sock.CloseAsync(result.CloseStatus.Value,result.CloseStatusDescription,CancellationToken.None);
                        return;
                    }
                });
            }
            else
            {
                Console.WriteLine("Hello from 2rd Request delegate or middleware");
                await _next(context);
            }
        }

        private async Task SendConnectionIdAsync(WebSocket socket, string connID)
        {
            var buffer = Encoding.UTF8.GetBytes ("ConnId: " + connID);
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task ReceiveMessage(WebSocket socket, Action<WebSocketReceiveResult,byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while(socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer : new ArraySegment<byte>(buffer),
                                                        cancellationToken : CancellationToken.None);

                handleMessage(result,buffer);
            }
        }        

       public async Task RouteJsonMessageAsync(string message)
       {
           var routeOb = JsonConvert.DeserializeObject<dynamic>(message);

           if(Guid.TryParse(routeOb.To.ToString(), out Guid guidOut))
           {
               Console.WriteLine("Targeted");

               var sock = _manager.GetAllSockets().FirstOrDefault(s => s.Key == routeOb.To.ToString());

                if (sock.Value != null)
                {
                    if (sock.Value.State == WebSocketState.Open)
                    {
                        await sock.Value.SendAsync(Encoding.UTF8.GetBytes(routeOb.Message.ToString()), WebSocketMessageType.Text, true, CancellationToken.None);
                    }

                }
                else
                {
                    Console.WriteLine("Invalid recipeint");
                }
           }
           else{
               Console.WriteLine("Broadcast");

               foreach (var socket in _manager.GetAllSockets())
               {
                   if(socket.Value.State == WebSocketState.Open)
                   {
                       await socket.Value.SendAsync(Encoding.UTF8.GetBytes(routeOb.Message.ToString()), WebSocketMessageType.Text, true, CancellationToken.None);
                   }
               }
           }
       }
    }
}