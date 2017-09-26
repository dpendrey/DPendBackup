using System;
using System.Collections.Generic;

using System.Text;

namespace DPend_Backup
{
    /// <summary>
    /// Represents a backup plan
    /// </summary>
    public class Plan
    {
        private string name, source, dest;
        private LocationType sourceType = LocationType.Directory, destType = LocationType.Directory;
        private string[] allowFiles = new string[] { "*.*" };
        private string[] blockFiles = new string[0];
        private string[] allowDirs = new string[] { "*" };
        private string[] blockDirs = new string[0];
        private string[] projectFiles = new string[0];
        private DateTime lastRun = DateTime.MinValue, lastAttempt = DateTime.Now;
        private int timeSpan = 1;
        private TimeSpanType timeSpanType = TimeSpanType.Days;
        private int retryTimeSpan = 5;
        private TimeSpanType retryTimeSpanType = TimeSpanType.Minutes;
        private long lastFileCount = -1;
        private long lastFileSize = -1;
        private int filesToKeep = 10;
        private PlanStatus status = PlanStatus.OK;
        private Log log = new Log();
        private int numWorkers = 3;

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
        /// Gets/sets the type of source
        /// </summary>
        public LocationType SourceType { get { return sourceType; } set { sourceType = value; } }
        /// <summary>
        /// Gets/sets the type of destination
        /// </summary>
        public LocationType DestinationType { get { return destType; } set { destType = value; } }

        /// <summary>
        /// Gets the allow list
        /// </summary>
        public string[] AllowFiles { get { return allowFiles; } }
        public void AddAllowFile(string Pattern)
        {
            List<string> tmp = new List<string>(allowFiles);
            if (!tmp.Contains(Pattern))
                tmp.Add(Pattern);
            allowFiles = tmp.ToArray();
        }

        /// <summary>
        /// Gets the blocklist
        /// </summary>
        public string[] BlockFiles { get { return blockFiles; } }
        public void AddBlockFile(string Pattern)
        {
            List<string> tmp = new List<string>(blockFiles);
            if (!tmp.Contains(Pattern))
                tmp.Add(Pattern);
            blockFiles = tmp.ToArray();
        }

        /// <summary>
        /// Gets the allow list
        /// </summary>
        public string[] AllowDirs { get { return allowDirs; } }
        public void AddAllowDir(string Pattern)
        {
            List<string> tmp = new List<string>(allowDirs);
            if (!tmp.Contains(Pattern))
                tmp.Add(Pattern);
            allowDirs = tmp.ToArray();
        }

        /// <summary>
        /// Gets the blocklist
        /// </summary>
        public string[] BlockDirs { get { return blockDirs; } }
        public void AddBlockDir(string Pattern)
        {
            List<string> tmp = new List<string>(blockDirs);
            if (!tmp.Contains(Pattern))
                tmp.Add(Pattern);
            blockDirs = tmp.ToArray();
        }

        /// <summary>
        /// Gets the project file markers
        /// </summary>
        public string[] ProjectFiles { get { return projectFiles; } }
        public void AddProjectFile(string Pattern)
        {
            List<string> tmp = new List<string>(projectFiles);
            if (!tmp.Contains(Pattern))
                tmp.Add(Pattern);
            projectFiles = tmp.ToArray();
        }

        /// <summary>
        /// Gets the last time the backup was run
        /// </summary>
        public DateTime LastRun { get { return lastRun; }set { lastRun = value; } }

        /// <summary>
        /// Gets the last time the backup was attempted (even if it failed)
        /// </summary>
        public DateTime LastAttmpted { get { return lastAttempt; }set { lastAttempt = value; } }

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
        public PlanStatus Status { get { return status; }set { status = value; } }

        /// <summary>
        /// Gets the log associated with this plan
        /// </summary>
        public Log Log { get { return log; } }

        /// <summary>
        /// Gets/sets the maximum number of workers to use for this plan
        /// </summary>
        public int NumberWorkers { get { return numWorkers; } set { numWorkers = value; } }

