# Parks Lookup

By: Stella Marie

## Technologies Used
- C# 12
- ASP.NET Core 7
  - EntityFrameworkCore
  - MySQL

## Description

Swagger: http://localhost:5006/swagger/
+ http://localhost:5006/swagger/v1/swagger.json

ParksLookup is a web api for retrieving and submitting information about state and national parks.

**Active port:** http://localhost:5006/api/v1/

### Account (authentication)
| Method    | URL format            | Action                                |
| --------- | --------------------- | ------------------------------------- |
| GET       | .../account/          | Depends on purpose                    |
| GET       | .../account/{id}      | Returns user information              |
| POST      | .../account/          | Create a confirmed user (req. auth)   |
| PUT       | .../account/{id}      | Update user information (req. auth)   |
| DELETE    | .../account/{id}      | Delete user account (req. auth)       |
| Extensions                                                                |
| GET       | .../account/seed/     | Confirm user (req. auth)              |
| POST      | .../account/seed/     | Create park and unconfirmed account   |
| POST      | .../account/register/ | Create new unconfirmed account        |
| POST      | .../account/login/    | Log into account                      |
| DELETE    | .../account/login/    | Log out of account                    |

By default, accounts are unconfirmed unless created by another user. It is assumed that users are park employees. Until a user has been confirmed with GET .../account/seed, they cannot create, update or delete any parks or visitor centers. Note: This is a development hack.

**Queries for: .../account?**

Parameter: purpose
Required - Supported values: seed, lookup
- seed: Seeds database with two sample parks and visitor centers
- lookup: Returns a sorted list of users (req. signin and confirmed account)

Parameter: name
Not required - Returns users whose name contains the searched name

Parameter: username
Not required - Returns users whose username contains the search term

Parameter: parkid
Not required - Return users working for a specified park

Parameter: pageSize
Not required - Returns sorted list of users sectioned by given number (default: 10)

Parameter: pageIndex
Not required - Returns section of sorted list (default: 1)

### Parks
| Method    | URL format        | Action                        |
| --------- | ----------------- | ----------------------------- |
| GET       | .../parks/        | Return list of all parks      |
| GET       | .../parks/{id}    | Return park information       |
| POST      | .../parks/        | Create new park (req. auth)   |
| PUT       | .../parks/{id}    | Update park info (req. auth)  |
| DELETE    | .../parks/{id}    | Delete park (req. auth)       |

To create, update or delete park information requires a confirmed user account. Only an affiliated user can update or delete park information. Every account can only be affiliated with one park.

**Queries for: .../parks?**

Parameter: name
Not required - Returns parks containing the search term in their name

Parameter: state
Not required - Returns parks located in the given state

### Visitor Centers
| Method    | URL format                | Action                            |
| --------- | ------------------------- | --------------------------------- |
| GET       | .../visitorcenters/       | Return list of visitor centers    |
| GET       | .../visitorcenters/{id}   | Return visitor center information |
| POST      | .../visitorcenters/       | Create new center (req. auth)     |
| PUT       | .../visitorcenters/{id}   | Update center info (req. auth)    |
| DELETE    | .../visitorcenters/{id}   | Delete park (req. auth)           |

To create, update or delete visitor center information requires a confirmed user account. Only an affiliated user can update or delete visitor center information. Visitor Centers can only be affiliated with one park.

**Queries for: .../visitorcenters?**

Parameter: name
Not required - Returns visitor centers containing the search term in their name

Parameter: park
Not required - Returns visitor centers at parks containing the search term in their name

<br>

## Complete Setup

This app requires use of a database. It is suggested to use migrations for ensuring the smooth setup of Identity.

### Database Schemas

To setup the database, follow the directions in [Connecting the Database](#connecting-the-database) and then implement migrations as detailed in [Migrations](#migrations).

### Connecting the Database

In your IDE:
- Create a file in the root of the assembly: appsettings.json
  - Do not remove the mention of this file from .gitignore
- Add this code:

```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=[hostname];Port=[port_number];database=[database_name];uid=[username];pwd=[password]"
    }
}
```

Replace the values accordingly and brackets are not to be included.

### Migrations

- In the terminal, run ```dotnet build --project AssemblyName```
  - Or navigate into the assembly subdirectory of the project and run ```dotnet build```

This command will install the necessary dependencies. For it to work though, appsettings.json should already be setup. As migrations are included in the clone or fork, you should only need to run:

```dotnet ef database update```

However, since the models are already set up, if update does not work, then do:

```bash
dotnet ef migrations add Initial
dotnet ef database update
```

### Run the App

Once you have a database setup and the connection string included in the appsettings.json, you can run the app:

- Navigate to main page of repo
- Either fork or clone project to local directory
- Bash or Terminal: ```dotnet run --project AssemblyName```
  - If you navigate into the main assembly, use ```dotnet run```

If the app does not launch in the browser:
- Run the app
- Open a browser
- Enter the url: https://localhost:5006/

## Known Bugs

Please report any issues in using the app.

## License

[MIT](https://choosealicense.com/licenses/mit/)