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