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

NOTE on NUGETS!!!

SQLitePCLRaw provides convenience-based bundle packages, that make it easy to bring in the right dependencies across different platforms. The main Microsoft.Data.Sqlite package brings in SQLitePCLRaw.bundle_e_sqlite3 by default. To use a different bundle, install the Microsoft.Data.Sqlite.Core package instead along with the bundle package you want to use. Bundles are automatically initialized by Microsoft.Data.Sqlite.

// Dictionary Work
REF: https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2?view=net-10.0

Improvments to Openning the database - if it does not exist
Choosing creatioon mode with SqliteOpenMode parameter!

file: Program.cs - Method GetMSLConnect so it now
constrol the automatic databse creation through the 
SqliteOpenMode OpenMode parameter:

```cs
   public static SqliteConnection GetMSLConnect(string dbFileName,SqliteOpenMode OpenMode=SqliteOpenMode.ReadWriteCreate)
        {
            //SqliteConnection Conn;

            string filePath = MrH.Console.Tools.Contools.filePath("data",dbFileName );
            System.Console.WriteLine($"filePath = {filePath}");
            
            
            var csb = new SqliteConnectionStringBuilder()
            {
                DataSource = filePath,
                ForeignKeys = true,             // enforces FK constraints
                Mode = OpenMode
                
            };

            string ConnectionString = csb.ToString();

            var Conn  = new SqliteConnection(ConnectionString);
                                                  
            //Open + Close creates the file
            try{
                Conn.Open();
                Conn.Close(); 
                return Conn;
            }//try
            catch (SqliteException ex)
            {
                
                Console.WriteLine("SQLite error while opening the existing database:");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Check that the file isn't corrupted and the connection string is correct.");

                throw;

            }//catch
        }//GetMSLConnect        
```

Working with a Dictionary as a class attribute!
REF: https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2?view=net-10.0

Successefull Built and tested:

SqlQueries Dictionary

Successfully implmented BuildSqlQueries()

```cs

namespace sqlitecrud
{
    class SqliteCRUDOps
    {
        static Dictionary<string,string> SqlQueries = new Dictionary<string,string>();
        static bool FailOnSqlErrors = true;

        public static void BuildSqlQueries(){
            //SqlQueries = new Dictionary<string,string>();

            try
            {
                
                //REF : https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/types
                //ref : https://www.sqlitetutorial.net/sqlite-create-table/
                SqlQueries.Add("QCreateTableEmployees",
                                     @"CREATE TABLE IF NOT EXISTS Employees(
                                      Id INTEGER PRIMARY KEY,
                                      Name  TEXT NOT NULL,
                                      DOB   TEXT,
                                      Email TEXT UNIQUE NOT NULL,
                                      Salary REAL
                                     );"
                                    );

                //ref: https://www.sqlitetutorial.net/sqlite-csharp/insert/
                SqlQueries.Add("PQAddEmployee",
                                     @"INSERT INTO Employees(Name,DOB,Email,Salary) 
                                      VALUES ($Name,$DOB,$Email,$Salary);"
                                     
                                    );

                //ref: https://www.sqlitetutorial.net/sqlite-csharp/insert/
                SqlQueries.Add("PQUpdateEmployeeEmail",
                                     @"UPDATE Employees
                                       SET Email = $NewEmail
                                       WHERE Id = $Id;
                                " );
                SqlQueries.Add("PQUpdateEmployeeSalary",
                                     @"UPDATE Employees
                                       SET Salary = $NewSalary
                                       WHERE Id = $Id;
                                " );

                //ref: https://www.sqlitetutorial.net/sqlite-delete/
                SqlQueries.Add("PQDeleteEmployee",
                                     @"DELETE FROM  Employees
                                       WHERE Id = $Id;
                                " );


            }
            catch(Exception Ex)
            {
                System.Console.WriteLine("Caller BuildSqlQueries()");
                System.Console.WriteLine($"Dictionary Key Exception!");
                System.Console.WriteLine(Ex);
            }
        }

       : : :
```
Towards adapting 
```cs
    static void RunNonParamNonQuery(string SqlQuery)
```
to
```cs
 static void RunParamNonQuery(string SqlQuery,params SqliteParameter[] parameters)
```
Co-Pilot: Explaining 'params' keyword
What does params do in params SqliteParameter[] parameters?
params is a C# keyword that allows a method to accept a variable number of arguments as if they were a comma‑separated list at the call site. Under the hood, the parameter is still an array (SqliteParameter[]); params simply gives you syntactic sugar so callers don’t have to explicitly create an array.

