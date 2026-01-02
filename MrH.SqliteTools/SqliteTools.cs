//file : MrH.SqliteTools\SqliteTools.cs

//using System.Data.SQLite; <-- Problematic with c# .. go figure!
using System.Data;
using Microsoft.Data.Sqlite;     // ADD this
using Microsoft.VisualBasic;
using MrH.Console.Tools;
//using sqlitecrud.Models;

namespace MrH.SqliteTools
{
    public static class SqliteTools
    {
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



    }//class SqliteTools
}//namespace MrH.SqliteTools