# MvcMusicStore
---
AWS Sample to modernize an old ASP.NET Framework 4.0 MVC e-commerce application.

![mvc-music-store](./static/images/music-store.png)

# License
This library is licensed under the Apache 2.0 License

# Pre-Requisites
1. Visual Studio 2019 16.4 or later with the ASP.NET and web development workload
2. Full .NET Framework 4.0 and 4.8
3. .NET Core 3.1 sdk or later.
4. SQL Server Management Studio
5. MS SQL Server (LocalDB version is perfectly suitable with appropriate connection string adjustments).
6. Git for Windows.

## Checking out source code.

In a directory of your choice, please run

```PowerShell
git clone https://github.com/aws-samples/Dotnet-modernization-music-store.git
```

## Set up Membership Schema and database to manage Users.

* Open Powershell and run the below script. This script will create a SQL Server database named "Identity" on your local machine to manage Users, Roles, and Claims using [Membership Provider](https://docs.microsoft.com/en-us/previous-versions/aspnet/tw292whz(v=vs.100)). (SQL Server is expected to be available at (local) address).

```PowerShell
& $env:WINDIR\Microsoft.Net\Framework\v4.0.30319\aspnet_regsql.exe -S . -d Identity -A all -E
```

## Running locally
Once you have the prerequisites installed on your local development machine you should be able to run the Music Store locally in your IDE. The Music Store uses SQL Server for the backend by default and is seeded with data on startup. You will need to either provision or have access to a database. 