Type (what it is): SqliteParameter[] named parameters
Effect of params: Callers can pass zero, one, or many SqliteParameter values without wrapping them in an array.

Why is it useful here?
When building SQL commands, it’s convenient to pass any number of parameters inline:
```cs

RunParamNonQuery(
    "INSERT INTO files(name, content) VALUES(@name, @content)",
    new SqliteParameter("@name", "readme.txt"),
    new SqliteParameter("@content", File.ReadAllBytes("readme.txt"))
);

```
Without params, you would have to write:
```cs

RunParamNonQuery(
    "INSERT INTO files(name, content) VALUES(@name, @content)",
    new SqliteParameter[]
    {
        new SqliteParameter("@name", "readme.txt"),
        new SqliteParameter("@content", File.ReadAllBytes("readme.txt"))
    }
);

```
Both compile to the same thing. params just makes the call site more ergonomic.

Key rules and behaviors of params

1) Must be the last parameter in the method signature.
2) Only one params parameter per method.
3) You can’t have multiple params parameters.


Caller can pass:

i)No arguments (the array becomes empty):
```cs
ParamNonQuery("DELETE FROM t WHERE 1=1"); // parameters.Length == 0
```
ii) A single argument:
```cs
 RunParamNonQuery("DELETE FROM t WHERE id=@id", new SqliteParameter("@id", 42));
```
iii) Many arguments (comma‑separated):
```cs

RunParamNonQuery("UPDATE t SET a=@a, b=@b WHERE id=@id",
    new SqliteParameter("@a", 1),
    new SqliteParameter("@b", 2),
    new SqliteParameter("@id", 99));

```
iv) An explicit array (if they prefer):
```cs

var ps = new[] { new SqliteParameter("@x", 10), new SqliteParameter("@y", 20) };
RunParamNonQuery("UPDATE t SET x=@x, y=@y", ps);

```
v) Null handling: If the caller passes null as the entire array, the runtime will pass parameters == null.
If they omit arguments, you get an empty array. In your method, you can defensively do:
```cs
    parameters ??= Array.Empty<SqliteParameter>();
```
What it does: If the left-hand side is null, assign the right-hand side to it; otherwise do nothing.
In words: “If parameters is null, set it to Array.Empty<SqliteParameter>(); else leave parameters unchanged.”
Introduced: C# 8.0.

