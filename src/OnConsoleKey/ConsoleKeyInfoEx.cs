using System;

namespace OnConsoleKey
{
    public class ConsoleKeyInfoEx
    {
        public ConsoleKeyInfo KeyInfo { get; set; }
        public byte CursorCount { get; set; } = 1;
    }
}