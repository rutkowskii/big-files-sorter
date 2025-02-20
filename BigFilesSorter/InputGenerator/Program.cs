// See https://aka.ms/new-console-template for more information

var outputFileSizeMb = 10; // MB


try
{
    

    var outputFileName = $"output-{Guid.NewGuid().ToString()[..5]}";
  
}
catch (Exception e)
{
    Console.WriteLine("!!! ERROR");
    Console.WriteLine(e);
    throw;
}