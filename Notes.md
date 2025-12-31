Awesome—let’s build a small C# (C# 10+) console app on modern .NET (works on .NET 6/7/8) that uses SQLite to manage a 3NF database with:

Players (entity)
Inventory (entity)
PlayerInventory (join table with a Qty field and proper foreign keys)

We’ll go step-by-step, including the NuGet installs, the schema, and C# code to create the DB/tables if not present, enforce referential integrity with ON DELETE CASCADE for players, and provide queries to add inventory items (plus some example CRUD).

As a Computer Science teacher, you’ll also see how we use Lists, Dictionaries, and Classes cleanly in the code.

#0) Prereqs

Install .NET SDK (6/7/8).

##0aCreate a new console project:

```powershell

dotnet new console -n GameDbDemo
cd GameDbDemo

```

##0b)Required NuGet packages
We’ll use Microsoft.Data.Sqlite (ADO.NET provider). It’s simple and perfect for small apps.

```powershell
dotnet add package Microsoft.Data.Sqlite

```

this is evident in the GameDbDemo\GameDbDemo.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Data.Sqlite" Version="10.0.1" />
  </ItemGroup>

</Project>
```
Copilot:
Why not EF Core? EF Core is great but adds complexity. For a quick tutorial focusing on SQL, Lists/Dictionaries, and referential integrity, ADO.NET with Microsoft.Data.Sqlite is straightforward.


#1)1) Data Model (3NF Design)
Third Normal Form (3NF) here:

Players(Id, Name, CreatedAt) — player details (no transitive dependencies).

Inventory(Id, Name, Description) — independent entity for items.

PlayerInventory(PlayerId, ItemId, Qty) — relationship only, with Qty dependent on the composite key (PlayerId, ItemId).

Copilot:
This prevents duplication and anomalies: attributes depend on keys, the whole keys, and nothing but the keys.

#2) Project Structure
Create three files:

Models.cs – classes for Players/Inventory/Join
Database.cs – DB helper class (create tables, CRUD)
Program.cs – app entry + demo flow

While searching how to structure this project I found 2 great tutorials:

REF 1 : https://www.xanthium.in/cross-platform-create-connect-update-sqlite3-database-using-csharp-dotnet-platform

REF 2 : https://www.xanthium.in/building-csharp-sqlite-gui-crud-applications-using-winforms-api-tutorial

Install System.Data.SQLite  (MS ADO one!)

GameDbDemo.csproj
```XML
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="System.Data.SQLite" Version="2.0.2" />
  </ItemGroup>

</Project>
```

