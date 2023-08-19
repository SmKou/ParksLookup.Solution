using System.ComponentModel.DataAnnotations;
using ParksApi.Models;

namespace ParksApi.InputModels;

public class SeedInputModel
{
    [Required]
    public string ParkName { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public string State { get; set; }
    [Required]
    public string Directions { get; set; }

    [Required]
    public string UserName { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string GivenName { get; set; }
    [Required]
    public string Password { get; set; }

    public static void Seed(ParksContext db)
    {
        Park parkA = new Park
        {
            Name = "Alagnak",
            Description = "The headwaters of Alagnak Wild River lie within the rugged Aleutian Range of neighboring Katmai National Park and Preserve. Meandering west towards Bristol Bay and the Bering Sea, the Alagnak traverses the beautiful Alaska Peninsula, providing an unparalleled opportunity to experience the unique wilderness, wildlife, and cultural heritage of southwest Alaska.",
            State = "Alaska",
            Directions = "Alagnak Wild River is located in a remote part of the Alaska Peninsula, about 290 miles southwest of Anchorage. Access is by boat or small floatplane."
        };
        Park parkB = new Park
        {
            Name = "Cape Krusenstern",
            Description = "North of the Arctic Circle, the monument forms 70 miles of shoreline on the Chukchi Sea.  More than 114 beach ridges provide evidence of human use for 5,000 years.  The Inupiat continue to use the area today.  Vast wetlands provide habitat for shorebirds from as far away as South America.  Hikers and boaters can see carpets of wildflowers among shrubs containing wisps of qiviut from muskoxen.",
            State = "Alaska",
            Directions = "Cape Krusenstern National Monument lies within a remote area of northwest Alaska and is bordered by the Arctic Ocean and Chukchi Sea. Visitors generally access the monument via the regional hub in Kotzebue. Commercial airlines provide daily service from Anchorage to Kotzebue. Chartered flights with licensed air taxi services, booked in advance, can take backcountry travelers to remote destinations within the monument."
        };
        db.Parks.AddRange(new Park[]
        {
            parkA,
            parkB
        });

        VisitorCenter centerA = new VisitorCenter
        {
            Name = "King Salmon Visitor Center",
            Description = "Located next door to the King Salmon Airport, the King Salmon Visitor Center provides information on the many federal public lands of Southwest Alaska, particularly those in the Bristol Bay area. A large collection of films is available for viewing and an Alaska Geographic bookstore sells maps, charts, videos, posters, clothing and more.",
            PhysicalAddress = "1000 Silver St.,  Bldg. 603 PO Box 7 King Salmon 99613",
            MailingAddress = "PO Box 245 King Salmon, AK 99613",
            PhoneNumber = "907-246-3305",
            ParkId = 1
        };
        VisitorCenter centerB = new VisitorCenter
        {
            Name = "Northwest Arctic Heritage Center",
            Description = "Large, half-dome shaped, blue and grey building with just over 11,000 square feet of space. The museum space is just over 1,800 square feet and contains animal displays, soundscapes, tactile exhibits and more. The Heritage Center also contains a bookstore, restroom, art gallery, and sitting area. The Northwest Arctic Heritage Center serves as the visitor centers for the Western Arctic National Parklands: Kobuk Valley National Park, Cape Krusenstern National Monument, and Noatak National Preserve.",
            PhysicalAddress = "171 3rd Ave Kotzebue, AK 99752",
            MailingAddress = "PO Box 1029 Kotzebue, AK 99752",
            PhoneNumber = "907 442-3890",
            ParkId = 2
        };
        db.Centers.AddRange(new VisitorCenter[] 
        {
            centerA,
            centerB
        });
        db.SaveChangesAsync();
    }
}