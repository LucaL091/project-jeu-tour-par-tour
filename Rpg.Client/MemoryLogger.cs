using System.Collections.Generic;
using Rpg.Core.Interfaces;

namespace Rpg.Client
{
    public class MemoryLogger : IActionLogger
    {
        private readonly List<string> _logs = new List<string>();
        private const int MaxLogs = 5;

        public void Log(string message)
        {
            _logs.Add(message);
            if (_logs.Count > MaxLogs)
            {
                _logs.RemoveAt(0); // Remove oldest
            }
        }

        public List<string> GetLogs()
        {
            return new List<string>(_logs);
        }

        public void Clear()
        {
            _logs.Clear();
        }
    }
}
