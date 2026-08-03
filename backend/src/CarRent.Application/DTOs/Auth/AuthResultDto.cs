namespace CarRent.Application.DTOs.Auth;

public class AuthResultDto
{
    public bool IsSuccess { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiry { get; set; }
    public string Message { get; set; } = string.Empty;
}
