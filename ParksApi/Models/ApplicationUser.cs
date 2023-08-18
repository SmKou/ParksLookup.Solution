using Microsoft.AspNetCore.Identity;

namespace ParksApi.Models;

public class ApplicationUser : IdentityUser
{
    /* Inherited */
    // Id
    // UserName
    // Email
    [Required]
    public string Name { get; set; }
    [required]
    [DataType(DataType.PhoneNumber)]
    public string PhoneNumber { get; set; }
    [Required]
    public int ParkId { get; set; }
    [Required]
    public bool IsConfirmedEmployee { get; set; }
}