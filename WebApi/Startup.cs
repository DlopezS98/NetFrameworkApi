﻿using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Endpoint.Extensions;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using WebApi.Providers;
using System.Linq;
using System;
using System.Web.Http.Controllers;
using Domain.Endpoint.Interfaces.Services;
using Domain.Endpoint.Services;
using Application.Endpoint.Handlers;

[assembly: OwinStartup(typeof(WebApi.Startup))]

namespace WebApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            ServiceCollection services = new ServiceCollection();
            HttpConfiguration config = new HttpConfiguration();

            ConfigureServices(services);
            DefaultDependencyResolver resolver = new DefaultDependencyResolver(services.BuildServiceProvider());
            // (Setup): Support for MVC controllers injection...
            // DependencyResolver.SetResolver(resolver);

            // (Setup): Support for Web Api Controllers injection...
            config.DependencyResolver = resolver;
            WebApiConfig.Register(config);
            app.UseWebApi(config);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Map controllers to use as services (DPI)
            services.AddControllersAsServices(
                typeof(Startup).Assembly
                    .GetExportedTypes()
                    .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                    .Where(t => typeof(IHttpController).IsAssignableFrom(t) || t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
            );

            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssembly(typeof(MediatRHandlers).Assembly);
            });

            services.AddScoped<IToDosService, ToDosService>();
            services.AddInfrastructureServices();
        }
    }
}
