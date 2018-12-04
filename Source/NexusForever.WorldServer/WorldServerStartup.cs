﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NexusForever.Shared.Database.Auth.Model;
using NexusForever.WorldServer.Command;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Database.World.Model;
using NexusForever.WorldServer.Web.Controllers;

namespace NexusForever.WorldServer
{
    public class WorldServerStartup
    {
        public IConfiguration Configuration { get; }

        public WorldServerStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            CommandManager.RegisterServices(services);
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);
            services.AddScoped<AuthContext>();
            services.AddScoped<WorldContext>();
            services.AddScoped<CharacterContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            app.UseStaticFiles();
            app.UseWebSockets(new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(30),
                ReceiveBufferSize = 16384
            });
            app.UseMiddleware<WebSocketMiddleware>();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}