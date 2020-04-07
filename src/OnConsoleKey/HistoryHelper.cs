using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System;

namespace OnConsoleKey
{
    public class HistoryHelper
    {
        private int _currentIdx = 0;
        private int _maxHistoryCount;

        public int MaxHistoryCount
        {
            get => _maxHistoryCount;
            set
            {
                if (_maxHistoryCount == value || value < 1) return;
                _maxHistoryCount = value;
                TrimQueue();
            }
        }
        private List<string> HistoryQueue { get; set; }

        public HistoryHelper()
        {
            HistoryQueue = new List<string>();
            MaxHistoryCount = 10;
        }
        public bool RestoreFromFile(string file)
        {
            if (File.Exists(file) == false)
            {
                return false;
            }
            var list = File.ReadAllLines(file)?.Where(t => false == string.IsNullOrWhiteSpace(t)).ToList();
            if (list != null)
            {
                HistoryQueue = list;
                TrimQueue();
                _currentIdx = HistoryQueue.Count - 1;
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Add(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return;
            str = str.Trim();
            var idx = HistoryQueue.IndexOf(str);
            if (idx > -1)
            {
                HistoryQueue.RemoveAt(idx);
            }
            HistoryQueue.Add(str);
            TrimQueue();
            _currentIdx = HistoryQueue.Count - 1;
        }
        public bool SaveToFile(string file)
        {
            try
            {
                File.WriteAllLines(file, HistoryQueue);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
            return true;
        }
        private void TrimQueue()
        {
            while (HistoryQueue.Count > MaxHistoryCount)
            {
                HistoryQueue.RemoveAt(0);
            }
        }
        public string Next()
        {
            if (_currentIdx > HistoryQueue.Count - 1)
            {
                _currentIdx = 0;
            }
            return HistoryQueue.ElementAtOrDefault(_currentIdx++);
        }
        public string Previous()
        {
            if (_currentIdx < 0)
            {
                _currentIdx = HistoryQueue.Count - 1;
            }
            return HistoryQueue.ElementAtOrDefault(_currentIdx--);
        }
        public IEnumerable<string> All()
        {
            return HistoryQueue;
        }
    }
}