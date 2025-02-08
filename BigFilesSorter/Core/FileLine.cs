namespace Core;

public class FileLine
{
    public int Number { get; private set; }
    public string Str { get; private set; }

    public FileLine(int number, string str)
    {
        Number = number;
        Str = str;
    }

    public override string ToString() => $"{Number}. {Str}";

    public static FileLine FromString(string serializedStr)
    {
        var dotIndex = serializedStr.IndexOf('.');

        var number = int.Parse(serializedStr.Substring(0, dotIndex));
        var str = serializedStr.Substring(dotIndex + 2);

        return new FileLine(number, str);
    } 
}