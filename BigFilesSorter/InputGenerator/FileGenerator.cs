namespace InputGenerator;

public class FileGenerator
{
    const int CharsPerMb = 1048576;
    
    public void Run(string outputFileName, int outputFileSizeMb)
    {
        var charsWritten = 0l;
        var linesWritten = 0l;
        
        using var fileStream = File.Create(outputFileName);
        using var writer = new StreamWriter(fileStream);
    
        var lineGenerator = new LineGenerator();
        while (charsWritten < outputFileSizeMb * CharsPerMb)
        {
            var line = lineGenerator.Build().ToString();
            writer.WriteLine(line);
    
            charsWritten += line.Length;
            linesWritten++;
        }
    }
}