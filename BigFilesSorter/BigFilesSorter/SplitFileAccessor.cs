﻿using Core;

namespace BigFilesSorter;

public class SplitFileAccessor : IDisposable
{
    public readonly string File;
    
    private readonly FileStream _fileReader;
    private readonly StreamReader _streamReader;

    public SplitFileAccessor(string file)
    {
        File = file;
        _fileReader = System.IO.File.OpenRead(File);
        _streamReader = new StreamReader(_fileReader);
        ReachedEnd = false;
    }
    
    public bool ReachedEnd { get; private set; }

    public async Task<List<FileLine>> ReadLines(int n)
    {
        var results = new List<FileLine>();
        if (ReachedEnd)
        {
            return results;
        }

        var linesCount = 0;
        while (linesCount < n)
        {
            var line = await _streamReader.ReadLineAsync();
            if (line is null)
            {
                ReachedEnd = true;
                break;
            }

            results.Add(FileLine.FromString(line));
            linesCount++;
        }

        return results;
    }

    public void Dispose()
    {
        _streamReader.Dispose();
        _fileReader.Dispose();
    }
}