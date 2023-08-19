using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ParksApi.Models;

public class ApplicationUser : IdentityUser
{
    /* Inherited */
    // Id
    // UserName
    // Email
    // PhoneNumber
    [Required]
    public string Name { get; set; }
    [Required]
    public int ParkId { get; set; }
    [Required]
    public bool IsConfirmedEmployee { get; set; }
}