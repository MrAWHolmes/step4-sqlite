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

Vital to make sure the wierd file management works ALWAYS:

Copilot:

Option A — Automatically copy data on build (recommended)
Tell MSBuild to include everything under data\ and copy it to your output folder on every build. This is hands‑off after you set it once.
GameDbDemo.csproj:
```XML

<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <!-- Copy everything under data\ to the output (bin/…/data) automatically -->
  <ItemGroup>
    <!-- Use Content to be explicit that these are runtime files -->
    <Content Include="data\**\*">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

</Project>
```
Use PreserveNewest for fast builds, or Always if you want to force-copy every build.

```cs

using System;
using System.IO;

class Program
{
    static void Main()
    {
        // Points to the folder where the app is running (bin/... or publish folder)
        string baseDir = AppContext.BaseDirectory;
        string dataDir = Path.Combine(baseDir, "data");

        Directory.CreateDirectory(dataDir); // no-op if exists
        Console.WriteLine($"Data directory: {dataDir}");

        // Example usage:
        string filePath = Path.Combine(dataDir, "example.txt");
        if (!File.Exists(filePath))
            File.WriteAllText(filePath, "Hello from ./data copied automatically!");

        Console.WriteLine(File.ReadAllText(filePath));
    }
}


```
Copilot: Fixing Errors

You’ve got two “copies” of the same code being pulled into the build:

A project named MrH.Console.Tools in your solution (MrH.Console.Tools.csproj), and
The same source files (e.g., Contools.cs) physically included inside the GameDbDemo project (under GameDbDemo\MrH.Console.Tools\…)

That’s why you see CS0436—the compiler finds two types named MrH.Console.Tools.Contools: one from the referenced tools project assembly and one compiled directly into GameDbDemo.
Your .sln itself is fine—having a solution folder named “MrH.Console.Tools” plus a project with the same name is normal. The problem is the double compilation path inside GameDbDemo.

step4-sqlite/
  GameDbDemo/
    GameDbDemo.csproj
    Program.cs
    data/
  MrH.Console.Tools/
    MrH.Console.Tools.csproj
    Contools.cs

   Fix-Step1 Open GameDbDemo.csproj and remove the reference to the compiled library (one of these forms may exist):
    ```XML
    
<!-- Remove this if present -->
<ItemGroup>
  <Reference Include="MrH.Console.Tools" />
</ItemGroup>

<!-- Or remove this ProjectReference if you have source already in this project -->
<ItemGroup>
  <ProjectReference Include="..\MrH.Console.Tools\MrH.Console.Tools.csproj" />
</ItemGroup>

<!-- Or remove a PackageReference if you packaged it -->
<ItemGroup>
  <PackageReference Include="MrH.Console.Tools" Version="1.0.0" />
</ItemGroup>
```
Fix Step 2
If you previously copied a DLL manually into the project, remove the DLL from any lib/ folder and its <Reference> entry.


Fix step 3
Clean your build artifacts:
```shell

dotnet clean
rd /s /q bin obj   # PowerShell/CMD on Windows

```

Fix step 4
Rebuild:
```shell
dotnet build
```
After this, there will be only one Contools (your source file), and CS0436 will disappear.


PROBLEMS with SQLite version library and oh my - not straigtforward

Here are the most authoritative sources for the Microsoft.Data.Sqlite API documentation:

📘 Core Overview & Getting Started

Overview on Microsoft Learn
Covers installation, basic usage, and ADO.NET patterns (connections, commands, readers). Includes code snippets and provides links to deeper API docs. 
Overview – Microsoft.Data.Sqlite [learn.microsoft.com]
ref: https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/?tabs=net-cli 


🔗 DbConnection Class Reference

SqliteConnection Class (API Reference)
Details constructors, properties (ConnectionString, DataSource, ServerVersion, etc.), methods (Open(), BeginTransaction(), BackupDatabase(), etc.), and inheritance information. 
SqliteConnection Class – Microsoft Learn [learn.microsoft.com]
ref: https://learn.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite?view=msdata-sqlite-9.0.0 


🛠 Connection String Builder

SqliteConnectionStringBuilder Class (API Reference)
Defines connection string properties like DataSource, Cache, ForeignKeys, Mode, Pooling, and more. Includes usage patterns. 
SqliteConnectionStringBuilder – Microsoft Learn [learn.microsoft.com]

ref: https://learn.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqliteconnectionstringbuilder?view=msdata-sqlite-9.0.0 


🔄 Connection String Syntax & Options

dotnet/docs GitHub (Connection Strings)
Explains SQLite connection string keywords used by this provider: Data Source, Mode, Cache, FailIfMissing, etc. 
Connection String Options – dotnet/docs [github.com]

