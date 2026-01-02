//file : Models.cs

using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Text.RegularExpressions;

namespace sqlitecrud.Models{
public record Employee( string Name, 
                        string? DOB, 
                        string Email, 
                        decimal Salary);

public static  class ParamFactory
{
    private static bool traceOn = true;    
    public static SqliteParameter[] ToParameters(Employee e)
    // ref : https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex?view=net-10.0 
    // Maps Employee Data onto Params, skip if null
    {
        var P = new List<SqliteParameter>();
        
        if (!string.IsNullOrWhiteSpace(e.Name) ) {
            P.Add(new SqliteParameter("@Name",e.Name));
            if (traceOn) System.Console.WriteLine($"Parameter @Name bound to '{e.Name}'");
        }
        else//Dropped
        {
              if (traceOn) System.Console.WriteLine(@"/!\Parameter @Name was DROPPED");  
        }  
        
        
        //REF: c# regex for dates in yyyy-mm-dd format
        /* Bing Co-pilot:
            // Regex pattern for yyyy-mm-dd (basic validation)
            string pattern = @"^(?:19|20)\d\d-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$"; <-- used as is

        */
        if (!string.IsNullOrWhiteSpace(e.DOB))
        {
            string pattern = @"^(?:17|18|19|20)\d\d-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$";      //<-- co-pilot+bing  2026-01-01
            var  isMatch = Regex.IsMatch(e.DOB,pattern);                                    //<-- co-pilot+bing  2026-01-01
            if (isMatch) {
                P.Add(new SqliteParameter("@DOB",e.DOB));
                if (traceOn) System.Console.WriteLine($"Parameter @DOB bound to '{e.DOB}'");
            }
            else//Dropped as format invalid
            {
              if (traceOn) System.Console.WriteLine(@"/!\Parameter @DOB was DROPPED");  
            }
        }
        else//dropped as white space or null
        {
            if (traceOn) System.Console.WriteLine(@"/!\Parameter @DOB was DROPPED");  
        }//e.DOB
        

        /* Bing + Co-Pilot:
            c# regex for valid email address:
             // Practical regex for most email formats (case-insensitive)
            private static readonly Regex EmailRegex = new Regex(
                    @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase
                );

        */
        if (!string.IsNullOrWhiteSpace(e.Email)){
            
            var ValidEmail = new Regex( @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase );        //<-- co-pilot+bing  2026-01-01, adapted
            var isMatch = ValidEmail.IsMatch(e.Email);
            if (isMatch) {
                P.Add(new SqliteParameter("@Email",e.Email));
                 if (traceOn) System.Console.WriteLine($"Parameter @Email bound to '{e.Email}'");
                }
            else//Drop as format invalid
            {
              if (traceOn) System.Console.WriteLine(@"/!\Parameter @Email was DROPPED");  
            }
        }
        else//drop as null or whitespace
        {
            if (traceOn) System.Console.WriteLine(@"/!\Parameter @Email was DROPPED");  
        }//if e.Email
    
        

        // Co-Pilot 2026-01-01
        /*
        
            
        // Salary mapping: set DbType explicitly if you want decimal semantics;
        // Microsoft.Data.Sqlite typically maps numeric to REAL/INTEGER.
        var pSalary = new SqliteParameter("$Salary", e.Salary) { DbType = DbType.Decimal };


        */

        //Use negative values as a skip value
        if (e.Salary >= 0.0m) {
            P.Add(new SqliteParameter("@Salary",e.Salary){ DbType = DbType.Decimal });
            if (traceOn) System.Console.WriteLine($"Parameter @Salary bound to '{e.Salary}'");
            }
            else
            {
              if (traceOn) System.Console.WriteLine(@"/!\Parameter @Salary was DROPPED");  
            }//if trace


      //var Result = P.ToArray();

      return  P.ToArray();

    }//ToParameters
}//class ParamFactory

}//namespace