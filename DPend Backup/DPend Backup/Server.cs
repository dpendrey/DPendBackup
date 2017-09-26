using System;
using System.Collections.Generic;
using System.Text;

namespace DPend_Backup
{
    /// <summary>
    /// Handles all the plan management stuff
    /// </summary>
    public static class Server
    {
        private static List<Plan> plans = new List<Plan>();
        private static Plan curPlan = null;
        private static Status status = DPend_Backup.Status.Stopped;
        private static PlanExecutionStatus executionStatus = new PlanExecutionStatus();
        private static DestTypes.DestType destinationType = null;

        /// <summary>
        /// Gets the complete list of current plans
        /// </summary>
        public static Plan[] Plans
        {
            get
            {
                Plan[] retVal;
                lock (plans)
                    retVal = plans.ToArray();
                return retVal;
            }
        }

        public static void AddPlan(Plan Plan)
        {
            lock (plans)
                plans.Add(Plan);
        }

        public static void RemovePlan(Plan Plan)
        {
            lock (plans)
                plans.Remove(Plan);
        }

        /// <summary>
        /// Gets the currently executing plan
        /// </summary>
        public static SystemStatus Status
        {
            get
            {
                SystemStatus retVal;
                lock (plans)
                    retVal = new SystemStatus(status, curPlan, workerThreads.Count, executionStatus.FilesScanned, executionStatus.BytesScanned, executionStatus.FilesCopied, executionStatus.BytesCopied, files.Count, dirs.Count);
                return retVal;
            }
        }

        public static void Start()
        {
            lock (plans)
            {
                if (status == DPend_Backup.Status.Stopped)
                    status = DPend_Backup.Status.Starting;
                System.Threading.Thread thr = new System.Threading.Thread(new System.Threading.ThreadStart(runWorker));
                thr.Priority = System.Threading.ThreadPriority.Lowest;
                thr.Start();
            }
        }

        public static void Stop()
        {
            lock (plans)
            {
                if (status != DPend_Backup.Status.Stopped)
                    status = DPend_Backup.Status.Stopping;
            }
        }

        public static void SaveSettings()
        {
            string oldFile = System.IO.Path.Combine(
                    System.IO.Path.Combine(
                        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DPend"),
                    "DPend Backup"),
                "Settings.iniold");
            string newFile = System.IO.Path.Combine(
                    System.IO.Path.Combine(
                        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DPend"),
                    "DPend Backup"),
                "Settings.ini");

            if (System.IO.File.Exists(oldFile))
                System.IO.File.Delete(oldFile);
            if (System.IO.File.Exists(newFile))
                System.IO.File.Move(
                    newFile,
                    oldFile);

            System.IO.StreamWriter writer = new System.IO.StreamWriter(newFile);

            lock (plans)
            {
                foreach (Plan p in plans)
                {
                    writer.WriteLine("[NEW PLAN]");
                    p.WriteData(writer);
                    writer.WriteLine();
                }
            }

            writer.Flush();
            writer.Close();

            if (System.IO.File.Exists(oldFile))
                System.IO.File.Delete(oldFile);
        }

