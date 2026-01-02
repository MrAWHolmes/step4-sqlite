Help with RunParamNonQuery()

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