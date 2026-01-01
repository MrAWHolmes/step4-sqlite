//file : GameDbDemo\Program.cs

//using System.Data.SQLite; <-- Problematic with c# .. go figure!
using Microsoft.Data.Sqlite;     // ADD this
using Microsoft.VisualBasic;
using MrH.Console.Tools;

namespace sqlitecrud
{
    class SqliteCRUDOps
    {
        Dictionary<string,string> SqlQueries ; 

        public static void BuildSqlQueries(){
            This.SqlQueries = new Dictionary<string,string>();

            try
            {
                BuildSqlQueries.Add("CreateXanthiumTable",
                                     @"CREATE TABLE IF NOT EXISTS Customers(
                                      Id INTEGER PRIMARY KEY)

                                     "
                                    );

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
           Test_GetMSLConnect();

           UsingStatmenOpentDemo(runme:false); // crash as db does not exist and cant create as in 'ReadWrite' mode!

           Sql


           //adapte from Hello, World
           // REF :: https://github.com/dotnet/docs/blob/main/samples/snippets/standard/data/sqlite/HelloWorldSample/Program.cs
           
           
            
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

        static void UsingStatmentDemo()
        {
            System.Console.WriteLine("Connection to 'xanthium.db'...");
            var Conn = GetMSLConnect("xanthium.db",SqliteOpenMode.ReadWrite);
            // using closes when statement ends

            
            using (Conn)
            {
                Conn.Open();
            }//using
        }//UsingStatmentDemo

         static void CreateTableWithUsing(string SqlQuery)
        {
            System.Console.WriteLine("Connection to 'xanthium.db'...");
            var Conn = GetMSLConnect("xanthium.db",SqliteOpenMode.ReadWrite);
            // using closes when statement ends
            using (Conn)
            {
                Conn.Open();
            }//using
        }//CreateTableWithUsing


        static void CreateAndSeed()
        {
            
        }
    }//class SqliteCRUDOps
    
    
    
}//namespace