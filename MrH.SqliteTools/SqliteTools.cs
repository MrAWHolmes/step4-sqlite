//file : MrH.SqliteTools\SqliteTools.cs

//using System.Data.SQLite; <-- Problematic with c# .. go figure!
using System.Data;
using Microsoft.Data.Sqlite;     // ADD this
using Microsoft.VisualBasic;
using MrH.Console.Tools;


namespace MrH.SqliteTools
{
    
    public static class SqliteTools
    {

        //force crash on any Sqlite Error - for strict debugging!
        public  static  bool FailOnSqlErrors = false;
        
        public static SqliteConnection GetConn(string dbFileName,SqliteOpenMode OpenMode=SqliteOpenMode.ReadWriteCreate)
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
                
                System.Console.WriteLine("SQLite error while opening the existing database:");
                System.Console.WriteLine(ex.Message);
                System.Console.WriteLine("Check that the file isn't corrupted and the connection string is correct.");

                throw;

            }//catch
        }//GetMSLConnect        

        public static void RunNonParamNonQuery(string SqlQuery)
        {
                        //debugging flag
            bool FailOnSqlErrors = false;
            
            var Conn = SqliteTools.GetConn("xanthium.db",SqliteOpenMode.ReadWrite);
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
        // See Co-Pilot-help.MD file
        
    public static void RunParamNonQuery(string SqlQuery,params SqliteParameter[] Parameters)
        //co-pilot : params params SqliteParameter[] Parameters) <-- parameter
        {   bool Success = false;
            //protect against exlicit null calue to Parameters
            Parameters ??= Array.Empty<SqliteParameter>(); //Ensure null -> {}
            
            //System.Console.WriteLine("Connection to 'xanthium.db'...");
            var Conn = SqliteTools.GetConn("xanthium.db",SqliteOpenMode.ReadWrite);
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
                            System.Console.WriteLine($"Parameter {p.ParameterName} bound to {p.Value}");
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
        


    }//class SqliteTools
}//namespace MrH.SqliteTools