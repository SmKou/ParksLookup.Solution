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
    public string FirstName { get; set; }
    public string LastName { get; set; }
}