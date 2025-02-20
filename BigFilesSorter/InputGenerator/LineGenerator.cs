using System.Text;
using Core;

public class LineGenerator
{
    private readonly Queue<string> _previousStrings;
    private readonly Random _rand;
    private readonly int _repetitionCheck = 55;

    private readonly int _repetitionCoef = 100;
    private readonly WordGenerator _wordGenerator;
    private readonly int _wordLenMax;
    private readonly int _wordLenMin;
    private readonly int _wordsPerStrMax;
    private readonly int _wordsPerStrMin;

    public LineGenerator(int wordsPerStrMin, int wordsPerStrMax, int wordLenMin, int wordLenMax)
    {
        _rand = new Random();
        _wordGenerator = new WordGenerator();

        _previousStrings = new Queue<string>(7000);

        _wordsPerStrMin = wordsPerStrMin;
        _wordsPerStrMax = wordsPerStrMax;
        _wordLenMin = wordLenMin;
        _wordLenMax = wordLenMax;
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
            if (i < wordsCount - 1) sb.Append(' ');
        }

        var str = sb.ToString();
        return str;
    }
}