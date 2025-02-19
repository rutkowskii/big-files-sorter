namespace Core;

public class DefaultFileLineComparer : IComparer<FileLine>
{
    public int Compare(FileLine? x, FileLine? y)
    {
        var strComparisonResult = string.Compare(x.Str, y.Str, StringComparison.InvariantCultureIgnoreCase);
        if (strComparisonResult == 0)
        {
            return x.Number.CompareTo(y.Number);
        }

        return strComparisonResult;
    }
}