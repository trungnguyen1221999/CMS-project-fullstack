namespace Application.DTOs.Response.Auth
{
    public class SignInResponseDto
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }

        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