ref: https://github.com/dotnet/docs/blob/main/docs/standard/data/sqlite/connection-strings.md 


⚙ Native Provider & Bundle Info

SQLitePCLRaw.bundle_e_sqlite3 Documentation
Describes the native library used by Microsoft.Data.Sqlite (e_sqlite3). It includes full builds with extensions (FTS4/5, JSON1, R*Tree). 
Custom SQLite Versions & Bundles – Microsoft Learn [learn.microsoft.com], [nuget.org] [learn.microsoft.com]

Overview – Microsoft.Data.Sqlite (Learn)Setup and basic usageSqliteConnection Class ReferenceFull API referenceSqliteConnectionStringBuilder ClassConnection string helperConnection String Syntax (GitHub)Available optionsSQLitePCLRaw.bundle_e_sqlite3 & custom provider infoNative provider bundles 

rEF: https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/custom-versions?tabs=net-cli 



Failed Run!!!!
Connection to 'xanthium.db'...
filePath = C:\Users\holmesa\vscode\raylib\step4-sqlite\GameDbDemo\bin\Debug\net10.0\data\xanthium.db
Warning! C:\Users\holmesa\vscode\raylib\step4-sqlite\GameDbDemo\bin\Debug\net10.0\data\xanthium.db databse does not exist.
Create a new EMPTY database. (Y/N)?y
SQLite error while opening the existing database:
SQLite Error 14: 'unable to open database file'.
Check that the file isn't corrupted and the connection string is correct.
Unhandled exception. Microsoft.Data.Sqlite.SqliteException (0x80004005): SQLite Error 14: 'unable to open database file'.
   at Microsoft.Data.Sqlite.SqliteException.ThrowExceptionForRC(Int32 rc, sqlite3 db)
   at Microsoft.Data.Sqlite.SqliteConnectionInternal..ctor(SqliteConnectionStringBuilder connectionOptions, SqliteConnectionPool pool)
   at Microsoft.Data.Sqlite.SqliteConnectionPool.GetConnection()
   at Microsoft.Data.Sqlite.SqliteConnectionFactory.GetConnection(SqliteConnection outerConnection)
   at Microsoft.Data.Sqlite.SqliteConnection.Open()
   at sqlitecrud.SqliteCRUDOps.GetMSLConnect(String dbFileName) in C:\Users\holmesa\vscode\raylib\step4-sqlite\GameDbDemo\Program.cs:line 43   
   at sqlitecrud.SqliteCRUDOps.Test_GetMSLConnect() in C:\Users\holmesa\vscode\raylib\step4-sqlite\GameDbDemo\Program.cs:line 92
   at sqlitecrud.SqliteCRUDOps.Main(String[] Args) in C:\Users\holmesa\vscode\raylib\step4-sqlite\GameDbDemo\Program.cs:line 111

   CO-PILOT ANSWER!
   Hi Archibald — you’re very close.
SQLite Error 14: 'unable to open database file' with Microsoft.Data.Sqlite almost always means the directory for the database file doesn’t exist or the process lacks permission to create/write there.
In your run, the target path is:
C:\Users\holmesa\vscode\raylib\step4-sqlite\GameDbDemo\bin\Debug\net10.0\data\xanthium.db

If ...\net10.0\data does not exist at runtime, Microsoft.Data.Sqlite cannot create the file and will throw error 14. Opening the connection doesn’t create the parent folders, only the file itself. So you need to ensure the data directory exists before Conn.Open().

What to change
1) Ensure the directory exists (and is writable) before opening the connection
You can do this either in Contools.filePath or inside GetMSLConnect. Here are two safe options.
Option A — Fix in Contools.filePath (auto-create the directory)

Contools.cs
```cs

public static string filePath(string RelativePathFolder, string FileName)
{
    string baseDir = AppContext.BaseDirectory;
    string dataDir = Path.Combine(baseDir, RelativePathFolder);

    // Ensure the directory exists
    Directory.CreateDirectory(dataDir); // <-- absolutely BLOODY VItal!!!!

    string filePathStr = Path.Combine(dataDir, FileName);
    return filePathStr;
}
```

SUCCESS!!!

