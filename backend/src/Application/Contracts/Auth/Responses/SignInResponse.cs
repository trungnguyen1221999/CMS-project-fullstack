namespace Application.Contracts.Auth.Responses
{
    public class SignInResponse
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
