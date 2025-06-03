namespace WebApi.Models;

public class VerificationServiceResult
{
    public bool Suceeded { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
}
