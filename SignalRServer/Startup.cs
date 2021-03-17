using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalRServer.Hubs;

namespace SignalRServer
{
    public class Startup
    {
      
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddSignalR();
        }

        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(builder => 
            {
                builder
                .WithOrigins("null") //Http Header origin null meaning it will accept request from localhost
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            });

            // app.UseSignalR(endpoints => 
            // {
            //     endpoints.MapHub<ChatHub>("/chatHub");
            // });
            app.UseRouting();

            app.UseEndpoints(endpoints => 
            {
                endpoints.MapHub<ChatHub>("/chatHub");
            });
        }
    }
}
