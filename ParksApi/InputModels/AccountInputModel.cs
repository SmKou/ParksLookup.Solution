using System.ComponentModel.DataAnnotations;

namespace ParksApi.InputModels;

public class AccountInputModel : UserInputModel
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{6,}$", ErrorMessage = "Your password must contain at least 8 characters, a capital letter, a lowercase letter, a number and a special character.")]
    public string NewPassword { get; set; }
}