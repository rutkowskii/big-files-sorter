using Core;

namespace BigFilesSorter;

public class SortedFileCheck
{
    public async Task Run(string sortedFile)
    {
        Console.WriteLine("Check if sorted file is valid - START");
        
        var fileStream = File.OpenRead(sortedFile);
        var fileReader = new StreamReader(fileStream);

        var queue = new Queue<FileLine>(2);

        var comparer = new DefaultFileLineComparer();
        while (!fileReader.EndOfStream)
        {
            var line = FileLine.FromString(await fileReader.ReadLineAsync());
            queue.Enqueue(line);

            if (queue.Count < 2)
            {
                var prev = queue.Peek();
                var nxt = queue.Last();
                var comparisonResult = comparer.Compare(prev, nxt);
                if (comparisonResult > 0)
                {
                    Console.WriteLine($"!!!!!!! ERROR IN SORTED FILE !!!!!!! prev: [{prev}] nxt: [{nxt}]");
                }
            }
        }
        
        Console.WriteLine("Check if sorted file is valid - FINISH");
    }
}