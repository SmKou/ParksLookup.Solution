using System.ComponentModel.DataAnnotations;

namespace ParksApi.Models;

public class UserPark
{
    public int UserParkId { get; set; }
    public string ParkCode { get; set; }
    public ApplicationUser User { get; set; }
}