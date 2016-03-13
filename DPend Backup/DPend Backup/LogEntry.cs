using System;
using System.Collections.Generic;

using System.Text;

namespace DPend_Backup
{
    public class LogEntry
    {
        private DateTime time;
        private string title, desc;
        private LogEntryType type;

        public DateTime Time { get { return time; } }
        public string Title { get { return title; } }
        public string Description { get { return desc; } }
        public LogEntryType Type { get { return type; } }
    }
}
