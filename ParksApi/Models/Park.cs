using System.ComponentModel.DataAnnotations;

namespace ParksApi.Models;

public class Park
{
    public int Id { get; set; }
    [Required]
    public string ParkCode { get; set; }
    [Required]
    public bool IsStatePark { get; set; }
    [Required]
    public string StateCode { get; set; }
    [Required]
    public string FullName { get; set; }
    [Required]
    public string Description { get; set; }
}