Program.cs thus far...
```cs
//file : GameDbDemo\Program.cs

//using System.Data.SQLite; <--
using Microsoft.Data.Sqlite;     // ADD this
using MrH.Console.Tools;

namespace sqlitecrud
{
    class SqliteCRUDOps
    {
        public static SqliteConnection GetMSLConnect(string dbFileName)
        {
            
            string filePath = MrH.Console.Tools.Contools.filePath("data",dbFileName);
            System.Console.WriteLine($"filePath = {filePath}");
            
            
            var csb = new SqliteConnectionStringBuilder()
            {
                DataSource = filePath,
                ForeignKeys = true,             // enforces FK constraints
                Mode = SqliteOpenMode.ReadWriteCreate

                
            };

            string ConnectionString = csb.ToString();


            SqliteConnection Conn;

            //if file does not exist WARN!
            if (!File.Exists(filePath))
            {   //force creation only if user responds Y/y
                if (MrH.Console.Tools.Contools.YesOrNo($"Warning! {filePath} databse does not exist.",
                                                       "Create a new EMPTY database.")) {
                
                    Conn  = new SqliteConnection(ConnectionString);
                    

                    //Open + Close creates the file
                    try{
                        Conn.Open();
                        Conn.Close(); 
                        return Conn;
                    }
                    catch (SqliteException ex)
                    {
                        
                        Console.WriteLine("SQLite error while opening the existing database:");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("Check that the file isn't corrupted and the connection string is correct.");

                        throw;

                    }
                                       
                }// then user said Yes
                else //do not create!
                {
                    System.Console.WriteLine("Critical Error! Database file not found!");
                    System.Console.WriteLine($"Ensure the path {filePath} is correct.");
                    System.Console.WriteLine($"Ensure the file {dbFileName} is in the above path!");

                    System.Console.WriteLine();
                    System.Console.WriteLine("Press any key to exit...");
                    System.Console.ReadKey();

                    //graceful exit
                    //Error code 2 is file not found
                    Environment.Exit(2);

                    //unreachable, but yields a throw which satisfies compiler error
                    throw new OperationCanceledException("Process exited due to missing database.");

                }// User said N so gracefull exit ELSE
            }  
                            //file exists - open and close?       
                
                Conn  = new SqliteConnection(ConnectionString);

                //Open + Close creates the file?
                Conn.Open();
                Conn.Close(); 

                return Conn;
        }//GetMSLConnect        

        public static void Test_GetMSLConnect()
        {
            System.Console.WriteLine("Connection to 'xanthium.db'...");
            var Conn = GetMSLConnect("xanthium.db");

            System.Console.WriteLine($"Conn is {Conn}");

            System.Console.WriteLine($"Conn.State : {Conn.State}");

            System.Console.WriteLine("Using Conn to perform Conn.Open()");
            Conn.Open();
            System.Console.WriteLine($"Conn.State : {Conn.State}");

            System.Console.WriteLine("Using Conn to perform Conn.Close()");
            Conn.Close();
            System.Console.WriteLine($"Conn.State : {Conn.State}");
            
        }

        public static void Main(string[] Args)
        {
           //Run Tests..
           Test_GetMSLConnect();
           
            
        }// main
    }//class SqliteCRUDOps
    
    
    
}//namespace

```

Updated Contools.cs
```cs

//file: GameDbDemo\MrH.Console.Tools\MrH.Console.Tools\Contools.cs

namespace MrH.Console.Tools;

public static class Contools
{
    public static bool YesOrNo(string Context,string Prompt)
    {
        System.Console.WriteLine(Context);
        ConsoleKeyInfo  Key ;

        while (true)
        {
            System.Console.Write(Prompt+" (Y/N)?");
            Key  = System.Console.ReadKey();

            //line spacing
            System.Console.WriteLine();

            if(Key.Key == ConsoleKey.Y) return true;
            if(Key.Key == ConsoleKey.N) return false;

        }//while
        
    }//YesOrNo

    public static string filePath(string RelativePathFolder,string FileName)
    {
        string baseDir = AppContext.BaseDirectory;
        string dataDir = Path.Combine(baseDir,RelativePathFolder);

        //ABSOLUTELY BLOODY VITAL!!!!
        
        // Ensure the directory exists

        Directory.CreateDirectory(dataDir); // <-- Absoluteletly BLOODY vital !!!!


        string filePathStr = Path.Combine(dataDir,FileName);

        return filePathStr;

    }//filePath

}//class Contools
```

Successful run:
```shell

Connection to 'xanthium.db'...
filePath = C:\Users\holmesa\vscode\raylib\step4-sqlite\GameDbDemo\bin\Debug\net10.0\data\xanthium.db
Warning! C:\Users\holmesa\vscode\raylib\step4-sqlite\GameDbDemo\bin\Debug\net10.0\data\xanthium.db databse does not exist.
Create a new EMPTY database. (Y/N)?Y
Conn is Microsoft.Data.Sqlite.SqliteConnection
Conn.State : Closed
Using Conn to perform Conn.Open()
Conn.State : Open
Using Conn to perform Conn.Close()
Conn.State : Closed
```

