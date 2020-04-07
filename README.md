# OnConsoleKey

[![NuGet Badge](https://buildstats.info/nuget/OnConsoleKey)](https://www.nuget.org/packages/OnConsoleKey)

In the C # console application, there is no way to monitor or subscribe to keyboard events. Therefore, I wrote this small program as an alternative.

It is a bit like the user operation feature of Ubuntu terminal: press the tab key to automatically complete and record history.

## Examples

```csharp
var historyFile = "history.txt";
var sc = new SimpleConsole();
sc.Histories.RestoreFromFile(historyFile);
Console.CancelKeyPress += (sen, arg) =>
{
    Console.WriteLine("exit...");
    sc.Histories.SaveToFile(historyFile);
};
sc.Prompts = "shell> ";
sc.PromptsColor = ConsoleColor.Red;
sc.CursorCountFromCharactersHandle = c => c > 128 ? (by2 : (byte)1;
sc.AutoCompleteHandle = async str =>
{
    return await Task.FromResult(new[]
    {
       "测试自动补全",
       "Test auto-replenishment",
    });
};
sc.OnConsoleKeyInfo += (sen, key) =>
{
    if (key.Key == ConsoleKey.Enter)
    {
        Console.Write($"\ntype result :{sc.Buffer}");
    }
};
sc.Start();
```

For more information, please check [OnConsoleKey.Example](src/OnConsoleKey.Example/Program.cs).


In addition, I also wrote an application based on this program. Please check [here](https://github.com/hsxian/AnyDict).
