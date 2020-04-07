using System.Threading.Tasks;
using System;

namespace OnConsoleKey.Example
{
    class Program
    {
        static void Main(string[] args)
        {
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
            sc.CursorCountFromCharactersHandle = c => c > 128 ? (byte)2 : (byte)1;
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
        }
    }
}
