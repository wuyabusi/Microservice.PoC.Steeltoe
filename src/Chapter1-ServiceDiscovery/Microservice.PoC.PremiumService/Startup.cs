﻿using Microservice.PoC.PremiumService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pivotal.Discovery.Client;
using Steeltoe.Common.Http.Discovery;
using System;

namespace Microservice.PoC.PremiumService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IClientService, ClientService>();
            // Add Steeltoe Discovery Client service
            services.AddDiscoveryClient(Configuration);
            // Add Steeltoe handler to container
            services.AddTransient<DiscoveryHttpMessageHandler>();
            // Configure a HttpClient
            services.AddHttpClient("client-api-values", c =>
            {
                c.BaseAddress = new Uri(Configuration["Services:Client-Service:Url"]);
            })
            .AddHttpMessageHandler<DiscoveryHttpMessageHandler>()
            .AddTypedClient<IClientService, ClientService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            // Add Steeltoe Discovery Client service
            app.UseDiscoveryClient();
        }
    }
}
