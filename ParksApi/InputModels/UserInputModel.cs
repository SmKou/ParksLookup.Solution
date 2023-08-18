namespace ParksApi.InputModels;

public class UserInputModel
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public int ParkId { get; set; }
}