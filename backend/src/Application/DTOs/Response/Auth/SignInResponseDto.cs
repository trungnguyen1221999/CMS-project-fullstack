namespace Application.DTOs.Response.Auth
{
    public class SignInResponseDto : ResponseDto<string>
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
