//file : GameDbDemo\Program.cs

//using System.Data.SQLite; <-- Problematic with c# .. go figure!
using System.Data;
using Microsoft.Data.Sqlite;     // ADD this
using Microsoft.VisualBasic;
using MrH.Console.Tools;
using sqlitecrud.Models;
using MrH.SqliteTools;

namespace sqlitecrud
{
    
    class SqliteCRUDOps
    {


        static Dictionary<string,string> SqlQueries = new Dictionary<string,string>();
        //static bool FailOnSqlErrors = false;

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
        }//GetConn

        
        public static void Test_GetConn()
        {
            System.Console.WriteLine("Connection to 'xanthium.db'...");
            var Conn = SqliteTools.GetConn("xanthium.db");

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

           SqliteTools.RunNonParamNonQuery(SqlQueries["QCreateTableEmployees"]);

           TestInsertEmployeesQuery(); // Also test RunParamterNonQuery(); and the Models.ParamFactory Method!
            
        }// main

        static void UsingStatmenOpentDemo(bool runme)
        {   if (!runme) return;
            System.Console.WriteLine("We try and open ''xanthium2.db' which dne in SqliteOpenMode = 'ReadWrite'");
            System.Console.WriteLine("This will throw an exception rather than force creation!'");

            System.Console.WriteLine("Connection to 'xanthium2.db'...");
            var Conn = SqliteTools.GetConn("xanthium2.db",SqliteOpenMode.ReadWrite);
            // using closes when statement ends


            using (Conn)
            {
                Conn.Open();
            }//using
        }

        

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
                SqliteTools.RunParamNonQuery(SqlQueries["PQAddEmployee"],p);
            }//foreach
        }//TestInsertEmployeesQuery
        
    }//class SqliteCRUDOps
    
    
    
}//namespace