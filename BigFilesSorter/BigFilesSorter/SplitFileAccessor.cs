using Core;

namespace BigFilesSorter;

public class SplitFileAccessor : IDisposable
{
    private readonly FileStream _fileReader;
    private readonly StreamReader _streamReader;
    public readonly string File;

    public SplitFileAccessor(string file)
    {
        File = file;
        _fileReader = System.IO.File.OpenRead(File);
        _streamReader = new StreamReader(_fileReader);
        ReachedEnd = false;
        
    }

    public bool ReachedEnd { get; private set; }

    public List<FileLine> ReadLines(int bytesToLoad)
    {
        var results = new List<FileLine>();
        if (ReachedEnd) return results;

        var bytesLoaded = 0;
        while (bytesLoaded < bytesToLoad)
        {
            var line = _streamReader.ReadLine();
            if (line is null)
            {
                ReachedEnd = true;
                break;
            }

            results.Add(FileLine.FromString(line));
            bytesLoaded += line.Length;
        }

        return results;
    }

    public void Dispose()
    {
        _streamReader.Dispose();
        _fileReader.Dispose();
    }
}