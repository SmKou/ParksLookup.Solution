# Parks Lookup

By: Stella Marie

## Technologies Used
- C# 12
- ASP.NET Core 7
  - EntityFrameworkCore
  - Identity
  - MySQL

## Description

Swagger: http://localhost:5006/swagger/
+ http://localhost:5006/swagger/v1/swagger.json

ParksLookup is a web api for looking up state and national parks. Anyone can view park information and search through parks. Users can save parks to a list for quick access to.

Note: This project is not seeded. To test its functionality, first make a call to GET http://localhost:5006/api/v1/account/seed.

**Features Implemented**
- Versioning
- Pagination
- JWT Token Authentication

Please note that swagger documentation is not indicative of how to use this api. **Status:** Broken

**Active port:** http://localhost:5006/api/v1/

### Account (authentication)
| Method    | URL format                | Action                                |
| --------- | ------------------------- | ------------------------------------- |
| GET       | .../account/{username}    | Returns user information (req. auth)  |
| PUT       | .../account/{username}    | Update user information (req. auth)   |
| DELETE    | .../account/{username}    | Delete user account (req. auth)       |
| Extensions                                                                    |
| POST      | .../account/register/     | Create user account                   |
| POST      | .../account/login/        | Log into account                      |

For authorization, you will need an account and a token. In every request, provide the token in the header as the value of Bearer Token, and in the body, provide at least login credentials.

### Parks

**Note**: Users cannot create, update or delete parks. However, since the database isn't hosted, when setting up the project, there will be no data in the database. For demonstration purposes, make a call to GET .../account/seed to seed the database. If already seeded, it will return "Database already seeded".

| Method    | URL format                | Action                                |
| --------- | ------------------------- | ------------------------------------- |
| GET       | .../parks/                | Returns list of parks                 |
| GET       | .../parks/{parkcode}      | Returns park information              |


**Queries for: .../parks?**

Parameter: name
Not required - Returns list of parks containing the search term in their name

Parameter: state
Not required - Returns list of parks containing the state in their statecode

Parameter: type (state | national)
Not required - Returns list of parks either state or national

Parameter: sortorder
Not required - Returns sorted list of parks ordered by name

Parameter: pagesize
Not required - Returns listing of parks of a given size (default = 10)

Parameter: pageindex
Not required - Returns portion of listing of parks sectioned by pagesize (default = 1)

### Users (Authorization required)

You will need an account and a token to access the users api. Every request requires the JWT token in the header as the value of Bearer Token, and in the body, at least provide the login credentials.

| Method    | URL format                | Action                                |
| --------- | ------------------------- | ------------------------------------- |
| GET       | .../users/{username}      | Return list of parks user has saved   |
| POST      | .../users/                | Add park to user's list               |
| DELETE    | .../users/                | Delete park from user's list          |

**Queries for: .../users/{username}/**

Refer to Parks queries. Same parameters apply, except that the list is the user's list of parks.

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

Required:
- Database created with migrations
- Connection string in appsettings.json
- EnvironmentVariables.cs

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

- Swagger documentation

## License

[MIT](https://choosealicense.com/licenses/mit/)
