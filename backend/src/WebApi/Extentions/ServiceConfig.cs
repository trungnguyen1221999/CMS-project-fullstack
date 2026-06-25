namespace WebApi.Extentions
{
    public static class ServiceConfig
    {
        public static WebApplicationBuilder ConfigureApplicationServices(
            this WebApplicationBuilder builder
        )
        {
            builder
                .Services.AddControllers()
                .AddJsonOptions(option =>
                {
                    option.JsonSerializerOptions.UnmappedMemberHandling = System
                        .Text
                        .Json
                        .Serialization
                        .JsonUnmappedMemberHandling
                        .Disallow;
                });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            return builder;
        }
    }
}
