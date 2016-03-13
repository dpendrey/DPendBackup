using System;
using System.Collections.Generic;

using System.Text;

namespace DPend_Backup
{
    public class SystemStatus
    {
        internal SystemStatus(Status Status, Plan Plan, int NumberWorkers, long FilesChecked, long BytesChecked, long FilesCopied, long BytesCopied, int FilesWaiting, int DirectoriesWaiting)
        {
            status = Status;
            plan = Plan;
            filesChecked = FilesChecked;
            bytesChecked = BytesChecked;
            filesCopied = FilesCopied;
            bytesCopied = BytesCopied;
            numWorkers = NumberWorkers;
            filesWaiting = FilesWaiting;
            dirsWaiting = DirectoriesWaiting;
        }

        private long filesChecked, bytesChecked, filesCopied, bytesCopied;
        private Plan plan;
        private Status status;
        private int numWorkers, filesWaiting, dirsWaiting;

        public long FilesChecked { get { return filesChecked; } }
        public long BytesChecked { get { return bytesChecked; } }
        public long FilesCopied { get { return filesCopied; } }
        public long BytesCopied { get { return bytesCopied; } }
        public Plan Plan { get { return plan; } }
        public Status Status { get { return status; } }
        public int NumberWorkers { get { return numWorkers; } }
        public int FilesWaiting { get { return filesWaiting; } }
        public int DirectoriesWaiting { get { return dirsWaiting; } }
    }

    public enum Status : int
    {
        Starting,
        Idle,
        Running,
        Stopping,
        Stopped
    }
}
