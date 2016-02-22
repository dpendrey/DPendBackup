using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DPend_Backup
{
    /// <summary>
    /// Represents a backup plan
    /// </summary>
    public class Plan
    {
        private string name, source, dest;
        private string[] allowList = new string[] { "*.*" };
        private string[] blockList = new string[0];
        private DateTime lastRun = DateTime.MinValue, lastAttempt=DateTime.Now;
        private int timeSpan=1;
        private TimeSpanType timeSpanType = TimeSpanType.Days;
        private int retryTimeSpan = 5;
        private TimeSpanType retryTimeSpanType = TimeSpanType.Minutes;
        private long lastFileCount = -1;
        private long lastFileSize = -1;
        private int filesToKeep = 10;
        private PlanStatus status = PlanStatus.Created;
        private Log log = new Log();

        /// <summary>
        /// Gets/sets the name of this plan
        /// </summary>
        public string Name { get { return name; } set { name = value; } }
        /// <summary>
        /// Gets/sets the source for this plan
        /// </summary>
        public string Source { get { return source; } set { source = value; } }
        /// <summary>
        /// Gets/sets the destination for this plan
        /// </summary>
        public string Destination { get { return dest; } set { dest = value; } }

        /// <summary>
        /// Gets the allow list
        /// </summary>
        public string[] AllowList { get { return allowList; } }

        /// <summary>
        /// Gets the blocklist
        /// </summary>
        public string[] BlockList { get { return blockList; } }

        /// <summary>
        /// Gets the last time the backup was run
        /// </summary>
        public DateTime LastRun { get { return lastRun; } }

        /// <summary>
        /// Gets the last time the backup was attempted (even if it failed)
        /// </summary>
        public DateTime LastAttmpted { get { return lastAttempt; } }

        /// <summary>
        /// Gets/sets the time span for this plan
        /// </summary>
        public int TimeSpan { get { return timeSpan; } set { timeSpan = value; } }

        /// <summary>
        /// Gets/sets the time span type for this plan
        /// </summary>
        public TimeSpanType TimeSpanType { get { return timeSpanType; } set { timeSpanType = value; } }

        /// <summary>
        /// Gets/sets the retry time span for this plan if there was a failure
        /// </summary>
        public int RetryTimeSpan { get { return retryTimeSpan; } set { retryTimeSpan = value; } }

        /// <summary>
        /// Gets/sets the retry time span type for this plan if there was a failure
        /// </summary>
        public TimeSpanType RetryTimeSpanType { get { return retryTimeSpanType; } set { retryTimeSpanType = value; } }

        /// <summary>
        /// Gets the number of files included in the last run of this backup
        /// </summary>
        public long LastFileCount { get { return lastFileCount; } }

        /// <summary>
        /// Gets the number of bytes of files in the last run of this backup
        /// </summary>
        public long LastFileSize { get { return lastFileSize; } }

        /// <summary>
        /// Gets/sets the number of historical versions to keep of each file
        /// </summary>
        public int FilesToKeep { get { return filesToKeep; } set { filesToKeep = value; } }

        /// <summary>
        /// Gets the current status of the plan
        /// </summary>
        public PlanStatus Status { get { return status; } }

        /// <summary>
        /// Gets the log associated with this plan
        /// </summary>
        public Log Log { get { return log; } }
    }
}