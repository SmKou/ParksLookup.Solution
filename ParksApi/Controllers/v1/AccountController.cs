using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using ParksApi.Models;

namespace ParksApi.Controllers;

[Route("api/v{version:ApiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly ParksContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signinManager;

    public AccountController(ParksContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signinManager)
    {
        _db = db;
        _userManager = userManager;
        _signinManager = signinManager;
    }

    [HttpGet]
    public async Task<ActionResult<UserViewModel>> Get([FromBody] LoginInputModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid login");

        ApplicationUser user = await _userManager.FindByEmailAsync(model.UserNameOrEmail);
        if (user == null)
        {
            user = await _userManager.FindByNameAsync(model.UserNameOrEmail);
            if (user == null)
                return NotFound();
        }

        UserViewModel view = new UserViewModel
        {
            UserName = user.UserName,
            FullName = user.FirstName + " " + user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };
        return Ok(view);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] AccountInputModel model)
    {
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        ApplicationUser user = await _userManager.FindByNameAsync(model.UserName);
        if (user == null)
            return NotFound();
        if (!string.IsNullOrEmpty(model.NewUserName) && !string.IsNullOrEmpty(model.NewEmail))
            return BadRequest("Update Invalid: Cannot change both username and email");

        if (!string.IsNullOrEmpty(model.NewUserName) && model.UserName != model.NewUserName)
            user.UserName = model.NewUserName;
        else if (!string.IsNullOrEmpty(model.NewEmail) && model.Email != model.NewEmail)
            user.Email = model.NewEmail;
        if (user.PhoneNumber != model.PhoneNumber)
            user.PhoneNumber = model.PhoneNumber;
        if (user.FirstName != model.FirstName)
            user.FirstName = model.FirstName;
        if (user.LastName != model.LastName)
            user.LastName = model.LastName;

        IdentityResult result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest("Update Failed: Could not update user");
        
        if (!string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.NewPassword))
        {
            IdentityResult password = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);
            if (!password.Succeeded)
                return BadRequest("Update Failed: Could not update user");
        }

        return Ok("User updated");
    }

    [HttpDelete]
    public async Task<ActionResult> Delete([FromBody] LoginInputModel model)
    {
        ApplicationUser user = await _userManager.FindByEmailAsync(model.UserNameOrEmail);
        if (user == null)
        {
            user = await _userManager.FindByNameAsync(model.UserNameOrEmail);
            if (user == null)
                return NotFound();
        }

        List<UserPark> parklist = _db.UserParks.Where(entry => entry.User == user).ToList();
        _db.UserParks.RemoveRange(parklist);

        IdentityResult result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
            return Ok("Account deleted");
        else
            return BadRequest("User could not be deleted");
    }

    public static string GenerateJSONWebToken()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Kou.dBlueParksLookupApiCodeReview"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "http://localhost:5006",
            audience: "http://localhost:5006",
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

[Route("api/v{version:apiVersion}/account/[controller]")]
[ApiVersion("1.0")]
public class RegisterController : ControllerBase
{
    private readonly ParksContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signinManager;

    public RegisterController(ParksContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signinManager)
    {
        _db = db;
        _userManager = userManager;
        _signinManager = signinManager;
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] UserInputModel model)
    {
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        ApplicationUser user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            FirstName = model.FirstName,
            LastName = model.LastName
        };
        IdentityResult result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            Microsoft.AspNetCore.Identity.SignInResult signin = await _signinManager.PasswordSignInAsync(model.UserName, model.Password, isPersistent: true, lockoutOnFailure: false);
            if (signin.Succeeded)
                return Ok("Token: " + AccountController.GenerateJSONWebToken());
            else
                return BadRequest("Auth Failed: Could not produce token.");
        }
        else
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("Registration Failed", error.Description);
            return UnprocessableEntity(ModelState);
        }
    }
}

[Route("api/v{version:apiVersion}/account/[controller]")]
[ApiVersion("1.0")]
public class LoginController : ControllerBase
{
    private readonly ParksContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signinManager;