Herewith co-pilot conversation and final code 01/01/2026
//AI Ref: Copilot  static void RunParamNonQuery(string SqlQuery,params SqliteParameter[] Parameters)
        /*
        Prompt: Im trying to abstract this c# ADO SQlite example https://www.sqlitetutorial.net/sqlite-csharp/insert/I 
        // I have coded this method for Runing NonParam Queries:  static void RunNonParamNonQuery(string SqlQuery)

        Code to self coded RunNonParamNonQuery(string SqlQuery) uploaded

         // that will in-order traverse the Params which will be a container of possible mixed types.
         // In python I could use a list.What data type will do this role in c#.
         // Please explain and justify the data type give examples of how to declare, 
         // initialise and traverse the Data Type.
         // 
         // I have a working knowledge of c# Arrays and Lists of primitive type.


         3 Code options were generated - I decided on Option C

         Option C: params SqliteParameter[] (maximum control)
        Pros

            You can set DbType, size, precision/scale, and even SqliteType explicitly.
            Best when schema and types matter (e.g., blobs, decimals).

        Cons

            Slightly heavier to write.

        code generated 

```cs

static void RunParamNonQuery(string sql, params SqliteParameter[] parameters)
{
    using var conn = GetMSLConnect("xanthium.db", SqliteOpenMode.ReadWrite);
    Console.WriteLine("Running Query:");
    Console.WriteLine(sql);

    try
    {
        conn.Open();
        using var cmd = new SqliteCommand(sql, conn);

        foreach (var p in parameters)                       <--
        {                                                   <--
            // Ensure null => DBNull                        <--
            if (p.Value is null) p.Value = DBNull.Value;    <--
            cmd.Parameters.Add(p);                          <--
        }                                                   <--

        cmd.ExecuteNonQuery();
        Console.WriteLine("Success! query executed:");
        Console.WriteLine(cmd.CommandText);
    }
    catch (Exception ex)
    {
        Console.WriteLine(@"/!\ Failure executing query:");
        Console.WriteLine(sql);
        Console.WriteLine(ex);
        if (FailOnSqlErrors) throw;
    }
}


```
        Code Utilised show with <--

        References::

        SqliteParameter API and purpose: Microsoft Learn API Browser. [learn.microsoft.com]
        https://learn.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqliteparameter?view=msdata-sqlite-9.0.0

        Parameter usage, prefixes, type control: Microsoft Learn – Parameters. [learn.microsoft.com]
        https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/parameters 

        ADO.NET usage pattern with commands/parameters: Microsoft Learn – Overview. [learn.microsoft.com]
        https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/?tabs=net-cli 

        ExecuteNonQuery behavior: Microsoft Learn – API. [learn.microsoft.com]
        https://learn.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqlitecommand.executenonquery?view=msdata-sqlite-9.0.0

        params semantics: C# language reference. [learn.microsoft.com]
        https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/method-parameters 

        */

My final code for this Method : static void RunParamNonQuery(string SqlQuery,params SqliteParameter[] Parameters)
```cs
static void RunParamNonQuery(string SqlQuery,params SqliteParameter[] Parameters)
static void RunParamNonQuery(string SqlQuery,params SqliteParameter[] Parameters)
        //co-pilot : params params SqliteParameter[] Parameters) <-- parameter
        {
            //protect against exlicit null calue to Parameters
            Parameters ??= Array.Empty<SqliteParameter>(); //Ensure null -> {}
            
            //System.Console.WriteLine("Connection to 'xanthium.db'...");
            var Conn = GetMSLConnect("xanthium.db",SqliteOpenMode.ReadWrite);
            System.Console.WriteLine("Running Query:");
            System.Console.WriteLine(SqlQuery);
            
            // using closes when statement ends

            //sanity check - Params.Count == "@" count in SqlQuery String

            int sqlParamCount = 0;
            for (int i = 0; i < SqlQuery.Length; i++)
            {
                if (SqlQuery[i]=='@') sqlParamCount++;
            }

            System.Console.WriteLine($"sqlParamCount : {sqlParamCount}");
            System.Console.WriteLine($"Parameters.Count : {Parameters.Length}");

            if (sqlParamCount != Parameters.Length)
            {
                System.Console.WriteLine(@"/!\ Warning possible parameter count mismatch.");
            }



            using (Conn)
            {
                Conn.Open();
                var command = new SqliteCommand(SqlQuery,Conn); //Assume Parameters are @ prefixed
                //create command
                try{
                    using (command)
                    {   // Bind the paramters!
                        // We assume validation is undertaken by the caller
                        foreach(var p in Parameters)                        // <-- start co-pilot code clip 1/01/2026
                        {                           
                            //map p.Value == null -> DBNull.Value
                            p.Value ??= DBNull.Value;                       
                            command.Parameters.Add(p);
                        }                                                   // <-- end co-pilot code clip 1/01/2026


                        command.ExecuteNonQuery();
                    }//using command
                }catch(Exception Ex)
                {
                    System.Console.WriteLine(@"/!\Faiilure! executing query:");
                    System.Console.WriteLine(command.CommandText);
                    System.Console.WriteLine(Ex);
                    
                    if (FailOnSqlErrors) throw;
                }
                System.Console.WriteLine(" ... Success! Query was executed successfully.");
                //System.Console.WriteLine(command.CommandText);
            }//using Conn
        }//RunParamNonQuery
```