        /// <summary>
        /// Gets true if the plans needs to run
        /// </summary>
        public bool NeedsToRun
        {
            get
            {
                // We cannot acces either the source of the destination, so we don't really need to run right now
                if (!System.IO.Directory.Exists(source) ||
                    !System.IO.Directory.Exists(dest))
                    return false;

                switch (status)
                {
                    case PlanStatus.OK:
                        switch (timeSpanType)
                        {
                            case DPend_Backup.TimeSpanType.Minutes:
                                return DateTime.Now.Subtract(lastRun).TotalMinutes > timeSpan;
                            case DPend_Backup.TimeSpanType.Hours:
                                return DateTime.Now.Subtract(lastRun).TotalHours > timeSpan;
                            case DPend_Backup.TimeSpanType.Days:
                                return DateTime.Now.Subtract(lastRun).TotalDays > timeSpan;
                            case DPend_Backup.TimeSpanType.Weeks:
                                return (DateTime.Now.Subtract(lastRun).TotalDays / 7) > timeSpan;
                            case DPend_Backup.TimeSpanType.Months:
                            default:
                                return (DateTime.Now.Subtract(lastRun).TotalDays / 30) > timeSpan;
                        }
                    case PlanStatus.Running:
                        return false;
                    case PlanStatus.Faults:
                        switch (retryTimeSpanType)
                        {
                            case DPend_Backup.TimeSpanType.Minutes:
                                return DateTime.Now.Subtract(lastAttempt).TotalMinutes > retryTimeSpan;
                            case DPend_Backup.TimeSpanType.Hours:
                                return DateTime.Now.Subtract(lastAttempt).TotalHours > retryTimeSpan;
                            case DPend_Backup.TimeSpanType.Days:
                                return DateTime.Now.Subtract(lastAttempt).TotalDays > retryTimeSpan;
                            case DPend_Backup.TimeSpanType.Weeks:
                                return (DateTime.Now.Subtract(lastAttempt).TotalDays / 7) > retryTimeSpan;
                            case DPend_Backup.TimeSpanType.Months:
                            default:
                                return (DateTime.Now.Subtract(lastAttempt).TotalDays / 30) > retryTimeSpan;
                        }
                    default:
                        return false;
                }
            }
        }

