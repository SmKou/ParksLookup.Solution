using System.ComponentModel.DataAnnotations;

namespace ParksApi.Models;

public class Park
{
    public int ParkId { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public string State { get; set; }
    [Required]
    public string Directions { get; set; }
}