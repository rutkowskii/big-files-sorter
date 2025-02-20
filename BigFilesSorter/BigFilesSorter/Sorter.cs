using System.Text;
using Core;

namespace BigFilesSorter;


public class Sorter : IDisposable
{
    private readonly string _inputFile;
    private readonly bool _deleteIntermediateFiles;
    private readonly FileStream _inputFileStream;
    private readonly StreamReader _inputFileReader;
    private bool _needsNextFile;
    private readonly FileStream _resultFileStream;
    private readonly StreamWriter _resultStreamWriter;
    const int CharsPerMb = 1048576;
    const int AvgCharsPerLine = 28;
    const int SmallestFileMb = 2; // this is the max MB we load in the RAM. 

    public Sorter(string inputFile, bool deleteIntermediateFiles)
    {
        _inputFile = inputFile;
        _deleteIntermediateFiles = deleteIntermediateFiles;
        
        _inputFileStream = File.OpenRead(inputFile);
        _inputFileReader = new StreamReader(_inputFileStream);
        
        var outputFileName = $"{_inputFile}-SORTED";
        _resultFileStream = File.OpenWrite(outputFileName);
        _resultStreamWriter = new StreamWriter(_resultFileStream);
    }
    
    public async Task Sort()
    {
        try
        {
            var splitFiles = await SplitInputFile();

            var sortedSplitFilesAccessors = await SortSplitFiles(splitFiles);
            var accesorsCount = sortedSplitFilesAccessors.Length;

          
            
            var linesBuffer = new List<FileLine>();
            
            var linesCountToLoadFirst = CharsPerMb * SmallestFileMb / AvgCharsPerLine / accesorsCount;

            var lastLinesLoadedCount = int.MaxValue;
            var isFirstIteration = true;
            do
            {
                var linesToLoadCount = isFirstIteration ? linesCountToLoadFirst : linesCountToLoadFirst / accesorsCount; 
                var linesLoaded = await LoadLinesFromSplitFiles(sortedSplitFilesAccessors, linesToLoadCount);
                lastLinesLoadedCount = linesLoaded.Count;
                
                Console.WriteLine($"Loaded {lastLinesLoadedCount} from split files.");

                linesBuffer.AddRange(linesLoaded);
                linesBuffer.Sort(new DefaultFileLineComparer());

                await OutputTopLinesAndRemove(linesBuffer, linesCountToLoadFirst);
                isFirstIteration = false;
            } while (lastLinesLoadedCount > 0);

            await OutputTopLinesAndRemove(linesBuffer, linesBuffer.Count);

            
            foreach (var accessor in sortedSplitFilesAccessors)
            {
                accessor.Dispose();
                if (_deleteIntermediateFiles)
                {
                    File.Delete(accessor.File);
                }
            }
            

        }
        catch (System.Exception e)
        {
            Console.WriteLine("!!!!!!!!!!!!! ERROR !!!!!!!!!!!!!");
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task<SplitFileAccessor[]> SortSplitFiles(List<string> splitFiles)
    {
        var sortSmallFilesTasks = splitFiles.Select(SortSingleFile).ToArray();
        await Task.WhenAll(sortSmallFilesTasks);

        if (_deleteIntermediateFiles)
        {
            foreach (var splitFile in splitFiles)
            {
                File.Delete(splitFile);
            }
        }
        
        var sortedSplitFiles = sortSmallFilesTasks.Select(x => x.Result).ToArray();
        var sortedSplitFilesAccessors = sortedSplitFiles.Select(x => new SplitFileAccessor(x)).ToArray();
        return sortedSplitFilesAccessors;
    }

    private static async Task<List<FileLine>> LoadLinesFromSplitFiles(SplitFileAccessor[] accessors, int linesCountToLoad)
    {
        var linesLoadTasks = accessors.Select(x => x.ReadLines(linesCountToLoad)).ToArray();
        await Task.WhenAll();
        var allLines = linesLoadTasks.Select(x => x.Result).SelectMany(x => x).ToList();
        return allLines;
    }

    private async Task OutputTopLinesAndRemove(List<FileLine> linesBuffer, int linesCount)
    {
        foreach (var line in linesBuffer.Take(linesCount))
        {
            await _resultStreamWriter.WriteLineAsync(line.ToString());
        }

        linesBuffer.RemoveRange(0, linesCount);
        
        Console.WriteLine($">>> Outputted {linesCount} lines");
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
        foreach (var entry in entries)
        {
            await streamWriter.WriteLineAsync(entry.ToString());
        }

        return sortedFileName;
    }

    private async Task WriteNextFile(string fileName)
    {
        var iFileChars = 0;
        
        using var splitFileStream = File.Create(fileName);
        using var writer = new StreamWriter(splitFileStream);
        while (iFileChars < CharsPerMb * SmallestFileMb)
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
        if (_inputFileReader.EndOfStream)
        {
            return null;
        }

        return await _inputFileReader.ReadLineAsync();
    }

    public void Dispose()
    {
        _inputFileStream.Dispose();
        _inputFileReader.Dispose();

        _resultStreamWriter.Dispose();
        _resultFileStream.Dispose();
    }
}