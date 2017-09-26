using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPend_Backup.DestTypes
{
    public class Directory : DestType
    {
        public override void SaveFile(Plan Plan, PlanExecutionStatus Status, string Path)
        {
            string src = System.IO.Path.Combine(Plan.Source, Path);
            string dst = System.IO.Path.Combine(Plan.Destination, Path);
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
                    while (numRead > 0 && Server.Status.Status == DPend_Backup.Status.Running)
                    {
                        write.Write(buff, 0, numRead);
                        Status.BytesCopied += numRead;
                        numRead = read.Read(buff, 0, buff.Length);
                    }

                    Status.FilesCopied++;

                    write.Flush();
                    write.Close();
                    read.Close();

                    if (Server.Status.Status == DPend_Backup.Status.Running)
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

            Status.FilesScanned++;
            Status.BytesScanned += iSrc.Length;
        }

        public override void SaveDirectory(Plan Plan, PlanExecutionStatus Status, string Path, List<string> NewDirectories, List<string> NewFiles)
        {
            string src = System.IO.Path.Combine(Plan.Source, Path);
            string dst = System.IO.Path.Combine(Plan.Destination, Path);

            string[] tmp;

            #region Scan directories
            try
            {
                tmp = System.IO.Directory.GetDirectories(src);
                for (int i = 0; i < tmp.Length; i++)
                {
                    if (tmp[i].StartsWith(Plan.Source))
                    {
                        tmp[i] = tmp[i].Substring(Plan.Source.Length);

                        bool allowScan = false;
                        foreach (string filter in Plan.AllowDirs)
                        {
                            if (Worker.CompareWildcard(tmp[i], filter))
                            {
                                allowScan = true;
                                break;
                            }
                        }
                        if (allowScan)
                        {
                            foreach (string filter in Plan.BlockDirs)
                            {
                                if (Worker.CompareWildcard(tmp[i], filter))
                                {
                                    allowScan = false;
                                    break;
                                }
                            }

                            if (allowScan)
                            {
                                NewDirectories.Add(tmp[i]);
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
                    if (tmp[i].StartsWith(Plan.Source))
                    {
                        tmp[i] = tmp[i].Substring(Plan.Source.Length);
                        if (tmp[i] == null)
                        {
                        }

                        bool allowScan = false;
                        foreach (string filter in Plan.AllowFiles)
                        {
                            if (Worker.CompareWildcard(tmp[i], filter))
                            {
                                allowScan = true;
                                break;
                            }
                        }
                        if (allowScan)
                        {
                            foreach (string filter in Plan.BlockFiles)
                            {
                                if (Worker.CompareWildcard(tmp[i], filter))
                                {
                                    allowScan = false;
                                    break;
                                }
                            }

                            if (allowScan)
                            {
                                NewFiles.Add(tmp[i]);
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
        }
    }

}
