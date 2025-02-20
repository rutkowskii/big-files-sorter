namespace Core;

public class FileLine
{
    public FileLine(int number, string str)
    {
        Number = number;
        Str = str;
    }

    public int Number { get; }
    public string Str { get; }

    public override string ToString()
    {
        return $"{Number}. {Str}";
    }

    public static FileLine FromString(string serializedStr)
    {
        var dotIndex = serializedStr.IndexOf('.');

        var number = int.Parse(serializedStr.Substring(0, dotIndex));
        var str = serializedStr.Substring(dotIndex + 2);

        return new FileLine(number, str);
    }
}