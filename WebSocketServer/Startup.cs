using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.WebSockets;
using System.Threading;
using WebSocketServer.Middleware;

namespace WebSocketServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //Use an extension method on IServicesCollection to add WebsocketserverconnectionManager to the dependency injection container

            services.AddWebServerSocketManager();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseWebSockets();

            // app.Use(async (context, next) => {
            //     //WriteRequestParam(context);
            //     if(context.WebSockets.IsWebSocketRequest){
            //         WebSocket websocket = await context.WebSockets.AcceptWebSocketAsync();
            //         Console.WriteLine("Websocket connected");

            //         await ReceiveMessage(websocket, async (result, buffer) => 
            //         {
            //             if(result.MessageType == WebSocketMessageType.Text)
            //             {
            //                 Console.WriteLine("Message Received");
            //                 return;
            //             }
            //             else if(result.MessageType == WebSocketMessageType.Close)
            //             {
            //                 Console.WriteLine("Received Close Message");
            //                 return;
            //             }
            //         });
            //     }
            //     else{
            //         Console.WriteLine("Hello from 2rd Request delegate or middleware");
            //         await next();
            //     }
            // });

            //The aboove code is replaced with the IApplicationBuilder Middleware implementation - UseWebSocketServer 

            app.UseWebSocketServer();
            app.Run(async context =>
            {
                Console.WriteLine("Hello from 3rd Request delegate or middleware");
                await context.Response.WriteAsync("Hello from 3rd Request delegate or middleware");
            });
        }

        public void WriteRequestParam(HttpContext context)
        {
            Console.WriteLine("Request Method : " + context.Request.Method);
            Console.WriteLine("Request Protocol : " + context.Request.Protocol);

            if(context.Request.Headers != null)
            {
                foreach(var h in context.Request.Headers)
                {
                    Console.WriteLine(" --> " + h.Key + " : " + h.Value);
                }
            }
        }

      
    }
}
