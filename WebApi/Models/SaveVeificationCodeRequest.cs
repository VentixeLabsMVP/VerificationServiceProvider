using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public class SaveVeificationCodeRequest
{
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public string Code { get; set; } = null!;

    public TimeSpan ValidFor {  get; set; }
}
