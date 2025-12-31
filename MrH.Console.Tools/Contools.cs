//file: GameDbDemo\MrH.Console.Tools\MrH.Console.Tools\Contools.cs

namespace MrH.Console.Tools;

public static class Contools
{
    public static bool YesOrNo(string Context,string Prompt)
    {
        System.Console.WriteLine(Context);
        ConsoleKeyInfo  Key ;

        while (true)
        {
            System.Console.Write(Prompt+" (Y/N)?");
            Key  = System.Console.ReadKey();

            //line spacing
            System.Console.WriteLine();

            if(Key.Key == ConsoleKey.Y) return true;
            if(Key.Key == ConsoleKey.N) return false;

        }//while
        
    }//YesOrNo

    public static string filePath(string RelativePathFolder,string FileName)
    {
        string baseDir = AppContext.BaseDirectory;
        string dataDir = Path.Combine(baseDir,RelativePathFolder);

        //ABSOLUTELY BLOODY VITAL!!!!
        
        // Ensure the directory exists

        Directory.CreateDirectory(dataDir); // <-- Absoluteletly BLOODY vital !!!!


        string filePathStr = Path.Combine(dataDir,FileName);

        return filePathStr;

    }//filePath

}//class Contools
