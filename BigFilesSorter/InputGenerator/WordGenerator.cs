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
        for (int i = 0; i < len; i++)
        {
            var ch = (char) _rand.NextInt64('a', 'z');
            sb.Append(ch);
        }

        return sb.ToString();
    }
}