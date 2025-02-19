// See https://aka.ms/new-console-template for more information

var outputFileSizeMb = 10; // MB
const int charsPerMb = 1048576;

try
{
    var charsWritten = 0l;
    var linesWritten = 0l;

    var outputFileName = $"output-{Guid.NewGuid().ToString()[..5]}";
    using var fileStream = File.Create(outputFileName);
    using var writer = new StreamWriter(fileStream);
    
    var lineGenerator = new LineGenerator();
    while (charsWritten < outputFileSizeMb * charsPerMb)
    {
        var line = lineGenerator.Build().ToString();
        writer.WriteLine(line);
    
        charsWritten += line.Length;
        linesWritten++;
    }
}
catch (Exception e)
{
    Console.WriteLine("!!! ERROR");
    Console.WriteLine(e);
    throw;
}