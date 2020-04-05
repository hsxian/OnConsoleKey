using System.Text;
using System.Collections.Generic;
using System.Linq;
using System;
using Console = Colorful.Console;
using System.Drawing;

namespace OnConsoleKey
{
    public class SimpleConsole
    {
        public string Prompts { get; set; }
        public Color PromptsColor { get; set; } = Color.White;
        public event Action<SimpleConsole, ConsoleKeyInfo> OnConsoleKeyInfo;
        public Func<string, IEnumerable<string>> AutoCompleteHandel { get; set; }
        public ConsoleKeyInfo LastKey { get; private set; }
        public List<ConsoleKeyInfoEx> CurrentKeys { get; private set; }
        private int _currentIdx = 0;
        private int _tabCount = 0;
        private string _tabLine = "";
        public SimpleConsole()
        {
            CurrentKeys = new List<ConsoleKeyInfoEx>();
            Console.CancelKeyPress += (sen, arg) => { if (LastKey.Key != ConsoleKey.Enter) Console.WriteLine(); };
        }
        public void Start()
        {
            new StringBuilder();
            Console.Write(Prompts, PromptsColor);
            do
            {
                // while (!Console.KeyAvailable)
                {
                    LastKey = Console.ReadKey(true);

                    if (LastKey.Key != ConsoleKey.Tab)
                    {
                        _tabCount = 0;
                    }

                    if (LastKey.Key == ConsoleKey.Backspace)
                    {
                        if (RemoveAt(_currentIdx - 1))
                            _currentIdx--;
                    }
                    else if (LastKey.Key == ConsoleKey.Delete)
                    {
                        if (RemoveAt(_currentIdx))
                            _currentIdx--;
                    }
                    else if (LastKey.Key == ConsoleKey.Enter)
                    {
                        Console.Write($"\n{Prompts}", PromptsColor);
                        Clear();
                    }
                    else if (LastKey.Key == ConsoleKey.LeftArrow)
                    {
                        if (MoveOffset(_currentIdx, -1)) _currentIdx--;
                    }
                    else if (LastKey.Key == ConsoleKey.RightArrow)
                    {
                        if (MoveOffset(_currentIdx, 1)) _currentIdx++;
                    }
                    else if (LastKey.Key == ConsoleKey.Tab)
                    {
                        if (_tabCount == 0)
                        {
                            _tabLine = CurrentKeys.Select(t => t.KeyInfo.KeyChar).ToArray().ToString();
                        }
                        var autos = AutoCompleteHandel?.Invoke(_tabLine);
                        if (autos?.Any() == true)
                        {
                            var ac = autos.Skip(_tabCount++ % autos.Count()).First();
                            Clear();
                            _currentIdx = ac.Length;
                            AddKey(ac);
                        }

                    }
                    else
                    {
                        Insert(_currentIdx, LastKey);

                        _currentIdx++;
                    }

                    OnConsoleKeyInfo?.Invoke(this, LastKey);

                }
            }
            while (true);
        }
        private void AddKey(string str)
        {
            var arr = str.ToArray();
            foreach (var c in arr)
            {
                AddKey(c);
            }
        }
        private void AddKey(char c)
        {
            var ck = (ConsoleKey)0;
            if (c > 0 && c < 255)
            {
                ck = (ConsoleKey)c;
            }
            var key = new ConsoleKeyInfo(c, ck, false, false, false);
            AddKey(key);
        }
        private byte ChartWidthCount(char c)
        {
            var count = Console.OutputEncoding.GetByteCount(new[] { c });
            byte w = 1;
            if (count > 2)
            {
                w = 2;
            }
            return w;
        }
        private int StringWidthCount(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return 0;
            return str.ToArray().Sum(t => ChartWidthCount(t));
        }
        private void AddKey(ConsoleKeyInfo c)
        {
            byte w = ChartWidthCount(c.KeyChar);

            CurrentKeys.Add(new ConsoleKeyInfoEx { KeyInfo = c, CursorCount = w });

            Console.Write(c.KeyChar);
        }
        private bool Insert(int idx, ConsoleKeyInfo c)
        {
            if (idx < 0 || idx > CurrentKeys.Count) return false;

            var right = CurrentKeys.Skip(idx).Select(t => t.KeyInfo.KeyChar).ToArray();

            byte w = ChartWidthCount(c.KeyChar);
            CurrentKeys.Insert(idx, new ConsoleKeyInfoEx { KeyInfo = c, CursorCount = w });

            var left = (Console.CursorLeft + w) % Console.WindowWidth;
            var top = Console.CursorTop + (Console.CursorLeft + w) / Console.WindowWidth;
            Console.Write(c.KeyChar);
            Console.Write(right);
            Console.SetCursorPosition(left, top);

            return true;
        }
        private bool RemoveAt(int idx)
        {
            if (idx < 0 || idx >= CurrentKeys.Count) return false;
            var key = CurrentKeys.ElementAt(idx);
            var left = Console.CursorLeft - key.CursorCount;
            var top = Console.CursorTop;
            if (left < 0 && CurrentKeys.Any())
            {
                top--;
                left = Console.WindowWidth - key.CursorCount;
            }
            var blankCount = CurrentKeys.Skip(idx).Sum(t => t.CursorCount);
            Console.SetCursorPosition(left, top);
            Console.Write(new string(' ', blankCount));

            CurrentKeys.RemoveAt(idx);
            Console.SetCursorPosition(left, top);
            var right = CurrentKeys.Skip(idx).Select(t => t.KeyInfo.KeyChar).ToArray();
            Console.Write(right);
            Console.SetCursorPosition(left, top);

            return true;
        }

        private bool MoveOffset(int idx, int offset)
        {
            if (offset > 0 && idx < CurrentKeys.Count)
            {
                var count = CurrentKeys.Skip(idx).Take(Math.Abs(offset)).Sum(t => t.CursorCount);
                var left = Console.CursorLeft + count;
                var top = Console.CursorTop;
                var allCount = CurrentKeys.Sum(t => t.CursorCount) + StringWidthCount(Prompts);
                if (left >= Console.WindowWidth && allCount >= Console.WindowWidth)
                {
                    left = 0;
                    top++;
                }
                Console.SetCursorPosition(left, top);
                return true;
            }
            else if (offset < 0 && idx > 0)
            {
                var count = CurrentKeys.Skip(idx + offset).Take(Math.Abs(offset)).Sum(t => t.CursorCount);
                var left = Console.CursorLeft - count;
                var top = Console.CursorTop;
                if (left < 0 && CurrentKeys.Any())
                {
                    top--;
                    left = Console.WindowWidth - 1;
                }
                Console.SetCursorPosition(left, top);
                return true;
            }
            return false;
        }
        private void Clear()
        {
            var left = StringWidthCount(Prompts);
            Console.SetCursorPosition(left, Console.CursorTop);
            Console.Write(new string(' ', CurrentKeys.Sum(t => t.CursorCount)));
            Console.SetCursorPosition(left, Console.CursorTop);
            CurrentKeys.Clear();
            _currentIdx = 0;
        }
    }
}
