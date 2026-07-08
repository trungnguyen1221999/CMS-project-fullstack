using WebApi.Middlewares;

namespace WebApi.Extensions
{
    public static class MiddlewareConfig
    {
        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }
    }
}
