using System.ComponentModel.DataAnnotations;
using ParksApi.Models;

namespace ParksApi.InputModels;

public class SeedInputModel
{
    [Required]
    public string ParkName { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public string State { get; set; }

    [Required]
    public string Directions { get; set; }

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
    public string FullName { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{6,}$", ErrorMessage = "Your password must contain at least 8 characters, a capital letter, a lowercase letter, a number and a special character.")]
    public string Password { get; set; }
}