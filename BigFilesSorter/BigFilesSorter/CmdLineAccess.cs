using System.CommandLine;

namespace BigFilesSorter;

public static class CmdLineAccess
{
    public static RootCommand BuildRootCmd()
    {
        var fileOption = new Option<FileInfo>("--file", "Input file");
        var memBufferSizeMbOption = new Option<int>("--memBufferSizeMb", () => 10, description: "Memory buffer size (MB)");

        var rootCommand1 = new RootCommand("Input files generator for BIG sort");
        rootCommand1.AddOption(fileOption);
        rootCommand1.AddOption(memBufferSizeMbOption);

        rootCommand1.SetHandler(async (file, memBufferSizeMb)  =>
            {
                string sortedFile = null;
                using(var sorter = new Sorter(file.FullName, memBufferSizeMb, deleteIntermediateFiles: true))
                {
                    sortedFile = await sorter.Sort();
                    Console.WriteLine($"Sorted file: [{sortedFile}]");          
                }
                
                await new SortedFileCheck().Run(sortedFile);
            },
            fileOption, memBufferSizeMbOption);
        return rootCommand1;
    }
}