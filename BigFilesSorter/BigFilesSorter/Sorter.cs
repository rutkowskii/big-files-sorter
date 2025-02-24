using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Core;

namespace BigFilesSorter;

public class Sorter : IDisposable
{
    private const int CharsPerMb = 1048576;
    private const int AvgCharsPerLine = 28;
    
    private readonly bool _deleteIntermediateFiles;
    private readonly string _inputFile;
    private readonly int _memBufferSizeMb;
    private readonly StreamReader _inputFileReader;
    private readonly FileStream _inputFileStream;
    private readonly FileStream _resultFileStream;
    private readonly StreamWriter _resultStreamWriter;
    private bool _needsNextFile;
    private readonly string _outputFileName;

    public Sorter(string inputFile, int memBufferSizeMb, bool deleteIntermediateFiles)
    {
        _inputFile = inputFile;
        _memBufferSizeMb = memBufferSizeMb;
        
        _deleteIntermediateFiles = deleteIntermediateFiles;

        _inputFileStream = File.OpenRead(inputFile);
        _inputFileReader = new StreamReader(_inputFileStream);

        _outputFileName = $"{_inputFile}-SORTED";
        _resultFileStream = File.OpenWrite(_outputFileName);
        _resultStreamWriter = new StreamWriter(_resultFileStream);
    }

    public void Dispose()
    {
        _inputFileStream.Dispose();
        _inputFileReader.Dispose();

        _resultStreamWriter.Dispose();
        _resultFileStream.Dispose();
    }

    /// <summary>
    /// Returns sorted file name
    /// </summary>
    /// <returns></returns>
    public async Task<string> Sort()
    {
        try
        {
            var sw = new Stopwatch();
            sw.Start();
            
            var splitFiles = await SplitInputFile();

            var sortedSplitFilesAccessors = await SortSplitFiles(splitFiles);
            var accesorsCount = sortedSplitFilesAccessors.Length;


            var linesBuffer = new List<FileLine>();

            var lastLinesLoadedCount = int.MaxValue;
            var isFirstIteration = true;
            do
            {
                var megabytesToLoadTotal = isFirstIteration ? _memBufferSizeMb : _memBufferSizeMb / accesorsCount;
                var linesLoaded = await LoadLinesFromSplitFiles(sortedSplitFilesAccessors, megabytesToLoadTotal);
                lastLinesLoadedCount = linesLoaded.Length;

                // Console.WriteLine($"Loaded {lastLinesLoadedCount} lines from split files.");

                linesBuffer.AddRange(linesLoaded);
                linesBuffer.Sort(new DefaultFileLineComparer());

                OutputTopLinesAndRemove(linesBuffer, linesBuffer.Count / accesorsCount);
                isFirstIteration = false;
            } while (lastLinesLoadedCount > 0);

            OutputTopLinesAndRemove(linesBuffer, linesBuffer.Count);
            
            sw.Stop();
            Console.WriteLine($"Finished sorting file {_inputFile}, it took {sw.ElapsedMilliseconds / 1000}s");

            CleanupSplitFilesAccessors(sortedSplitFilesAccessors);

            return _outputFileName;
        }
        catch (Exception e)
        {
            Console.WriteLine("!!!!!!!!!!!!! ERROR !!!!!!!!!!!!!");
            Console.WriteLine(e);
            throw;
        }
    }

    private void CleanupSplitFilesAccessors(SplitFileAccessor[] sortedSplitFilesAccessors)
    {
        foreach (var accessor in sortedSplitFilesAccessors)
        {
            accessor.Dispose();
            if (_deleteIntermediateFiles) File.Delete(accessor.File);
        }
    }

    private async Task<SplitFileAccessor[]> SortSplitFiles(List<string> splitFiles)
    {
        var sortSmallFilesTasks = splitFiles.Select(SortSingleFile).ToArray();
        await Task.WhenAll(sortSmallFilesTasks);

        if (_deleteIntermediateFiles)
            foreach (var splitFile in splitFiles)
                File.Delete(splitFile);

        var sortedSplitFiles = sortSmallFilesTasks.Select(x => x.Result).ToArray();
        var sortedSplitFilesAccessors = sortedSplitFiles.Select(x => new SplitFileAccessor(x)).ToArray();
        return sortedSplitFilesAccessors;
    }

    private static async Task<FileLine[]> LoadLinesFromSplitFiles(
        SplitFileAccessor[] accessors,
        int megabytesToLoadTotal)
    {
        var results = new ConcurrentBag<FileLine>();
        
        var bytesToLoadPerAccessor = megabytesToLoadTotal * 1024 * 1024 / accessors.Length;
        
        var linesLoadTasks = accessors.Select(accessor => Task.Run(() =>
        {
            foreach (var fileLine in accessor.ReadLines(bytesToLoadPerAccessor))
            {
                results.Add(fileLine);
            }
        })).ToArray();
        
        await Task.WhenAll();
        return results.ToArray();
    }

    private void OutputTopLinesAndRemove(List<FileLine> linesBuffer, int linesCount)
    {
        var sb = new StringBuilder();
        foreach (var line in linesBuffer.Take(linesCount))
        {
            sb.AppendLine(line.ToString());
        }
        _resultStreamWriter.Write(sb);

        linesBuffer.RemoveRange(0, linesCount);

        // Console.WriteLine($">>> Outputted {linesCount} lines");
    }

    private async Task<List<string>> SplitInputFile()
    {
        var splitFiles = new List<string>();
        var iFile = 0;

        _needsNextFile = true;

        while (_needsNextFile)
        {
            var fileName = $"{_inputFile}-{iFile}";

            await WriteNextFile(fileName);
            splitFiles.Add(fileName);
            iFile++;
        }


        return splitFiles;
    }

    private async Task<string> SortSingleFile(string fileName)
    {
        var fileLines = await File.ReadAllLinesAsync(fileName);
        var entries = fileLines.Select(FileLine.FromString).ToList();
        entries.Sort(new DefaultFileLineComparer());

        var sortedFileName = $"{fileName}-SORTED";
        using var fileStream = File.OpenWrite(sortedFileName);
        using var streamWriter = new StreamWriter(fileStream);
        foreach (var entry in entries) await streamWriter.WriteLineAsync(entry.ToString());

        return sortedFileName;
    }

    private async Task WriteNextFile(string fileName)
    {
        var iFileChars = 0;

        using var splitFileStream = File.Create(fileName);
        using var writer = new StreamWriter(splitFileStream);
        while (iFileChars < CharsPerMb * _memBufferSizeMb)
        {
            var lineStr = await TryGetNextLine();
            if (lineStr is null)
            {
                _needsNextFile = false;
                break;
            }

            await writer.WriteLineAsync(lineStr);
            iFileChars += lineStr.Length;
        }
    }

    private async Task<string?> TryGetNextLine()
    {
        if (_inputFileReader.EndOfStream) return null;

        return await _inputFileReader.ReadLineAsync();
    }
}