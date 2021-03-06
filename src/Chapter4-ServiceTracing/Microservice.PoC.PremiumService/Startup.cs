﻿using Microservice.PoC.PremiumService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pivotal.Discovery.Client;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Common.Http.Discovery;
using Steeltoe.Management.Exporter.Tracing;
using Steeltoe.Management.Tracing;
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
            // Add Steeltoe Hystrix Command
            services.AddHystrixCommand<ClientServiceCommand>("ClientService", Configuration);
            // Add Steeltoe Distributed Tracing
            services.AddDistributedTracing(Configuration);
            // Export traces to Zipkin
            services.AddZipkinExporter(Configuration);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Add Hystrix Metrics to container
            services.AddHystrixMetricsStream(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Add Steeltoe Discovery Client service
            app.UseDiscoveryClient();

            app.UseMvc();
            
            // Start Hystrix metrics stream service
            app.UseHystrixMetricsStream();
            // Start up trace exporter
            app.UseTracingExporter();
        }
    }
}