    public LoginController(ParksContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signinManager)
    {
        _db = db;
        _userManager = userManager;
        _signinManager = signinManager;
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] LoginInputModel login)
    {
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        ApplicationUser user = await _userManager.FindByEmailAsync(login.UserNameOrEmail);
        if (user == null)
        {
            user = await _userManager.FindByNameAsync(login.UserNameOrEmail);
            if (user == null)
                return NotFound();
        }

        Microsoft.AspNetCore.Identity.SignInResult result = await _signinManager.PasswordSignInAsync(user, login.Password, isPersistent: true, lockoutOnFailure: false);
        if (result.Succeeded)
            return Ok("Token: " + AccountController.GenerateJSONWebToken());
        else
        {
            ModelState.AddModelError("Login Failed", "There is something wrong with your login or password.");
            return UnprocessableEntity(ModelState);
        }
    }
}

[Route("api/v{version:apiVersion}/account/[controller]")]
[ApiVersion("1.0")]
public class SeedController : ControllerBase
{
    private readonly ParksContext _db;

    public SeedController(ParksContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult> Get()
    {
        if (!_db.Parks.Any())
        {
            _db.Parks.AddRange(Seed);
            await _db.SaveChangesAsync();
            return Ok("Database seeded");
        }
        return BadRequest("Database already seeded");
    }

    private static Park[] Seed = new Park[]
    {
        new Park
        {
            ParkCode = "alag",
            IsStatePark = false,
            StateCode = "AK",
            FullName = "Alagnak Wild River",
            Description = "The headwaters of Alagnak Wild River lie within the rugged Aleutian Range of neighboring Katmai National Park and Preserve. Meandering west towards Bristol Bay and the Bering Sea, the Alagnak traverses the beautiful Alaska Peninsula, providing an unparalleled opportunity to experience the unique wilderness, wildlife, and cultural heritage of southwest Alaska."
        },
        new Park
        {
            ParkCode = "anch",
            IsStatePark = false,
            StateCode = "AK",
            FullName = "Alaska Public Lands",
            Description = "Alaska’s parks, forests, and refuges are rich and varied. The Alaska Public Lands Information Centers help visitors and residents to have meaningful, safe, enjoyable experiences on public lands, and encourages them to sustain the natural and cultural resources of Alaska. These centers provide trip-planning, interpretation, and education for all ages."
        },
        new Park
        {
            ParkCode = "aleu",
            IsStatePark = false,
            StateCode = "AK",
            FullName = "Aleutian Islands World War II National Historic Area",
            Description = "During World War II the remote Aleutian Islands, home to the Unangax̂ (Aleut) people for over 8,000 years, became a fiercely contested battleground in the Pacific. This thousand-mile-long archipelago saw invasion by Japanese forces, the occupation of two islands; a mass relocation of Unangax̂ civilians; a 15-month air war; and one of the deadliest battles in the Pacific Theater."
        },
        new Park
        {
            ParkCode = "ania",
            IsStatePark = false,
            StateCode = "AK",
            FullName = "Aniakchak National Monument & Preserve",
            Description = "Given its remote location and challenging weather conditions, Aniakchak is one of the most wild and least visited places in the National Park System. This landscape is a vibrant reminder of Alaska's location in the volcanically active \"Ring of Fire,\" as it is home to an impressive six mile (10 km) wide, 2,500 ft (762 m) deep caldera formed during a massive volcanic eruption 3,500 years ago."
        },
        new Park
        {
            ParkCode = "bela",
            IsStatePark = false,
            StateCode = "AK",
            FullName = "Bering Land Bridge National Preserve",
            Description = "Bering Land Bridge National Preserve lies at the continental crossroad that greatly influenced the distribution of life in the Western Hemisphere during the Pleistocene Epoch. It is a vital landscape for indigenous communities who depend on the land just as their ancestors did for many generations. It is a wild and ecologically healthy landscape unlike any other."
        },
        new Park
        {
            ParkCode = "cakr",
            IsStatePark = false,
            StateCode = "AK",
            FullName = "Cape Krusenstern National Monument",
            Description = "North of the Arctic Circle, the monument forms 70 miles of shoreline on the Chukchi Sea. More than 114 beach ridges provide evidence of human use for 5,000 years. The Inupiat continue to use the area today. Vast wetlands provide habitat for shorebirds from as far away as South America. Hikers and boaters can see carpets of wildflowers among shrubs containing wisps of qiviut from muskoxen."
        },
        new Park
        {
            ParkCode = "dena",
            IsStatePark = false,
            StateCode = "AK",
            FullName = "Denali National Park & Preserve",
            Description = "Denali is six million acres of wild land, bisected by one ribbon of road. Travelers along it see the relatively low-elevation taiga forest give way to high alpine tundra and snowy mountains, culminating in North America's tallest peak, 20,310' Denali. Wild animals large and small roam un-fenced lands, living as they have for ages. Solitude, tranquility and wilderness await."
        },
        new Park
        {
            ParkCode = "gaar",
            IsStatePark = false,
            StateCode = "AK",
            FullName = "Gates Of The Arctic National Park & Preserve",
            Description = "This vast landscape does not contain any roads or trails. Visitors discover intact ecosystems where people have lived with the land for over ten thousand years. Wild rivers meander through glacier-carved valleys, caribou migrate along age-old trails, endless summer light fades into aurora-lit night skies of winter. Virtually unchanged, except by the forces of nature."
        },
        new Park
        {
            ParkCode = "glba",
            IsStatePark = false,
            StateCode = "AK",
            FullName = "Glacier Bay National Park & Preserve",
            Description = "Covering 3.3 million acres of rugged mountains, dynamic glaciers, temperate rainforest, wild coastlines and deep sheltered fjords, Glacier Bay National Park is a highlight of Alaska's Inside Passage and part of a 25-million acre World Heritage Site—one of the world’s largest international protected areas. From sea to summit, Glacier Bay offers limitless opportunities for adventure and inspiration."
        },
        new Park
        {
            ParkCode = "inup",
            IsStatePark = false,
            StateCode = "AK",
            FullName = "Iñupiat Heritage Center",
            Description = "On the rooftop of the world, the Iñupiat Heritage Center in Barrow, Alaska, tells the story of the Iñupiat people. They have thrived for thousands of years in one of the most extreme climates on Earth, hunting the bowhead, or \"Agviq.\" In the 19th century, the quiet northern seas swarmed with commercial whalemen from New England, who also sought the bowhead for its valuable baleen and blubber."
        },
        new Park
        {
            ParkCode = "katm",
            IsStatePark = false,
            StateCode = "AK",
            FullName = "Katmai National Park & Preserve",
            Description = "A landscape is alive underneath our feet, filled with creatures that remind us what it is to be wild. Katmai was established in 1918 to protect the volcanically devastated region surrounding Novarupta and the Valley of Ten Thousand Smokes. Today, Katmai National Park and Preserve also protects 9,000 years of human history and important habitat for salmon and thousands of brown bears."
        },
        new Park
        {
            ParkCode = "kefj",
            IsStatePark = false,
            StateCode = "AK",
            FullName = "Kenai Fjords National Park",
            Description = "At the edge of the Kenai Peninsula lies a land where the ice age lingers. Nearly 40 glaciers flow from the Harding Icefield, Kenai Fjords' crowning feature. Wildlife thrives in icy waters and lush forests around this vast expanse of ice. Sugpiaq people relied on these resources to nurture a life entwined with the sea. Today, shrinking glaciers bear witness to the effects of our changing climate."
        },
        new Park
        {
            ParkCode = "klgo",
            IsStatePark = false,
            StateCode = "AK",
            FullName = "Klondike Gold Rush National Historical Park",
            Description = "Headlines screamed \"Gold!\" The dream of a better life catapulted thousands of people to Alaska and the Yukon Territory. Their journey shaped them, and changed the people they encountered and the north forever. Today, the park remembers the trails, boomtowns, and stories of the Klondike Gold Rush."
        },
        new Park
        {
            ParkCode = "kova",
            IsStatePark = false,
            StateCode = "AK",
            FullName = "Kobuk Valley National Park",
            Description = "Caribou, sand dunes, the Kobuk River, Onion Portage - just some of the facets of Kobuk Valley National Park.Thousands of caribou migrate through, their tracks crisscrossing sculpted dunes.The Kobuk River is an ancient and current corridor for people and wildlife.For 9000 years, people came to Onion Portage to harvest caribou as they swam the river.Even today, that rich tradition continues."
        },
        new Park
        {
            ParkCode = "bdsp",
            IsStatePark = true,
            StateCode = "AK",
            FullName = "Big Delta State Historical Park",
            Description = "One of the greatest aspects of coming to Alaska is learning about its tremendous history. At Big Delta State Historical Park, you get to take a walk through the past. This site was an important crossroad for travelers, traders, and the military during the early days of the 20th century. Rika's Roadhouse is the centerpiece of the park. The house served travelers on the historic Valdez-to-Fairbanks Trail from 1913 to 1947. John Hajdukovich had the north-south section of this log structure built in 1913. The Valdez-to-Fairbanks Trail was being improved at this time and the roadhouse was a center of activity for gold stampeders, local hunters, traders, and freighters."
        },
        new Park
        {
            ParkCode = "blsr",
            IsStatePark = true,
            StateCode = "AK",
            FullName = "Birch Lake State Recreation Site",
            Description = "Birch Lake SRS is nestled between a lilypad covered lake and forested wetlands. Boat launch and courtesy dock with ADA Access. No long term docking, approximate 20 minute time limit. Users should bring their own firewood, especially in winter for use in the cabin, since there is not much left to scavenge in the surrounding forest. The lake is popular with fishers, jet skiers, and water skiers in the summer, and with snow machines and ice fishers in the winter. There is excellent fishing all year for stocked species: rainbow trout, king and silver salmon, grayling, and arctic char. Fishing during the open water months is best from a boat. Ice fishing huts are available for rent."
        },
        new Park
        {
            ParkCode = "cpsr",
            IsStatePark = true,
            StateCode = "AK",
            FullName = "Chena Pump State Recreation Site",
            Description = "The Chena Pump Wayside and Boat Launch is located at 8.7 mile Chena Pump Road, approximately 4.5 miles Southwest of the Mitchell Expressway overpass. This location covers approximately 60,000 sq ft of grassy areas with clusters of trees spread throughout the wayside. Featuring two large picnic shelters for picnicking activities, a port-a-potty for public use, and a beach access boat launch, it provides a great day location for Alaskan activities. Daily parking and boat launch fees apply for this location. The parking area is also maintained year-round."
        },
        new Park
        {
            ParkCode = "wtsp",
            IsStatePark = true,
            StateCode = "AK",
            FullName = "Wood-Tikchik State Park",
            Description = "At nearly 1.6 million acres, Wood-Tikchik State Park is the largest state park in the nation. This one park encompasses nearly half of the State Park land in Alaska and 15 percent of all state park land in the United States. The park's acreage is quite diverse and includes 12 lakes, over 1,000 acres, rivers up to 60 miles in length, mountains exceeding 5,000 feet in elevation, and extensive lowlands. Wood-Tikchik State Park was created in 1978 for the purposes of protecting the area's fish and wildlife breeding and support systems and to preserve the continued use of the area for subsistence and recreational activities. The land and water in this region are traditional grounds for subsistence fishing, hunting, and gathering. These activities are an integral part of the culture in this region and provide not only food but a cultural tie to the land."
        },
        new Park
        {
            ParkCode = "bbls",
            IsStatePark = true,
            StateCode = "AK",
            FullName = "Blueberry Lake State Recreation Site",
            Description = "Blueberry Lake State Recreation Site is located in spectacular Thompson Pass, 24 miles north of Valdez. The park is located at the large switchback before descending into Keystone Canyon. The high alpine lake offer excellent grayling fishing."
        },
        new Park
        {
            ParkCode = "kbsw",
            IsStatePark = true,
            StateCode = "AK",
            FullName = "Kachemak Bay State Wilderness Park",
            Description = "Kachemak Bay State Wilderness Park became Alaska's first state park in 1972. It abuts the southern boundary of Kachemak Bay State Park in the Kenai Mountains and extends south, into the waters of the Gulf of Alaska. The park offers excellent backcountry skiing, hiking, hunting, fishing, and sightseeing opportunities. It contains 198,399 acres including 79 miles of rugged coastline. There are no developed facilities in the park, however, a back-country hiking trail is being developed from Tutka Bay that will lead to Taylor Bay."
        },
        new Park
        {
            ParkCode = "ebla",
            IsStatePark = false,
            StateCode = "WA",
            FullName = "Ebey's Landing National Historical Reserve",
            Description = "This stunning landscape on the Salish Sea, with its rich farmland and promising seaport, lured the earliest American pioneers north of the Columbia River to Ebey’s Landing. Today Ebey’s Landing National Historical Reserve preserves the historical, agricultural and cultural traditions of both Native and Euro-American – while offering spectacular opportunities for recreation."
        },
        new Park
        {
            ParkCode = "fova",
            IsStatePark = false,
            StateCode = "WA",
            FullName = "Fort Vancouver National Historic Site",
            Description = "Located on the north bank of the Columbia River, in sight of snowy mountain peaks and a vibrant urban landscape, this park has a rich cultural past. From a frontier fur trading post, to a powerful military legacy, the magic of flight, and the origin of the American Pacific Northwest, history is shared at four unique sites. Discover stories of transition, settlement, conflict, and community."
        },
        new Park
        {
            ParkCode = "iafl",
            IsStatePark = false,
            StateCode = "WA",
            FullName = "Ice Age Floods National Geologic Trail",
            Description = "At the end of the last Ice Age, 18,000 to 15,000 years ago, an ice dam in northern Idaho created Glacial Lake Missoula stretching 3,000 square miles around Missoula, Montana. The dam burst and released flood waters across Washington, down the Columbia River into Oregon before reaching the Pacific Ocean. The Ice Age Floods forever changed the lives and landscape of the Pacific Northwest."
        },
        new Park
        {
            ParkCode = "klse",
            IsStatePark = false,
            StateCode = "WA",
            FullName = "Klondike Gold Rush - Seattle Unit National Historical Park",
            Description = "Seattle flourished during and after the Klondike Gold Rush. Merchants supplied people from around the world passing through this port city on their way to a remarkable adventure in the Yukon Territory of Canada. Today, the park is your gateway to learn about the Klondike Gold Rush, explore the area's public lands, and engage with the local community."
        },
        new Park
        {
            ParkCode = "laro",
            IsStatePark = false,
            StateCode = "WA",
            FullName = "Lake Roosevelt National Recreation Area",
            Description = "The ancient geologic landscape of the upper Columbia River cradles Lake Roosevelt in walls of stone carved by massive ice age floods. Come explore the shorelines and learn the stories of American Indians, traders and trappers, settlers and dam builders who called this place home. Swim, boat, hike, camp, and fish at this hidden gem in Northeast Washington, created by the Grand Coulee Dam."
        },
        new Park
        {
            ParkCode = "lecl",
            IsStatePark = false,
            StateCode = "WA",
            FullName = "Lewis & Clark National Historic Trail",
            Description = "The Lewis and Clark National Historic Trail winds nearly 4,900 miles through the homelands of more than 60 Tribal nations. It follows the historic outbound and inbound routes of the Lewis and Clark Expedition of 1803-1806 from Pittsburgh, Pennsylvania to the Pacific Ocean. Follow the trail to find the people, places, and stories that make up the complex legacy of the expedition."
        },
        new Park
        {
            ParkCode = "lewi",
            IsStatePark = false,
            StateCode = "OR,WA",
            FullName = "Lewis and Clark National Historical Park",
            Description = "Explore the timeless rainforests and majestic coastal vistas. Discover the rich heritage of the native people. Unfold the dramatic stories of America's most famous explorers. The park encompasses sites along the Columbia River and the Pacific Coast. Follow in the footsteps of the explorers and have an adventure in history."
        },
        new Park
        {
            ParkCode = "mapr",
            IsStatePark = false,
            StateCode = "WA",
            FullName = "Manhattan Project National Historical Park",
            Description = "The Manhattan Project is one of the most transformative events of the 20th century. It ushered in the nuclear age with the development of the world’s first atomic bombs. The building of atomic weapons began in 1942 in three secret communities across the nation. As World War II waned in 1945, the United States dropped the atomic bombs on Hiroshima and Nagasaki, Japan—forever changing the world."
        },
        new Park
        {
            ParkCode = "miin",
            IsStatePark = false,
            StateCode = "ID,WA",
            FullName = "Minidoka National Historic Site",
            Description = "During World War II, over 120,000 people of Japanese ancestry were forcibly removed from their homes and incarcerated without due process of law. Although little remains of the barbed-wire fences and tar-papered barracks, the Minidoka concentration camp once held over 13,000 Japanese Americans in the Idaho desert. Minidoka preserves their legacy and teaches the importance of civil liberties."
        },
        new Park
        {
            ParkCode = "mora",
            IsStatePark = false,
            StateCode = "WA",
            FullName = "Mount Rainier National Park",
            Description = "Ascending to 14,410 feet above sea level, Mount Rainier stands as an icon in the Washington landscape. An active volcano, Mount Rainier is the most glaciated peak in the contiguous U.S.A., spawning five major rivers. Subalpine wildflower meadows ring the icy volcano while ancient forest cloaks Mount Rainier’s lower slopes. Wildlife abounds in the park’s ecosystems. A lifetime of discovery awaits."
        },
        new Park
        {
            ParkCode = "rafa",
            IsStatePark = true,
            StateCode = "WA",
            FullName = "Rainbow Falls State Park",
            Description = "A rare cluster of old-growth trees creates an inspiring shadow to stand in. Rainbow Falls State Park, 16 miles west of Chehalis, is a walker’s paradise. Miles of loamy trails wind through the forest and along the river, to a small cascade that throws rainbows of spray at the sun.Tree lovers, maybe you’d rather see towering Douglas - fir, western hemlock and western red cedar trees from your bike or the back of your steed.Many of the park trails are mixed - use, and if you need more, take to the nearby Willapa Hills State Park Trail, a rails - to - trails fixture that goes over trestles and bridges, through forest, farmland and tiny towns."
        },
        new Park
        {
            ParkCode = "rckp",
            IsStatePark = true,
            StateCode = "WA",
            FullName = "Rockport State Park",
            Description = "Find your inner forest spirit among the rare old-growth stands of Rockport State Park. The park's ancient trees, having never been logged, form a landscape and ecosystem seldom seen nowadays, a canopy of towering evergreens so dense that minimal sunlight shines through. Breathe in the crisp smell of conifers and feel the earth beneath your feet, then look up and marvel at the Rockport giants, some more than 250 feet tall. Check out the Discovery Center, which is open most weekends throughout the year, and ask about guided ranger walks."
        },
        new Park
        {
            ParkCode = "spsp",
            IsStatePark = true,
            StateCode = "WA",
            FullName = "Spencer Spit State Park",
            Description = "Looking for an island camping park you can get to by car? Spencer Spit, on Lopez Island, may be just what you had in mind. One of the few auto-access state parks in the San Juan Islands, Spencer Spit provides dramatic, east-facing views of Decatur and Blakely islands and features a rare sand spit enclosed by a salt-chuck lagoon. The park offers crabbing, clamming, saltwater fishing, swimming, diving, bird and wildlife viewing and 2 miles of hiking trails. Families visiting this kid-friendly park between July 5 and Labor Day can enjoy a Junior Ranger interpretive program. The park, which is on the Cascadia Marine Trail, caters to boaters off all kinds. Amenities include kayak rentals and moorage for those who prefer to sleep in the comfort of their vessels, as well as large private campsites and primitive sites for hikers, bikers and kayakers."
        },
        new Park
        {
            ParkCode = "cadi",
            IsStatePark = true,
            StateCode = "WA",
            FullName = "Cape Disappointment State Park",
            Description = "Lighthouses stand sentinel atop windswept cliffs, sea smells waft up through the air and waves collide with a crash where the Pacific Ocean meets the Columbia River below. Named for Captain John Meares’ first thwarted voyage to find the Columbia, Cape Disappointment is steeped in Northwest history. This is the place to explore U.S. military and maritime legacies and to experience the story of Lewis & Clark and the effect of their Corps of Discovery Expedition on Native American tribes."
        },
        new Park
        {
            ParkCode = "cutt",
            IsStatePark = true,
            StateCode = "WA",
            FullName = "Cutts Island State Park",
            Description = "Cutts Island Marine State Park is proof that good things come in small packages. One-half mile from Kopachuck State Park, this intriguing little island offers the perfect day at the beach. This park can only be reached by water, so canoeists, kayakers and stand up paddlers mingle on the sandy beach with boaters, divers and seals (the latter of which should be given a very wide berth). "
        },
        new Park
        {
            ParkCode = "ikki",
            IsStatePark = true,
            StateCode = "WA",
            FullName = "Ike Kinswa State Park",
            Description = "Ike Kinswa State Park is a great place to make summer memories. This classic state park sits on Mayfield Lake amidst a rolling patchwork of farmland 20 miles east of Interstate 5 and off U.S. Highway 12. Ike Kinswa provides idyllic days of boating, swimming and water sports for the whole family. Fish for tiger muskie, largemouth bass, kokanee and rainbow trout; launch the family boat for water sports or bring kayaks and paddleboards for a mellow float. Kids can splash in the swim area until the air turns cool, the daylight fades and the smell of barbecue draws them back to camp."
        },
        new Park
        {
            ParkCode = "larr",
            IsStatePark = true,
            StateCode = "WA",
            FullName = "Larrabee State Park",
            Description = "Set on the seaward side of Chuckanut Mountain near Bellingham, Larrabee State Park is known for its postcard views of Samish Bay and the San Juan Islands. It is also Washington's first state park. This unique camping park on famed Chuckanut Drive offers boating, paddling, fishing, shellfish harvesting, diving, teeming tide pools and perfect spots for quiet contemplation, child play or a romantic date."
        },
        new Park
        {
            ParkCode = "lebp",
            IsStatePark = true,
            StateCode = "WA",
            FullName = "Leadbetter Point State Park",
            Description = "Have you ever seen a cotton ball hopping around on the sand? You will see all kinds of birds at Leadbetter Point State Park on the upper Long Beach Peninsula, between Willapa Bay and the Pacific Ocean. So, bring out the binocs, field guide, tripod and camera and scan the horizon for eagles, peregrine falcons, brown pelicans, terns and ducks. The park's forest is also alive with feathered friends - western tanager, warblers and black-headed grosbeak in summer, and chickadees, kinglets and thrushes year round."
        },
        new Park
        {
            ParkCode = "mybm",
            IsStatePark = true,
            StateCode = "WA",
            FullName = "Mystery Bay Marine State Park",
            Description = "All but hidden on the west side of Marrowstone Island, Mystery Bay State Park offers a great escape for visitors arriving by land or by water. Boaters can dock or lay anchor and stroll along the grassy and gravelly shoreline or enjoy the inlet's pristine waters. Take along a kayak or paddleboard, shellfish harvesting buckets (and permits), or scuba gear for a day of recreation. Take in views of the Olympic Mountains from one of six unsheltered, first-come, first-served picnic tables. Cook up those oysters and clams in one of three fire rings, (fires allowed during daylight hours) and enjoy an island sunset before heading to your boat or to your campsite at nearby Fort Flagler State Park."
        },
        new Park
        {
            ParkCode = "obpa",
            IsStatePark = true,
            StateCode = "WA",
            FullName = "Obstruction Pass State Park",
            Description = "Obstruction Pass State Park is one of the few public beaches on famed Orcas Island. Though most people flock to its bigger neighbor, Moran State Park, this property's quiet beauty is unsurpassed. Opal waters lap at pebbly beaches, and madrone trees cling to bluffs. Rocky viewpoints entice picnickers, birders, lovebirds and youthful explorers."
        },
        new Park
        {
            ParkCode = "pfsp",
            IsStatePark = true,
            StateCode = "WA",
            FullName = "Palouse Falls State Park Heritage Site",
            Description = "The Palouse River runs through a narrow cataract and drops 200 feet to a churning bowl. From there, the current moves swiftly, through a winding gorge of columnar basalt, to its southern end at the mighty Snake River. All Washingtonians, visitors to the region and Ice Age floods fans should see Palouse Falls State Park at least once in their lifetime. Carved more than 13,000 years ago, Palouse Falls is among the last active waterfalls on the Ice Age floods path."
        }
    };
}