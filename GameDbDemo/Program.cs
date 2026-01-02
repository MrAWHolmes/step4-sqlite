//file : GameDbDemo\Program.cs

//using System.Data.SQLite; <-- Problematic with c# .. go figure!
using System.Data;
using Microsoft.Data.Sqlite;     // ADD this
using Microsoft.VisualBasic;
using MrH.Console.Tools;
using sqlitecrud.Models;

namespace sqlitecrud
{
    class SqliteCRUDOps
    {
        static Dictionary<string,string> SqlQueries = new Dictionary<string,string>();
        static bool FailOnSqlErrors = false;

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
                                      VALUES (@Name,@DOB,@Email,@Salary);"
                                     
                                    );

                //ref: https://www.sqlitetutorial.net/sqlite-csharp/insert/
                SqlQueries.Add("PQUpdateEmployeeEmail",
                                     @"UPDATE Employees
                                       SET Email = @NewEmail
                                       WHERE Id = @Id;
                                " );
                SqlQueries.Add("PQUpdateEmployeeSalary",
                                     @"UPDATE Employees
                                       SET Salary = @NewSalary
                                       WHERE Id = @Id;
                                " );

                //ref: https://www.sqlitetutorial.net/sqlite-delete/
                SqlQueries.Add("PQDeleteEmployee",
                                     @"DELETE FROM  Employees
                                       WHERE Id = @Id;
                                " );


            }
            catch(Exception Ex)
            {
                System.Console.WriteLine("Caller BuildSqlQueries()");
                System.Console.WriteLine($"Dictionary Key Exception!");
                System.Console.WriteLine(Ex);
            }
        }

        public static SqliteConnection GetMSLConnect(string dbFileName,SqliteOpenMode OpenMode=SqliteOpenMode.ReadWriteCreate)
        {
            //SqliteConnection Conn;

            string filePath = MrH.Console.Tools.Contools.filePath("data",dbFileName );
            System.Console.WriteLine($"Attempting to use database = {filePath}");
            
            
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
                System.Console.WriteLine($"Success! USing the databse = {filePath}");
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
           //Run Tests.. first attempt
           
           UsingStatmenOpentDemo(runme:false); // crash as db does not exist and cant create as in 'ReadWrite' mode!

                     //adapte from Hello, World
           // REF :: https://github.com/dotnet/docs/blob/main/samples/snippets/standard/data/sqlite/HelloWorldSample/Program.cs
           
            //important to Generate the Queries or nothin else will work !
           BuildSqlQueries();

           RunNonParamNonQuery(SqlQueries["QCreateTableEmployees"]);

           TestInsertEmployeesQuery(); // Also test RunParamterNonQuery(); and the Models.ParamFactory Method!
            
        }// main

        static void UsingStatmenOpentDemo(bool runme)
        {   if (!runme) return;
            System.Console.WriteLine("We try and open ''xanthium2.db' which dne in SqliteOpenMode = 'ReadWrite'");
            System.Console.WriteLine("This will throw an exception rather than force creation!'");

            System.Console.WriteLine("Connection to 'xanthium2.db'...");
            var Conn = GetMSLConnect("xanthium2.db",SqliteOpenMode.ReadWrite);
            // using closes when statement ends


            using (Conn)
            {
                Conn.Open();
            }//using
        }

        static void RunNonParamNonQuery(string SqlQuery)
        {
            //System.Console.WriteLine("Connection to 'xanthium.db'...");
            var Conn = GetMSLConnect("xanthium.db",SqliteOpenMode.ReadWrite);
            System.Console.WriteLine("Running Query:");
            System.Console.WriteLine(SqlQuery);
            
            // using closes when statement ends
            using (Conn)
            {
                Conn.Open();
                var command = new SqliteCommand(SqlQuery,Conn);
                //create command
                try{
                    using (command)
                    {
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
        }//RunNonParamNonQuery



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

        code generated ```

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


        ````
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
    static void RunParamNonQuery(string SqlQuery,params SqliteParameter[] Parameters)
        //co-pilot : params params SqliteParameter[] Parameters) <-- parameter
        {   bool Success = false;
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
                        Success = true;
                    }//using command
                }catch(Exception Ex)
                {
                    System.Console.WriteLine(@"/!\Faiilure! executing query:");
                    System.Console.WriteLine(command.CommandText);
                    System.Console.WriteLine(Ex);
                    
                    if (FailOnSqlErrors) throw;
                }
                
                if (Success) System.Console.WriteLine(" ... Success! Query was executed successfully.");
                
                //System.Console.WriteLine(command.CommandText);
            }//using Conn
        }//RunParamNonQuery
        

        static void TestInsertEmployeesQuery()
        {
                
            var employees = new[]
            {
                new Employee("Ada Lovelace",  "1815-12-10", "ada@example.com", 123_456.78m),
                new Employee("Grace Hopper",  "1906-12-09", "grace@example.com", 200_000m),
                new Employee("Alan Turing",   "1912-06-23", "alan@example.com", 150_000m),
                new Employee("Donald Knuth",  null,         "donald@example.com", 175_000m),
                new Employee("Bill Gates",  "1955-10-28", "bgates@microsoft.com", 123_456.78m),
                new Employee("Elon Musk",  "1971-06-28", "grace@example.com", 4_200_000m),
                new Employee("Geoff Bezos",   "1964-01-12", "alan@example.com", 3_150_000m),
                new Employee("Donald Trump", "1946-06-14" , "djtump@magatrump.com", 1_175_000m)
            };

            SqliteParameter[] p; //declare type wiothout initialisation!

            foreach (Employee e in employees){
                p = ParamFactory.ToParameters(e);
                RunParamNonQuery(SqlQueries["PQAddEmployee"],p);
            }//foreach
        }//TestInsertEmployeesQuery
        
    }//class SqliteCRUDOps
    
    
    
}//namespace