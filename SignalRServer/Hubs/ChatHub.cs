using System;
using Microsoft.AspNetCore.SignalR; 
using System.Threading.Tasks;
using Newtonsoft.Json;

//Signal R is included in the main Asp.Net core package no need to install separate package for Signal R
namespace SignalRServer.Hubs
{
    public class ChatHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine("-----> Connection Established --------->" + Context.ConnectionId);
            Clients.Client(Context.ConnectionId).SendAsync("ReceiveConnID", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public async Task SendMessageAsync(string message)
        {
            var routeOb = JsonConvert.DeserializeObject<dynamic>(message);
            string toClient = routeOb.To;
            Console.WriteLine("Message Received on: " + Context.ConnectionId);

            if(toClient == string.Empty)
            {
                await Clients.All.SendAsync("ReceiveMessage", message);
            }
            else
            {
                await Clients.Client(toClient).SendAsync("ReceiveMessage", message);
            }
        }
    }
}