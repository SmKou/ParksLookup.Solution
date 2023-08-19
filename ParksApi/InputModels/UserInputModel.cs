using System.ComponentModel.DataAnnotations;

namespace ParksApi.InputModels;

public class UserInputModel
{
    [Required]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Enter a valid email")]
    public string Email { get; set; }

    [Required]
    [RegularExpression(@"^[1-9]\d{2}-[1-9]\d{2}-\d{4}$", ErrorMessage = "Enter a valid phone number")]
    public string PhoneNumber { get; set; }

    [Required]
    public string GivenName { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{6,}$", ErrorMessage = "Your password must contain at least 8 characters, a capital letter, a lowercase letter, a number and a special character.")]
    public string Password { get; set; }
    
    [Required]
    public int ParkId { get; set; }
}