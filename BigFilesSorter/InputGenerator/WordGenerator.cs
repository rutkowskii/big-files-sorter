using System.Text;

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
        for (var i = 0; i < len; i++)
        {
            var ch = (char)_rand.NextInt64('a', 'z' + 1);
            sb.Append(ch);
        }

        return sb.ToString();
    }
}