        private static void runWorker()
        {
            Status stat;
            int planIndex = 0;

            #region Load list of plans
            string oldFile = System.IO.Path.Combine(
                    System.IO.Path.Combine(
                        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DPend"),
                    "DPend Backup"),
                "Settings.iniold");
            string newFile = System.IO.Path.Combine(
                    System.IO.Path.Combine(
                        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DPend"),
                    "DPend Backup"),
                "Settings.ini");

            lock (plans)
                plans.Clear();
            if (System.IO.File.Exists(oldFile))
            {
                #region Read file
                System.IO.StreamReader read = new System.IO.StreamReader(
                    oldFile
                    );

                string curLine = read.ReadLine();

                while (curLine != null)
                {
                    int tmpIndex = curLine.IndexOf('#');
                    if (tmpIndex > -1)
                        curLine = curLine.Substring(tmpIndex);
                    curLine = curLine.Trim();

                    if (curLine.ToUpper() == "[NEW PLAN]")
                    {
                        Plan p = new Plan();
                        p.ReadData(read);
                        lock (plans)
                            plans.Add(p);
                    }

                    curLine = read.ReadLine();
                }

                read.Close();
                #endregion
            }
            else if (System.IO.File.Exists(newFile))
            {
                #region Read file
                System.IO.StreamReader read = new System.IO.StreamReader(
                    newFile
                    );

                string curLine = read.ReadLine();

                while (curLine != null)
                {
                    int tmpIndex = curLine.IndexOf('#');
                    if (tmpIndex > -1)
                        curLine = curLine.Substring(tmpIndex);
                    curLine = curLine.Trim();

                    if (curLine.ToUpper() == "[NEW PLAN]")
                    {
                        Plan p = new Plan();
                        p.ReadData(read);
                        lock (plans)
                            plans.Add(p);
                    }

                    curLine = read.ReadLine();
                }

                read.Close();
                #endregion
            }
            lock (plans)
                status = DPend_Backup.Status.Running;
            #endregion

            #region Run until a stop is requested
            lock (plans)
                stat = status;
            while (stat != DPend_Backup.Status.Stopping)
            {
                #region Get next plan to work with
                Plan p;
                lock (plans)
                {
                    if (curPlan != null)
                        p = null;
                    else if (planIndex >= plans.Count)
                    {
                        planIndex = 0;
                        p = null;
                    }
                    else
                    {
                        p = plans[planIndex++];
                    }
                }
                #endregion

                #region Check plan, run if needed
                if (p == null)
                    System.Threading.Thread.Sleep(1000);
                else
                {
                    if (p.NeedsToRun)
                    {
                        runBackup(p);
                    }
                }
                #endregion

                lock (plans)
                    stat = status;
            }
            #endregion
        }

