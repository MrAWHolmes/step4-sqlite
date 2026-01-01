
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
                                     @"INSERT INTO Employees(Name,DOB,Email,Price) 
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


         public static void Main(string[] Args)
        {
           //Run Tests.. first attempt
           
           UsingStatmenOpentDemo(runme:false); // crash as db does not exist and cant create as in 'ReadWrite' mode!

                     //adapte from Hello, World
           // REF :: https://github.com/dotnet/docs/blob/main/samples/snippets/standard/data/sqlite/HelloWorldSample/Program.cs
           
           RunNonParamNonQuery(SqlQueries["QCreateTableEmployees"]);
            
        }// main
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
                System.Console.WriteLine("Success! query executed:");
                System.Console.WriteLine(command.CommandText);
            }//using Conn
        }//RunNonParamNonQuery
   }//class SqliteCRUDOps
    
    
    
}//namespace
```

PS C:\Users\holmesa\vscode\raylib\step4-sqlite>  & 'c:\Users\holmesa\.vscode\extensions\ms-dotnettools.csharp-2.110.4-win32-x64\.debugger\x86_64\vsdbg.exe' '--interpreter=vscode' '--connection=7973e1c39d114bed9f3a681c53f35ee6' 
Unhandled exception. System.Collections.Generic.KeyNotFoundException: The given key 'QCreateTableEmployees' was not present in the dictionary.
   at System.Collections.Generic.Dictionary`2.get_Item(TKey key)
   at sqlitecrud.SqliteCRUDOps.Main(String[] Args) in C:\Users\holmesa\vscode\raylib\step4-sqlite\GameDbDemo\Program.cs:line 136
PS C:\Users\holmesa\vscode\raylib\step4-sqlite> 
