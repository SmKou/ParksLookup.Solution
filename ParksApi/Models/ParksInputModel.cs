using System.ComponentModel.DataAnnotations;

namespace ParksApi.Models;

public class ParksInputModel
{
    [Required]
    public string UserNameOrEmail { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public List<string> Parks { get; set; }
}