using Application.DTOs.Response;

namespace Application.DTOs.Response.Auth
{
    public class SignInResponseDto : WriteResponseDto
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
