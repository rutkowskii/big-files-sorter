// See https://aka.ms/new-console-template for more information

using System.Text;
using Core;

Console.WriteLine("Hello, World!");

var outputFileSizeMb = 10; // MB
const int charsPerMb = 1048576;



var charsWritten = 0l;

var randGenerator = new Random();

var outputFileName = $"output-{Guid.NewGuid().ToString()[..5]}";

// using var fileStream = File.OpenWrite(outputFileName);
using var fileStream = File.Create(outputFileName);
using var writer = new StreamWriter(fileStream);


var lineGenerator = new LineGenerator();
while (charsWritten < outputFileSizeMb * charsPerMb)
{
    var line = lineGenerator.Build().ToString();
    writer.WriteLine(line);
    charsWritten += line.Length;
}

public class LineGenerator
{
    private readonly Random _rand;
    private readonly WordGenerator _wordGenerator;
    private readonly int _wordsPerStrMin = 2;
    private readonly int _wordsPerStrMax = 6;
    private readonly int _wordLenMin = 3;
    private readonly int _wordLenMax = 10;       

    public LineGenerator()
    {
        _rand = new Random();
        _wordGenerator = new WordGenerator();
    }

    public FileLine Build()
    {
        var sb = new StringBuilder();
        
        var wordsCount = _rand.Next(_wordsPerStrMin, _wordsPerStrMax);
        for (var i = 0; i < wordsCount; i++)
        {
            var word = _wordGenerator.Build(_rand.Next(_wordLenMin, _wordLenMax));
            sb.Append(word);
            if (i < wordsCount - 1)
            {
                sb.Append(' ');
            }
        }

        return new FileLine(_rand.Next(), sb.ToString());
    } 
    
}

public class WordGenerator
{
    private readonly Random _rand;

    public WordGenerator()
    {
        _rand = new Random();
    }

    public string Build(int len)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < len; i++)
        {
            var ch = (char) _rand.NextInt64('a', 'z');
            sb.Append(ch);
        }

        return sb.ToString();
    }
}


 