Testing with insert Employee query:

Stuck - CO-Pilot Prompt
```quote
Im trying to create an array of employees, that has same field names as a database table, which can be traversed/iterated over and passed to an Insert Into SQL query which will be called using 

RunParamNonQuery(SqlQuey["PQAddEmployee"],record);

How do I construct a suitable data structure for this. record must essentially be an array of 
 SqliteParameter s

```

1) Model matching your table
```cs


public record Employee(
    string Name,
    string? DobIso,     // store as ISO string "YYYY-MM-DD" or null
    string Email,
    decimal Salary
);

```
I Kept DOB instead of DobIso

I added this to the file Models.cs with a namespace of 

2) Mapper: Employee → SqliteParameter[]
Make sure the parameter names match those in your query text. From your earlier dictionary, the insert uses $Name, $DOB, $Email, $Salary.
```cs

using Microsoft.Data.Sqlite;
using System;
using System.Data;

public static class EmployeeParamFactory
{
    public static SqliteParameter[] ToParameters(Employee e)
    {
        // Use `$` to match your SQL, or switch to `@` everywhere if you prefer.
        var pName   = new SqliteParameter("$Name",  e.Name);
        var pDob    = new SqliteParameter("$DOB",   (object?)e.DobIso ?? DBNull.Value);
        var pEmail  = new SqliteParameter("$Email", e.Email);

        // Salary mapping: set DbType explicitly if you want decimal semantics;
        // Microsoft.Data.Sqlite typically maps numeric to REAL/INTEGER.
        var pSalary = new SqliteParameter("$Salary", e.Salary) { DbType = DbType.Decimal };

        return new[] { pName, pDob, pEmail, pSalary };
    }
}


```

REFACTORING ReUSABLE Code into a LIBRARY:

1) Create a NEW Class PROJECT with a FOLDER Eg) .\MrH.SqlitTools
1b) Setup Nmaespace Eg) Mr.SqliteTools
1c) Add nugets to 
```xml
  <ItemGroup>
    <PackageReference Include="Microsoft.Data.Sqlite" Version="10.0.1" />
    <PackageReference Include="SQLitePCLRaw.bundle_e_sqlite3" Version="3.0.2" />
  </ItemGroup>
```

1d) Add MrH.Contools as a PROJECT REFERENCE
This adds
```XML
  
  <ItemGroup>
    <ProjectReference Include="..\..\MrH.Console.Tools\MrH.Console.Tools.csproj" />
  </ItemGroup>

```
Ceate public stats calss in .\MrH.SqliteTools\MrH.SqliteTools\SqliteTools.cs


In App add the name space "MrH.SqliteTools"
```cs
//using System.Data.SQLite; <-- Problematic with c# .. go figure!
using System.Data;
using Microsoft.Data.Sqlite;     // ADD this
using Microsoft.VisualBasic;
using MrH.Console.Tools;
using sqlitecrud.Models;
using MrH.SqliteTools; // <-- New Namespace :)


```

and change calls, Eg GetCon() --> MrH

I have run ouut opf time!

NOTED:: REsume here!!!!!!

Decouple Models.EmployeeToParameters()
from the very strict validation enforced to allow for 'easier' parameter substitution

Presently the Email validation is dropping Email as a parameter

It might still work with %@example.com% ??
