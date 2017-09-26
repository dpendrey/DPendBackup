using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPend_Backup.FileInfo
{
    public class DirectoryInfo
    {
        #region Storage
        private List<FileInfo> src = new List<FileInfo>(),
            dst = new List<FileInfo>(),
            srcOrphans = new List<FileInfo>(),
            dstOrphans = new List<FileInfo>(),
            srcNewer = new List<FileInfo>(),
            dstNewer = new List<FileInfo>();
        private bool isProject;
        private string path;
        #endregion

        #region Creation
        public static void SourceFromPath(Plan Plan, DirectoryInfo Directory)
        {
            Directory.src.Clear();
            string[] files = System.IO.Directory.GetFiles(System.IO.Path.Combine(Plan.Source, Directory.path));

            for (int i = 0; i < files.Length; i++)
                if (Worker.includeFile(Plan, files[i]))
                    Directory.src.Add(FileInfo.FromPath(Directory.path, files[i]));
        }

        public static void DestinationFromPath(Plan Plan, DirectoryInfo Directory)
        {
            Directory.dst.Clear();
            string[] files = System.IO.Directory.GetFiles(System.IO.Path.Combine(Plan.Source, Directory.path));

            for (int i = 0; i < files.Length; i++)
                if (Worker.includeFile(Plan, files[i]))
                    Directory.dst.Add(FileInfo.FromPath(Directory.path, files[i]));
        }

        public static void DestinationFromZip(Plan Plan, DirectoryInfo Directory, string ZipPath)
        {
            Directory.dst.Clear();
            System.IO.FileInfo info = new System.IO.FileInfo(ZipPath);
            System.IO.Compression.ZipArchive zip = System.IO.Compression.ZipFile.Open(ZipPath, System.IO.Compression.ZipArchiveMode.Read);

            foreach (System.IO.Compression.ZipArchiveEntry entry in zip.Entries)
                if (Worker.includeFile(Plan, entry.FullName))
                    Directory.dst.Add(new FileInfo(entry.FullName, info.CreationTime, info.LastWriteTime, entry.Length));

            zip.Dispose();
        }

        public DirectoryInfo(string Path)
        {
            path = Path;
        }
        #endregion

        #region Accessors
        public void AddSourceFile(FileInfo File) { src.Add(File); }
        public void AddDestinationFile(FileInfo File) { dst.Add(File); }
        public string Path { get { return path; } }
        public bool IsProject { get { return isProject; } }

        public int SourceOrphansCount { get { return srcOrphans.Count; } }
        public int DestinationOrphansCount { get { return dstOrphans.Count; } }
        public int SourceNewerCount { get { return srcNewer.Count; } }
        public int DestinationNewerCount { get { return dstNewer.Count; } }

        public FileInfo[] SourceOrphans { get { return srcOrphans.ToArray(); } }
        public FileInfo[] DestinationOrphans { get { return dstOrphans.ToArray(); } }
        public FileInfo[] SourceNewer { get { return srcNewer.ToArray(); } }
        public FileInfo[] DestinationNewer { get { return dstNewer.ToArray(); } }
        #endregion

        #region Comparing
        public void CompareFiles()
        {
            srcOrphans.Clear();
            dstOrphans.Clear();
            srcNewer.Clear();
            dstNewer.Clear();

            List<FileInfo> copySource = new List<FileInfo>(src),
                copyDestination = new List<FileInfo>(dst);

            for (int i = 0; i < copySource.Count; i++)
            {
                bool matchFound = false;

                for (int j = 0; j < copyDestination.Count && !matchFound; j++)
                {
                    if (copySource[i].LowerName == copyDestination[j].LowerName)
                    {
                        matchFound = true;
                        if (
                            copySource[i].Created > copyDestination[j].Created
                            ||
                            copySource[i].Modified > copyDestination[j].Modified
                            )
                            srcNewer.Add(copyDestination[j]);
                        else if (
                            copySource[i].Created < copyDestination[j].Created
                            ||
                            copySource[i].Modified < copyDestination[j].Modified
                            )
                            dstNewer.Add(copyDestination[j]);

                        copyDestination.RemoveAt(i);
                        i--;
                        copySource.RemoveAt(j);
                        j--;
                    }
                }
            }

            srcOrphans.AddRange(copySource);
            dstOrphans.AddRange(copyDestination);
        }
        #endregion
    }
}
