//file : Models.cs

using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;

namespace sqlitecrud.Models{
public record Employee( string? Name, 
                        string? DOB, 
                        string? Email, 
                        decimal? Salary);

public static  class ModelTools
{
    /*
    public static  bool IsValidDate(string data,string paramString,string YearRange)
    {
            
    }//IsValidDate

    
    public static bool IsValidEmail((string data,string paramString)
    {
            

    }//IsValidEmail

    */

    private static bool traceOn = false;    
    public static SqliteParameter[] EmployeeToParameters(Employee e)
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
        if (e.Salary!=null && e.Salary >= 0.0m) {
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


    public static Dictionary<string,object> ProjectEmployee(SqliteDataReader r)
        {
            var recRow = new Dictionary<string,object>();

            for (int i = 0; i < r.FieldCount;i++)
            {
                recRow[r.GetName(i)] = r.IsDBNull(i)?null:r.GetValue(i);
            }//for

            return recRow;
        }

    public static string fit(string s,int width)
        {   
            //System.Console.WriteLine($"s={s}");
            if (string.IsNullOrEmpty(s)) s = " ";
            //var w = int.Min(1,width);
            var w = int.Min(s.Length,width);
            //System.Console.WriteLine($"width={width}, w={w}");

            var returnS = "";
            int x = 0;

            while (returnS.Length < w){
                                   
                    returnS += s[x];
                    x++;
                    //System.Console.WriteLine($"copy loop '{returnS}'");
            
            }//while
            

            while (returnS.Length < width)
            {
                returnS = " " + returnS;//pad from front
                //System.Console.WriteLine($"'{returnS}'");
            }

            //System.Console.WriteLine($"'{returnS}'");
            return returnS;

        }//fit

    public static void ShowListOfDictionaryData(List<Dictionary<string,object>> L )
        {
            if (L.Count == 0) {
                System.Console.WriteLine("No Data");
                return; //no data
            }    
            //grab field names of first Dictionary
            List <string> Fields = new List<string>();

            foreach(string key in L[0].Keys) Fields.Add(key);

            var widths = new Dictionary<string,int>();
            string s;
            foreach(string field in Fields)  widths[field] = field.Length;
               
            //check data max width and update accordingly
            
            foreach (Dictionary<string,object> rec in L)
            {
                foreach(string f in Fields)
                {
                    //System.Console.WriteLine($"{f}:{rec[f]}");
                    if(widths[f]<rec[f].ToString().Length) widths[f] = rec[f].ToString().Length;
                
                }
            }

            
            //debug - widths printout
            //foreach (string field in Fields) {System.Console.Write($"{fit(widths[field].ToString(),widths[field])}|\t");}
            //System.Console.WriteLine();
        

            //display field names using the correct widths
            foreach (string field in Fields) System.Console.Write($"{fit(field,widths[field])}|\t");//:{widths[field]}|");
            System.Console.WriteLine();//NewLine

            
            //display the rows of data in the field list order
            foreach (Dictionary<string,object> rec in L)
            {
                foreach(string field in Fields) System.Console.Write($"{fit(rec[field].ToString(),widths[field])}|\t");
                System.Console.WriteLine();// newline
            }//foreach rec

            
        }//ShowListOfDictionaryData

}//class ModelTools

}//namespace Models