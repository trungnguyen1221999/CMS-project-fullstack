using Serilog;

namespace WebApi.Extensions
{
    public static class SerilogConfig
    {
        public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog(
                (context, configuration) =>
                    configuration.ReadFrom.Configuration(context.Configuration)
            );

            return builder;
        }

        public static WebApplication UseSerilogMiddleware(this WebApplication app)
        {
            app.UseSerilogRequestLogging();
            return app;
        }
    }
}
