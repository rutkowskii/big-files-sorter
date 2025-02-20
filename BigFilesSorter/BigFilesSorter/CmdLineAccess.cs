using System.CommandLine;
using BigFilesSorter;

public static class CmdLineAccess
{
    public static RootCommand BuildRootCmd()
    {
        var fileOption = new Option<FileInfo>(
            "--file",
            "Input file");

        var rootCommand1 = new RootCommand("Input files generator for BIG sort");
        rootCommand1.AddOption(fileOption);

        rootCommand1.SetHandler(async file =>
            {
                using var sorter = new Sorter(file.FullName, true);
                await sorter.Sort();
            },
            fileOption);
        return rootCommand1;
    }
}