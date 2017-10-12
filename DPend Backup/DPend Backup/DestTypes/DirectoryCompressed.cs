using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPend_Backup.DestTypes
{
    public class DirectoryCompressed : DestType
    {
        public override void SaveFile(Plan Plan, PlanExecutionStatus Status, string Path)
        {
        }

        public override void SaveDirectory(Plan Plan, PlanExecutionStatus Status, string Path, List<string> NewDirectories, List<string> NewFiles)
        {
            /*
            if (Path.ToLower() == Plan.Source.ToLower())
            {
                string[] subDirs = Worker.listDirs(Plan, Path);
                NewDirectories.AddRange(subDirs);
                return;
            }
            if (!Path.ToLower().Contains("voidrats"))
                return;
            */

            string src = Path;
            string dst = System.IO.Path.Combine(Plan.Destination, Worker.stripSource(Plan.Source, Path));
            DateTime lastBackup = DateTime.MinValue;
            DateTime lastModified = DateTime.MinValue;
            bool isProject = Worker.IsProjectDirectory(Plan, Path);


            #region Determine latest file to be backed up
            string[] filesToSave;
            if (isProject)
                filesToSave = Worker.listFilesRecursive(Plan, src);
            else
                filesToSave = Worker.listFiles(Plan, src);
            for (int i = 0; i < filesToSave.Length; i++)
            {
                System.IO.FileInfo info = new System.IO.FileInfo(filesToSave[i]);
                if (lastModified < info.CreationTime)
                    lastModified = info.CreationTime;
                if (lastModified < info.LastWriteTime)
                    lastModified = info.LastWriteTime;
            }
            #endregion

            #region Determine latest backup
            if (System.IO.Directory.Exists(dst))
            {
                string[] tmp = System.IO.Directory.GetFiles(dst, "Backup *.zip");
                for (int i = 0; i < tmp.Length; i++)
                {
                    DateTime fileSaved = DateTime.MinValue;

                    if (DateTime.TryParseExact(System.IO.Path.GetFileNameWithoutExtension(tmp[i]).Substring(7),
                         "yyyy_MM_dd HH_mm_ss",
                         System.Globalization.CultureInfo.InvariantCulture,
                         System.Globalization.DateTimeStyles.None,
                         out fileSaved))
                    {
                        if (lastBackup < fileSaved)
                            lastBackup = fileSaved;
                    }
                }
            }
            #endregion

            #region Compare list of files in folder vs. list of files in latest .ZIP file
            if (lastBackup != DateTime.MinValue)
            {
                System.IO.Compression.ZipArchive zip = System.IO.Compression.ZipFile.Open(
                    System.IO.Path.Combine(dst, "Backup " + lastBackup.ToString("yyyy_MM_dd HH_mm_ss") + ".zip"),
                    System.IO.Compression.ZipArchiveMode.Read);
                List<string> filesRemaining = new List<string>(filesToSave);

                foreach (System.IO.Compression.ZipArchiveEntry entry in zip.Entries)
                {
                    bool wasFound = false;
                    for (int i = 0; i < filesRemaining.Count; i++)
                        if (entry.FullName.ToUpper() == Worker.stripSource(Path, filesRemaining[i].ToUpper()))
                        {
                            wasFound = true;
                            filesRemaining.RemoveAt(i);
                            break;
                        }

                    if (!wasFound)
                    {
                        lastBackup = DateTime.MinValue;
                        break;
                    }
                }

                zip.Dispose();

                while (filesRemaining.Count > 0)
                {
                    if (System.IO.File.Exists(
                        System.IO.Path.Combine(dst, "Backup " + lastBackup.ToString("yyyy_MM_dd HH_mm_ss") + System.IO.Path.GetFileName(filesRemaining[0]))
                        ))
                        filesRemaining.RemoveAt(0);
                    else
                        break;
                }

                if (filesRemaining.Count > 0)
                    lastBackup = DateTime.MinValue;
            }
            #endregion

            #region Do we need to create the zip file?
            if (lastModified > lastBackup)
            {


                // Create zip file
                string path = System.IO.Path.GetTempFileName();
                string pathTo = System.IO.Path.Combine(dst, "Backup " + Plan.LastAttmpted.ToString("yyyy_MM_dd HH_mm_ss") + ".zip");
                System.IO.Compression.ZipArchive zip = System.IO.Compression.ZipFile.Open(
                    path,
                    System.IO.Compression.ZipArchiveMode.Update);
                // Add each file
                for (int i = 0; i < filesToSave.Length; i++)
                {
                    System.IO.FileInfo info = new System.IO.FileInfo(filesToSave[i]);
                    #region If this is a small file (under 20mb), add to the zip file
                    if (info.Length < 20 * 1024 * 1024)
                    {
                        #region Every 250 files, close and reopen the ZIP file (this keeps memory from just growing stupidly)
                        if (i % 250 == 0)
                        {
                            zip.Dispose();
                            zip = System.IO.Compression.ZipFile.Open(
                                path,
                                System.IO.Compression.ZipArchiveMode.Update);
                        }
                        #endregion
                        #region Add file to .ZIP file
                        try
                        {
                            System.IO.Compression.ZipArchiveEntry entry =
                                System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(
                                    zip,
                                    filesToSave[i],
                                    Worker.stripSource(Path, filesToSave[i]));
                        }
                        #endregion
                        #region Catch I/O exceptions
                        catch (System.IO.IOException)
                        {
                            zip.Dispose();
                            System.IO.File.Delete(path);
                            if (!isProject)
                            {
                                string[] subDirs = Worker.listDirs(Plan, src);
                                NewDirectories.AddRange(subDirs);
                            }
                            return;
                        }
                        #endregion
                        #region Catch access exceptions
                        catch (System.UnauthorizedAccessException)
                        {
                            zip.Dispose();
                            System.IO.File.Delete(path);
                            if (!isProject)
                            {
                                string[] subDirs = Worker.listDirs(Plan, src);
                                NewDirectories.AddRange(subDirs);
                            }
                            return;
                        }
                        #endregion
                        #region Catch out of memory exceptions (close and reopen .ZIP file)
                        catch (OutOfMemoryException)
                        {
                            zip.Dispose();
                            zip = System.IO.Compression.ZipFile.Open(
                                path,
                                System.IO.Compression.ZipArchiveMode.Update);

                            System.IO.Compression.ZipArchiveEntry entry =
                                System.IO.Compression.ZipFileExtensions.CreateEntryFromFile(
                                    zip,
                                    filesToSave[i],
                                    Worker.stripSource(Path, filesToSave[i]),
                                     System.IO.Compression.CompressionLevel.NoCompression);
                        }
                        #endregion
                    }
                    #endregion
                    #region IF this is a large file, copy as is
                    else
                    {
                        if (!System.IO.Directory.Exists(
                            System.IO.Path.GetDirectoryName(pathTo)))
                            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(pathTo));

                        if (System.IO.File.Exists(System.IO.Path.GetDirectoryName(pathTo)))
                            System.IO.File.Delete(System.IO.Path.GetDirectoryName(pathTo));

                        System.IO.File.Copy(
                            filesToSave[i],
                            System.IO.Path.Combine(System.IO.Path.GetDirectoryName(pathTo), "Backup " + Plan.LastAttmpted.ToString("yyyy_MM_dd HH_mm_ss") + info.Name + ".tmp")
                            );

                        if (System.IO.File.Exists(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(pathTo), "Backup " + Plan.LastAttmpted.ToString("yyyy_MM_dd HH_mm_ss") + info.Name)))
                            System.IO.File.Delete(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(pathTo), "Backup " + Plan.LastAttmpted.ToString("yyyy_MM_dd HH_mm_ss") + info.Name));

                        System.IO.File.Move(
                            System.IO.Path.Combine(System.IO.Path.GetDirectoryName(pathTo), "Backup " + Plan.LastAttmpted.ToString("yyyy_MM_dd HH_mm_ss") + info.Name + ".tmp"),
                            System.IO.Path.Combine(System.IO.Path.GetDirectoryName(pathTo), "Backup " + Plan.LastAttmpted.ToString("yyyy_MM_dd HH_mm_ss") + info.Name)
                            );
                    }
                    #endregion
                }
                zip.Dispose();
                if (!System.IO.Directory.Exists(
                    System.IO.Path.GetDirectoryName(pathTo)))
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(pathTo));
                if (System.IO.File.Exists(pathTo + ".tmp"))
                    System.IO.File.Delete(pathTo + ".tmp");
                System.IO.File.Move(path, pathTo + ".tmp");
                if (System.IO.File.Exists(pathTo))
                    System.IO.File.Delete(pathTo);
                System.IO.File.Move(pathTo + ".tmp", pathTo);
            }
            #endregion

            #region Add all subdirectories
            if (!isProject)
            {
                string[] subDirs = Worker.listDirs(Plan, src);
                NewDirectories.AddRange(subDirs);
            }
            #endregion
        }
    }
}