        static LinkedList<string> dirs = new LinkedList<string>();
        static LinkedList<string> files = new LinkedList<string>();
        static LinkedList<System.Threading.Thread> workerThreads = new LinkedList<System.Threading.Thread>();
        private static void runBackup(Plan Plan)
        {
            dirs = new LinkedList<string>();
            files = new LinkedList<string>();

            Plan.Status = PlanStatus.Running;
            Plan.LastAttmpted = DateTime.Now;

            dirs.AddLast(Plan.Source);

            curPlan = Plan;
            executionStatus = new PlanExecutionStatus();
            switch (Plan.DestinationType)
            {
                case LocationType.Directory:
                    destinationType = new DestTypes.Directory();
                    break;
                case LocationType.CompressedDirectory:
                    destinationType = new DestTypes.DirectoryCompressed();
                    break;
                default:
                    System.Windows.Forms.MessageBox.Show("Ah! Unknown destination type!");
                    throw new Exception();
            }

            int numFiles, numDirs;
            Status stat;

            #region Get info on next thing to run
            lock (plans)
            {
                numFiles = files.Count;
                numDirs = dirs.Count;
                stat = status;
            }
            #endregion
            while (stat != DPend_Backup.Status.Stopping &&
                (numDirs > 0 || numFiles > 0|| workerThreads.Count > 0))
            {
                #region Trim worker thread list
                LinkedListNode<System.Threading.Thread> node = workerThreads.First;
                while (node != null)
                {
                    if (node.Value.ThreadState == System.Threading.ThreadState.Aborted ||
                        node.Value.ThreadState == System.Threading.ThreadState.Stopped)
                    {
                        LinkedListNode<System.Threading.Thread> next = node.Next;
                        workerThreads.Remove(node);
                        node = next;
                    }
                    else
                        node = node.Next;
                }
                #endregion

                #region Fire up more workers as needed
                if (workerThreads.Count < curPlan.NumberWorkers && (numDirs > 0 | numFiles > 0))
                {
                    System.Threading.Thread thr = new System.Threading.Thread(new System.Threading.ThreadStart(runBackupWorker));
                    thr.Priority = System.Threading.ThreadPriority.Lowest;
                    thr.Start();
                    workerThreads.AddLast(thr);
                }
                #endregion

                #region Wait around for a bit
                System.Threading.Thread.Sleep(500);
                #endregion

                #region Get info on next thing to run
                lock (plans)
                {
                    numFiles = files.Count;
                    numDirs = dirs.Count;
                    stat = status;
                }
                #endregion
            }

            if (numDirs == 0 && numFiles == 0&& workerThreads.Count==0)
            {
                Plan.LastRun = Plan.LastAttmpted;
                Plan.Status = PlanStatus.OK;

                SaveSettings();
                lock (plans)
                {
                    if (curPlan == Plan)
                        curPlan = null;
                    else
                        stat = DPend_Backup.Status.Stopping;
                }
            }
        }
        private static void runBackupWorker()
        {
            Status stat;
            int numFiles, numDirs;
            string pathToRun = "";

            #region Get info on next thing to run
            lock (plans)
            {
                numFiles = files.Count;
                numDirs = dirs.Count;
                stat = status;

                if (workerThreads.First!=null&&System.Threading.Thread.CurrentThread == workerThreads.First.Value)
                {
                    if (numDirs > 0 && ((numDirs < curPlan.NumberWorkers * 2 && numFiles < curPlan.NumberWorkers * 2) || (numFiles < 1000)))
                    {
                        pathToRun = dirs.Last.Value;
                        dirs.RemoveLast();
                        numFiles = 0;
                    }
                    else if (numFiles > 0)
                    {
                        pathToRun = files.Last.Value;
                        files.RemoveLast();
                    }
                    else if (numDirs > 0)
                    {
                        pathToRun = dirs.Last.Value;
                        dirs.RemoveLast();
                    }
                }
                else
                {
                    if (numFiles > 0)
                    {
                        pathToRun = files.Last.Value;
                        files.RemoveLast();
                    }
                    else if (numDirs > 0)
                    {
                        pathToRun = dirs.Last.Value;
                        dirs.RemoveLast();
                    }
                }
            }
            #endregion

            while (stat != DPend_Backup.Status.Stopping &&
                (numDirs > 0 || numFiles > 0))
            {
                if (numDirs > 0)
                {
                    List<string> dirsToAdd = new List<string>(),
                        filesToAdd = new List<string>();
                    destinationType.SaveDirectory(curPlan, executionStatus, pathToRun, dirsToAdd, filesToAdd);
                    if (dirsToAdd.Count > 0 || filesToAdd.Count > 0)
                        lock (plans)
                        {
                            for (int i = 0; i < dirsToAdd.Count; i++)
                                dirs.AddLast(dirsToAdd[i]);
                            for (int i = 0; i < filesToAdd.Count; i++)
                                files.AddLast(filesToAdd[i]);
                        }
                }
                else if (numFiles > 0)
                {
                    destinationType.SaveFile(curPlan, executionStatus, pathToRun);
                }

                #region Get info on next thing to run
                lock (plans)
                {
                    numFiles = files.Count;
                    numDirs = dirs.Count;
                    stat = status;

                    if (System.Threading.Thread.CurrentThread == workerThreads.First.Value)
                    {
                        if (numDirs > 0 && ((numDirs < curPlan.NumberWorkers * 2 && numFiles < curPlan.NumberWorkers * 2) || (numFiles < 1000)))
                        {
                            pathToRun = dirs.Last.Value;
                            dirs.RemoveLast();
                            numFiles = 0;
                        }
                        else if (numFiles > 0)
                        {
                            pathToRun = files.Last.Value;
                            files.RemoveLast();
                        }
                        else if (numDirs > 0)
                        {
                            pathToRun = dirs.Last.Value;
                            dirs.RemoveLast();
                        }
                    }
                    else
                    {
                        if (numFiles > 0)
                        {
                            pathToRun = files.Last.Value;
                            files.RemoveLast();
                        }
                        else if (numDirs > 0)
                        {
                            pathToRun = dirs.Last.Value;
                            dirs.RemoveLast();
                        }
                    }
                }
                #endregion
            }
        }
    }
}
