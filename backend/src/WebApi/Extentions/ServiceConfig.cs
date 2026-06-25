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

            builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context
                        .ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList();

                    var message = errors.Any()
                        ? string.Join(" | ", errors)
                        : "Invalid request.";

                    return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
                        new Application.DTOs.Response.WriteResponseDto
                        {
                            IsSuccess = false,
                            ErrorCode = Application.Constants.ErrorMessages.Common.InvalidRequest,
                            ErrorMessage = message,
                        }
                    );
                };
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            return builder;
        }
    }
}