        internal void ReadData(System.IO.StreamReader Reader)
        {
            string curLine = Reader.ReadLine();

            while (curLine != null)
            {
                int tmpIndex = curLine.IndexOf('#');
                if (tmpIndex > -1)
                    curLine = curLine.Substring(tmpIndex);
                curLine = curLine.Trim();

                if (curLine.ToUpper().StartsWith("NAME=")) name = curLine.Substring("NAME=".Length);
                if (curLine.ToUpper().StartsWith("SOURCE=")) source = curLine.Substring("SOURCE=".Length);
                if (curLine.ToUpper().StartsWith("DESTINATION=")) dest = curLine.Substring("DESTINATION=".Length);
                if (curLine.ToUpper().StartsWith("ALLOWFILE=")) AddAllowFile(curLine.Substring("ALLOWFILE=".Length));
                if (curLine.ToUpper().StartsWith("BLOCKFILE=")) AddBlockFile(curLine.Substring("BLOCKFILE=".Length));
                if (curLine.ToUpper().StartsWith("ALLOWDIRECTORY=")) AddAllowDir(curLine.Substring("ALLOWDIRECTORY=".Length));
                if (curLine.ToUpper().StartsWith("BLOCKDIRECTORY=")) AddBlockDir(curLine.Substring("BLOCKDIRECTORY=".Length));
                if (curLine.ToUpper().StartsWith("PROJECTFILE=")) AddProjectFile(curLine.Substring("PROJECTFILE=".Length));
                if (curLine.ToUpper().StartsWith("LASTRUN=")) lastRun = DateTime.Parse(curLine.Substring("LASTRUN=".Length));
                if (curLine.ToUpper().StartsWith("LASTATTEMPT=")) lastAttempt = DateTime.Parse(curLine.Substring("LASTATTEMPT=".Length));
                if (curLine.ToUpper().StartsWith("TIMESPAN=")) timeSpan = int.Parse(curLine.Substring("TIMESPAN=".Length));
                if (curLine.ToUpper().StartsWith("RETRYTIMESPAN=")) retryTimeSpan = int.Parse(curLine.Substring("RETRYTIMESPAN=".Length));
                if (curLine.ToUpper().StartsWith("TIMESPANTYPE=")) timeSpanType = (DPend_Backup.TimeSpanType)int.Parse(curLine.Substring("TIMESPANTYPE=".Length));
                if (curLine.ToUpper().StartsWith("RETRYTIMESPANTYPE=")) retryTimeSpanType = (DPend_Backup.TimeSpanType)int.Parse(curLine.Substring("RETRYTIMESPANTYPE=".Length));
                if (curLine.ToUpper().StartsWith("LASTFILECOUNT=")) lastFileCount = long.Parse(curLine.Substring("LASTFILECOUNT=".Length));
                if (curLine.ToUpper().StartsWith("LASTFILESIZE=")) lastFileSize = long.Parse(curLine.Substring("LASTFILESIZE=".Length));
                if (curLine.ToUpper().StartsWith("FILESTOKEEP=")) filesToKeep = int.Parse(curLine.Substring("FILESTOKEEP=".Length));
                if (curLine.ToUpper().StartsWith("WORKERS=")) numWorkers = int.Parse(curLine.Substring("WORKERS=".Length));
                if (curLine.ToUpper().StartsWith("STATUS=")) status = (PlanStatus)int.Parse(curLine.Substring("STATUS=".Length));
                if (curLine.ToUpper().StartsWith("SOURCETYPE=")) sourceType =(LocationType)int.Parse( curLine.Substring("SOURCETYPE=".Length));
                if (curLine.ToUpper().StartsWith("DESTINATIONTYPE=")) destType = (LocationType)int.Parse( curLine.Substring("DESTINATIONTYPE=".Length));


                if (curLine.ToUpper() == "[END PLAN]")
                    curLine = null;
                else
                    curLine = Reader.ReadLine();
            }
        }

        internal void WriteData(System.IO.StreamWriter Writer)
        {
            Writer.WriteLine("Name=" + name);
            Writer.WriteLine("Source=" + source);
            Writer.WriteLine("Destination=" + dest);
            Writer.WriteLine("SourceType=" + ((int)sourceType).ToString());
            Writer.WriteLine("DestinationType=" + ((int)destType).ToString());
            foreach (string tmp in allowFiles)
                Writer.WriteLine("AllowFile=" + tmp);
            foreach (string tmp in blockFiles)
                Writer.WriteLine("BlockFile=" + tmp);
            foreach (string tmp in allowDirs)
                Writer.WriteLine("AllowDirectory=" + tmp);
            foreach (string tmp in blockDirs)
                Writer.WriteLine("BlockDirectory=" + tmp);
            foreach (string tmp in projectFiles)
                Writer.WriteLine("ProjectFile=" + tmp);
            Writer.WriteLine("LastRun=" + lastRun.ToString());
            Writer.WriteLine("LastAttempt=" + lastAttempt.ToString());
            Writer.WriteLine("TimeSpan=" + timeSpan.ToString());
            Writer.WriteLine("TimeSpanType=" + ((int)timeSpanType).ToString());
            Writer.WriteLine("RetryTimeSpan=" + retryTimeSpan.ToString());
            Writer.WriteLine("RetryTimeSpanType=" + ((int)retryTimeSpanType).ToString());
            Writer.WriteLine("LastFileCount=" + lastFileCount.ToString());
            Writer.WriteLine("LastFileSize=" + lastFileSize.ToString());
            Writer.WriteLine("FilesToKeep=" + filesToKeep.ToString());
            Writer.WriteLine("Workers=" + numWorkers.ToString());
            Writer.WriteLine("Status=" + ((int)status).ToString());
            Writer.WriteLine("[END PLAN]");
        }
    }
}