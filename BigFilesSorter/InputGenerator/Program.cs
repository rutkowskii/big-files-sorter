using System.CommandLine;

try
{
    var rootCommand = CmdLineAccess.BuildRootCmd();
    return await rootCommand.InvokeAsync(args);
}
catch (Exception e)
{
    Console.WriteLine("!!! ERROR");
    Console.WriteLine(e);
    throw;
}