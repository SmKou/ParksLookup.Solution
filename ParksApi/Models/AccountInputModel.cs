using System.ComponentModel.DataAnnotations;

namespace ParksApi.Models;

public class AccountInputModel : UserInputModel
{
    public string NewUserName { get; set; }
    public string NewEmail { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{6,}$", ErrorMessage = "Your password must contain at least 8 characters, a capital letter, a lowercase letter, a number and a special character.")]
    public string NewPassword { get; set; }
}