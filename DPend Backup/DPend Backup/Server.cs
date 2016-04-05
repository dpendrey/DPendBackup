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
        private static long filesChecked, bytesChecked, filesCopied, bytesCopied;

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
                    retVal = new SystemStatus(status, curPlan, workerThreads.Count, filesChecked, bytesChecked, filesCopied, bytesCopied, files.Count, dirs.Count);
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
            int curPlan = 0;

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
                    if (curPlan >= plans.Count)
                    {
                        curPlan = 0;
                        p = null;
                    }
                    else
                    {
                        p = plans[curPlan++];
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

            dirs.AddLast(Plan.Source);

            filesChecked =
                bytesChecked =
                filesCopied =
                bytesCopied =
                0;

            curPlan = Plan;

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
                (numDirs > 0 || numFiles > 0))
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

            SaveSettings();
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

            while (stat != DPend_Backup.Status.Stopping &&
                (numDirs > 0 || numFiles > 0))
            {
                if (numFiles > 0)
                {
                    #region Save file
                    string src = System.IO.Path.Combine(curPlan.Source, pathToRun);
                    string dst = System.IO.Path.Combine(curPlan.Destination, pathToRun);
                    System.IO.FileInfo iSrc = new System.IO.FileInfo(src),
                        iDst = new System.IO.FileInfo(dst);

                    // If the file doesn't exist or has a different time
                    if (!iDst.Exists ||
                        iSrc.LastWriteTimeUtc > iDst.LastWriteTimeUtc ||
                        iSrc.Length != iDst.Length)
                    {
                        if (!System.IO.Directory.Exists(iDst.DirectoryName))
                            System.IO.Directory.CreateDirectory(iDst.DirectoryName);

                        try
                        {
                            System.IO.FileStream read = new System.IO.FileStream(src, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                            System.IO.FileStream write = new System.IO.FileStream(dst,
                                System.IO.FileMode.OpenOrCreate,
                                System.IO.FileAccess.ReadWrite,
                                System.IO.FileShare.None,
                                256);
                            byte[] buff = new byte[4096];
                            int numRead = read.Read(buff, 0, buff.Length);
                            while (numRead > 0 && stat != DPend_Backup.Status.Stopping)
                            {
                                write.Write(buff, 0, numRead);
                                bytesCopied += numRead;
                                numRead = read.Read(buff, 0, buff.Length);
                            }

                            filesCopied++;
                            write.Flush();
                            write.Close();
                            read.Close();

                            if (stat != DPend_Backup.Status.Stopping)
                            {
                                System.IO.File.SetLastWriteTimeUtc(dst, iSrc.LastWriteTimeUtc);
                                System.IO.File.SetAttributes(dst, iSrc.Attributes);
                                System.IO.File.SetCreationTimeUtc(dst, iSrc.CreationTimeUtc);
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }

                    filesChecked++;
                    bytesChecked += iSrc.Length;
                    #endregion
                }
                else if (numDirs > 0)
                {
                    #region Scan dir
                    string src = System.IO.Path.Combine(curPlan.Source, pathToRun);
                    string dst = System.IO.Path.Combine(curPlan.Destination, pathToRun);

                    string[] tmp;

                    #region Scan directories
                    try
                    {
                        tmp = System.IO.Directory.GetDirectories(src);
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (tmp[i].StartsWith(curPlan.Source))
                            {
                                tmp[i] = tmp[i].Substring(curPlan.Source.Length);

                                bool allowScan = false;
                                foreach (string filter in curPlan.AllowDirs)
                                {
                                    if (CompareWildcard(tmp[i], filter))
                                    {
                                        allowScan = true;
                                        break;
                                    }
                                }
                                if (allowScan)
                                {
                                    foreach (string filter in curPlan.BlockDirs)
                                    {
                                        if (CompareWildcard(tmp[i], filter))
                                        {
                                            allowScan = false;
                                            break;
                                        }
                                    }

                                    if (allowScan)
                                    {
                                        lock (plans)
                                            dirs.AddLast(tmp[i]);
                                    }
                                }
                            }
                            else
                            {
                                System.Windows.Forms.MessageBox.Show("How do we have a directory that's not in its parent?");
                            }
                        }
                    }
                    catch (Exception) { }
                    #endregion

                    #region Scan files
                    try
                    {
                        tmp = System.IO.Directory.GetFiles(src);
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (tmp[i].StartsWith(curPlan.Source))
                            {
                                tmp[i] = tmp[i].Substring(curPlan.Source.Length);
                                if (tmp[i] == null)
                                {
                                }

                                bool allowScan = false;
                                foreach (string filter in curPlan.AllowFiles)
                                {
                                    if (CompareWildcard(tmp[i], filter))
                                    {
                                        allowScan = true;
                                        break;
                                    }
                                }
                                if (allowScan)
                                {
                                    foreach (string filter in curPlan.BlockFiles)
                                    {
                                        if (CompareWildcard(tmp[i], filter))
                                        {
                                            allowScan = false;
                                            break;
                                        }
                                    }

                                    if (allowScan)
                                    {
                                        if (tmp[i] == null)
                                        {
                                        }
                                        lock (plans)
                                            files.AddLast(tmp[i]);
                                        if (files.Last == null)
                                        {
                                        }
                                    }
                                }

                            }
                            else
                            {
                                System.Windows.Forms.MessageBox.Show("How do we have a file that's not in its parent?");
                            }
                        }
                    }
                    catch (Exception) { }
                    #endregion
                    #endregion
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


        #region IsLike
        /// <summary>
        /// Compares wildcard to string
        /// </summary>
        /// <param name="WildString">String to compare</param>
        /// <param name="Mask">Wildcard mask (ex: *.jpg)</param>
        /// <returns>True if match found</returns>
        public static bool CompareWildcard(string WildString, string Mask, bool IgnoreCase = true)
        {
            int i = 0, k = 0;

            i = WildString.LastIndexOf('/');
            if (i > -1)
                WildString = WildString.Substring(i + 1);
            i = WildString.LastIndexOf('\\');
            if (i > -1)
                WildString = WildString.Substring(i + 1);
            i = 0;

            // Cannot continue with Mask empty
            if (string.IsNullOrEmpty(Mask))
                return false;

            // If WildString is null -> make it an empty string
            if (WildString == null)
                WildString = string.Empty;

            // If Mask is * and WildString isn't empty -> return true
            if (string.Compare(Mask, "*") == 0 && !string.IsNullOrEmpty(WildString))
                return true;

            // If Mask is ? and WildString length is 1 -> return true
            if (string.Compare(Mask, "?") == 0 && WildString.Length == 1)
                return true;

            // If WildString and Mask match -> no need to go any further
            if (string.Compare(WildString, Mask, IgnoreCase) == 0)
                return true;

            while (k != WildString.Length)
            {
                switch (Mask[i])
                {
                    case '*':

                        if ((i + 1) == Mask.Length)
                            return true;

                        while (k != WildString.Length)
                        {
                            if (CompareWildcard(WildString.Substring(k + 1), Mask.Substring(i + 1), IgnoreCase))
                                return true;

                            k += 1;
                        }

                        return false;

                    case '?':

                        break;

                    default:

                        if (IgnoreCase == false && WildString[k] != Mask[i])
                            return false;

                        if (IgnoreCase && Char.ToLower(WildString[k]) != Char.ToLower(Mask[i]))
                            return false;

                        break;
                }

                i += 1;
                k += 1;
            }

            if (k == WildString.Length)
            {
                if (i == Mask.Length || Mask[i] == '*')
                    return true;
            }

            return false;
        }
        #endregion
    }
}
