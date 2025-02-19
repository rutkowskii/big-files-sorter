using System.Text;
using Core;

namespace BigFilesSorter;

public class Sorter : IDisposable
{
    private readonly string _inputFile;
    private readonly FileStream _inputFileStream;
    private readonly StreamReader _inputFileReader;
    private bool _needsNextFile;
    const int CharsPerMb = 1048576;
    const int AvgCharsPerLine = 28;
    const int SmallestFileMb = 2; // this is the max MB we load in the RAM. 

    public Sorter(string inputFile)
    {
        _inputFile = inputFile;
        _inputFileStream = File.OpenRead(inputFile);
        _inputFileReader = new StreamReader(_inputFileStream);
    }
    
    public async Task Sort()
    {
        try
        {
            var splitFiles = await SplitInputFile();

            var sortSmallFilesTasks = splitFiles.Select(SortSingleFile).ToArray();
            await Task.WhenAll(sortSmallFilesTasks);
            
            
            var sortedSplitFiles = sortSmallFilesTasks.Select(x => x.Result).ToArray();
            var accessors = sortedSplitFiles.Select(x => new SplitFileAccessor(x)).ToArray();
            var accesorsCount = accessors.Length;

            var outputFileName = $"{_inputFile}-SORTED";
            using var resultFileWriter = File.OpenWrite(outputFileName);
            
            var linesBuffer = new List<FileLine>();
            
            var linesCountToLoadFirst = CharsPerMb * SmallestFileMb / AvgCharsPerLine / accesorsCount;

            var lastLinesLoadedCount = int.MaxValue;
            var isFirstIteration = true;
            do
            {
                var linesToLoadCount = isFirstIteration ? linesCountToLoadFirst : linesCountToLoadFirst / accesorsCount; 
                var linesLoaded = await LoadLinesFromSplitFiles(accessors, linesToLoadCount);
                lastLinesLoadedCount = linesLoaded.Count;
                
                Console.WriteLine($"Loaded {lastLinesLoadedCount} from split files.");

                linesBuffer.AddRange(linesLoaded);
                linesBuffer.Sort(new DefaultFileLineComparer());

                await OutputTopLinesAndRemove(linesBuffer, linesCountToLoadFirst, resultFileWriter);
                isFirstIteration = false;
            } while (lastLinesLoadedCount > 0);

            await OutputTopLinesAndRemove(linesBuffer, linesBuffer.Count, resultFileWriter);

            
            foreach (var accessor in accessors)
            {
                accessor.Dispose();
            }
            

        }
        catch (System.Exception e)
        {
            Console.WriteLine("!!!!!!!!!!!!! ERROR !!!!!!!!!!!!!");
            Console.WriteLine(e);
            throw;
        }
    }

    private static async Task<List<FileLine>> LoadLinesFromSplitFiles(SplitFileAccessor[] accessors, int linesCountToLoad)
    {
        var linesLoadTasks = accessors.Select(x => x.ReadLines(linesCountToLoad)).ToArray();
        await Task.WhenAll();
        var allLines = linesLoadTasks.Select(x => x.Result).SelectMany(x => x).ToList();
        return allLines;
    }

    private static async Task OutputTopLinesAndRemove(List<FileLine> linesBuffer, int linesCount, FileStream resultFileWriter)
    {
        foreach (var line in linesBuffer.Take(linesCount))
        {
            await resultFileWriter.WriteAsync(Encoding.UTF8.GetBytes(line.ToString())); // todo piotr dry
            await resultFileWriter.WriteAsync(Encoding.UTF8.GetBytes(Environment.NewLine));
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
        using var fileWriter = File.OpenWrite(sortedFileName);
        foreach (var entry in entries)
        {
            await fileWriter.WriteAsync(Encoding.UTF8.GetBytes(entry.ToString()));
            await fileWriter.WriteAsync(Encoding.UTF8.GetBytes(Environment.NewLine));
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
    }
}