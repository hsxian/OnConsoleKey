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
        public StringBuilder Buffer { get; private set; }
        private int _currentIdx = 0;
        private int _tabCount = 0;
        private string _tabLine = "";
        public SimpleConsole()
        {
            Buffer = new StringBuilder();
            Console.CancelKeyPress += (sen, arg) => { if (LastKey.Key != ConsoleKey.Enter) Console.WriteLine(); };
        }
        public void Start()
        {
            Console.Write(Prompts, PromptsColor);
            do
            {
                LastKey = Console.ReadKey(true);

                if (LastKey.Key != ConsoleKey.Tab)
                {
                    _tabCount = 0;
                }

                switch (LastKey.Key)
                {
                    case ConsoleKey.Backspace:
                        if (RemoveAt(_currentIdx - 1)) _currentIdx--;
                        break;
                    case ConsoleKey.Delete:
                        if (RemoveAt(_currentIdx)) _currentIdx--;
                        break;
                    case ConsoleKey.Enter:
                        Enter();
                        break;
                    case ConsoleKey.LeftArrow:
                        if (MoveOffset(_currentIdx, -1)) _currentIdx--;
                        break;
                    case ConsoleKey.RightArrow:
                        if (MoveOffset(_currentIdx, 1)) _currentIdx++;
                        break;
                    case ConsoleKey.Tab:
                        Tab();
                        break;
                    default:
                        if (Insert(_currentIdx, LastKey.KeyChar)) _currentIdx++;
                        break;
                }

                OnConsoleKeyInfo?.Invoke(this, LastKey);

            }
            while (true);
        }
        private void AddKey(string str)
        {
            Buffer = Buffer.Append(str);

            Console.Write(str);
        }
        private void AddKey(char c)
        {
            Buffer = Buffer.Append(c);

            Console.Write(c);
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
            if (str == null) return 0;
            return str.ToArray().Sum(t => ChartWidthCount(t));
        }

        private bool Insert(int idx, char c)
        {
            if (idx < 0 || idx > Buffer.Length) return false;

            var right = Buffer.ToString(idx, Buffer.Length - idx);

            byte w = ChartWidthCount(c);
            Buffer = Buffer.Insert(idx, c);

            var left = (Console.CursorLeft + w) % Console.WindowWidth;
            var top = Console.CursorTop + (Console.CursorLeft + w) / Console.WindowWidth;
            Console.Write(c);
            Console.Write(right);
            Console.SetCursorPosition(left, top);

            return true;
        }
        private bool RemoveAt(int idx)
        {
            if (idx < 0 || idx >= Buffer.Length) return false;
            var key = Buffer[idx];
            var w = ChartWidthCount(key);
            var left = Console.CursorLeft - w;
            var top = Console.CursorTop;
            if (left < 0 && Buffer.Length > 0)
            {
                top--;
                left = Console.WindowWidth - w;
            }
            var right = Buffer.ToString(idx, Buffer.Length - idx);
            var blankCount = StringWidthCount(right);
            Console.SetCursorPosition(left, top);
            Console.Write(new string(' ', blankCount));

            Buffer = Buffer.Remove(idx, 1);
            Console.SetCursorPosition(left, top);
            if (false == string.IsNullOrWhiteSpace(right))
            {
                right = right.Substring(1, right.Length - 1);
                Console.Write(right);
                Console.SetCursorPosition(left, top);
            }

            return true;
        }

        private bool MoveOffset(int idx, int offset)
        {
            if (offset > 0 && idx < Buffer.Length)
            {
                var count = StringWidthCount(Buffer.ToString(idx, offset));
                var left = Console.CursorLeft + count;
                var top = Console.CursorTop;
                var allCount = BufferWidthCount() + StringWidthCount(Prompts);
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
                var count = StringWidthCount(Buffer.ToString(idx + offset, -offset));
                var left = Console.CursorLeft - count;
                var top = Console.CursorTop;
                if (left < 0 && Buffer.Length > 0)
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
            Console.Write(new string(' ', BufferWidthCount()));
            Console.SetCursorPosition(left, Console.CursorTop);
            Buffer.Clear();
            _currentIdx = 0;
        }
        private void Enter()
        {
            MoveOffset(_currentIdx, Buffer.Length - _currentIdx);
            Console.Write($"\n{Prompts}", PromptsColor);
            Buffer.Clear();
            _currentIdx = 0;
        }
        private void Tab()
        {
            if (_tabCount == 0)
            {
                _tabLine = Buffer.ToString();
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
        private int BufferWidthCount()
        {
            return StringWidthCount(Buffer.ToString());
        }
    }
}
