using System.ComponentModel.DataAnnotations;

namespace ParksApi.InputModels;

public class LoginInputModel
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public string UserNameOrEmail { get; set; }
    
    [Required]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{6,}$", ErrorMessage = "Your login is invalid.")]
    public string Password { get; set; }
}