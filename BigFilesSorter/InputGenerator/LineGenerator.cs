using System.Text;
using Core;

public class LineGenerator
{
    private readonly Random _rand;
    private readonly WordGenerator _wordGenerator;
    private readonly int _wordsPerStrMin = 2;
    private readonly int _wordsPerStrMax = 6;
    private readonly int _wordLenMin = 3;
    private readonly int _wordLenMax = 10;  
    
    private readonly int _repetitionCoef = 100;
    private readonly int _repetitionCheck = 55;
    private readonly Queue<string> _previousStrings;

    public LineGenerator()
    {
        _rand = new Random();
        _wordGenerator = new WordGenerator();

        _previousStrings = new Queue<string>(7000);

    }

    public FileLine Build()
    {
        var usePreviousStr = _rand.Next(1, _repetitionCoef) == _repetitionCheck;
        
        var str = usePreviousStr ? GetPreviousStr() : BuildNewRandomStr();
        _previousStrings.Enqueue(str);
        return new FileLine(_rand.Next(), str);
    }

    private string GetPreviousStr()
    {
        var index = _rand.Next(_previousStrings.Count - 1);
        return _previousStrings.ElementAt(index);
    }

    private string BuildNewRandomStr()
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

        var str = sb.ToString();
        return str;
    }
}