using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Owin;
using MassTransit;
using Microsoft.Extensions.Logging;
using AutofacSerilogIntegration;
using Serilog;

namespace MassTransitSyncContext
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Debug()
                .MinimumLevel.Verbose()
                .CreateLogger();

            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();

            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder.RegisterLogger();
            builder.RegisterType<SerilogLoggerFactory>()
                .As<ILoggerFactory>()
                .SingleInstance();

            builder.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("192.168.56.5", "/", h =>
                    {
                        h.Username("admin");
                        h.Password("admin");
                    });
                });
            });

            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(config);
            app.UseWebApi(config);

            container.Resolve<IBusControl>().Start();
        }
    }
}