using System.ComponentModel.DataAnnotations;

namespace ParksApi.InputModels;

public class LoginInputModel
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Password { get; set; }
}