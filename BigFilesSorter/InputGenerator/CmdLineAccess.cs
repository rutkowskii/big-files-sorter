using System.CommandLine;
using InputGenerator;

public static class CmdLineAccess
{
    public static RootCommand BuildRootCmd()
    {
        var sizeOption = new Option<ulong>(
            "--size",
            () => 20 * 1024 * 1024,
            "Output size (bytes), equivalent of 20 MB by default");

        var fileNameOption = new Option<string>(
            "--file",
            () => $"output-{Guid.NewGuid().ToString()[..5]}",
            "Output file name");

        var wordsPerStrMinOption = new Option<int>(
            "--wordsPerStrMin",
            () => 5,
            "Min number for words in string");

        var wordsPerStrMaxOption = new Option<int>(
            "--wordsPerStrMax",
            () => 5,
            "Max number for words in string");

        var wordLenMinOption = new Option<int>(
            "--wordLenMin",
            () => 400,
            "Min word length");

        var wordLenMaxOption = new Option<int>(
            "--wordLenMax",
            () => 400,
            "Max word len");

        var rootCommand1 = new RootCommand("Input files generator for BIG sort");
        rootCommand1.AddOption(sizeOption);
        rootCommand1.AddOption(fileNameOption);
        rootCommand1.AddOption(wordsPerStrMinOption);
        rootCommand1.AddOption(wordsPerStrMaxOption);
        rootCommand1.AddOption(wordLenMinOption);
        rootCommand1.AddOption(wordLenMaxOption);

        rootCommand1.SetHandler(
            (size, file, wordsPerStrMin, wordsPerStrMax, wordLenMin, wordLenMax) =>
            {
                new FileGenerator().Run(file, size, wordsPerStrMin, wordsPerStrMax, wordLenMin, wordLenMax);
            },
            sizeOption, fileNameOption, wordsPerStrMinOption, wordsPerStrMaxOption, wordLenMinOption, wordLenMaxOption);
        return rootCommand1;
    }
}