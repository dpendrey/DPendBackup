using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPend_Backup.FileInfo
{
    public class FileInfo
    {
        #region Storage
        private DateTime created, modified;
        private string name, lowerName;
        private long size;
        #endregion

        #region Creation
        public static FileInfo FromPath(string RootPath, string Path)
        {
            System.IO.FileInfo info = new System.IO.FileInfo(Path);
            FileInfo retVal = new FileInfo(Worker.stripSource(RootPath, Path), info.CreationTime, info.LastWriteTime, info.Length);
            return retVal;
        }

        public static FileInfo FromFTP(string RootPath, string Info)
        {
            throw new NotImplementedException();
        }

        public FileInfo(string Name, DateTime Created, DateTime Modified, long Size)
        {
            name = Name;
            lowerName = Name.ToLower();
            created = Created;
            modified = Modified;
            size = Size;
        }
        #endregion

        #region Accessors
        public string Name { get { return name; } }
        public string LowerName { get { return lowerName; } }
        public DateTime Modified { get { return modified; } }
        public DateTime Created { get { return created; } }
        public long Size { get { return size; } }
        #endregion
    }
}
