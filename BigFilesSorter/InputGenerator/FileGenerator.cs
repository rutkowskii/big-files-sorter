using System.Diagnostics;

namespace InputGenerator;

public class FileGenerator
{
    public void Run(
        string outputFileName,
        ulong outputBytes,
        int wordsPerStrMin, int wordsPerStrMax,
        int wordLenMin, int wordLenMax)
    {
        Console.WriteLine($"About to write {outputBytes}B ({outputBytes / 1024 / 1024}MB) to file {outputFileName}");
        Console.WriteLine(
            $"Words per str: ({wordsPerStrMin} - {wordsPerStrMax}), Word len: ({wordLenMin} - {wordLenMax})");
        var sw = new Stopwatch();
        sw.Start();

        var charsWritten = 0ul;
        var linesWritten = 0ul;

        using var fileStream = File.Create(outputFileName);
        using var writer = new StreamWriter(fileStream);

        var lineGenerator = new LineGenerator(wordsPerStrMin, wordsPerStrMax, wordLenMin, wordLenMax);
        while (charsWritten < outputBytes)
        {
            var line = lineGenerator.Build().ToString();
            writer.WriteLine(line);

            charsWritten += (ulong)line.Length;
            linesWritten++;
        }

        sw.Stop();
        Console.WriteLine($"Write finished after {sw.ElapsedMilliseconds / 1000}s");
    }
}