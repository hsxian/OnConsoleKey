using System.Drawing;
using System;

namespace OnConsoleKey.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var sc = new SimpleConsole();
            sc.Prompts = "shell> ";
            sc.PromptsColor = Color.Red;
            sc.AutoCompleteHandel = str =>
            {
                return new[]
                {
                    "测试自动补全",
                    "Test auto-replenishment",
                    "自動補完のテスト",
                    "Тест автоматическое пополнение",
                };
            };
            sc.OnConsoleKeyInfo += (sen, key) =>
            {
                if (key.Key == ConsoleKey.Enter)
                {

                }
            };
            sc.Start();
        }
    }
}
