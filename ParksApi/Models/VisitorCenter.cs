using System.ComponentModel.DataAnnotations;

namespace ParksApi.Models;

public class VisitorCenter
{
    public int VisitorCenterId { get; set; }
    [Required]
    public string CenterName { get; set; }
    public string Description { get; set; }
    [Required]
    public string PhysicalAddress { get; set; }
    [Required]
    public string MailingAddress { get; set; }
    [Required]
    [RegularExpression(@"^[1-9]\d{2}-[1-9]\d{2}-\d{4}$", ErrorMessage = "Enter a valid phone number")]
    public string PhoneNumber { get; set; }
    [Required]
    public int ParkId { get; set; }
}