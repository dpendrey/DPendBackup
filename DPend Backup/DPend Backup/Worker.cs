using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPend_Backup
{
    public static class Worker
    {
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

        public static bool includeFile(Plan Plan, string Filename)
        {
            for (int i = 0; i < Plan.BlockFiles.Length; i++)
                if (CompareWildcard(Filename, Plan.BlockFiles[i]))
                    return false;

            for (int i = 0; i < Plan.AllowFiles.Length; i++)
                if (CompareWildcard(Filename, Plan.AllowFiles[i]))
                    return true;

            return false;
        }

        public static bool includeDirectory(Plan Plan, string Filename)
        {
            for (int i = 0; i < Plan.BlockDirs.Length; i++)
                if (CompareWildcard(Filename, Plan.BlockDirs[i]))
                    return false;

            for (int i = 0; i < Plan.AllowDirs.Length; i++)
                if (CompareWildcard(Filename, Plan.AllowDirs[i]))
                    return true;

            return false;
        }

        public static string[] listFiles(Plan Plan, string Path)
        {
            List<string> retVal = new List<string>();
            string[] tmp = System.IO.Directory.GetFiles(Path);
            for (int i = 0; i < tmp.Length; i++)
                if (includeFile(Plan, tmp[i]))
                    retVal.Add(tmp[i]);
            return retVal.ToArray();
        }

        public static string[] listFilesRecursive(Plan Plan, string Path)
        {
            List<string> retVal = new List<string>();

            // Add files from current directory
            retVal.AddRange(listFiles(Plan, Path));

            // Add files from all subdirectories
            foreach (string subDir in listDirs(Plan, Path))
                retVal.AddRange(listFilesRecursive(Plan, subDir));

            // Return value
            return retVal.ToArray();
        }

        public static string[] listDirs(Plan Plan, string Path)
        {
            List<string> retVal = new List<string>();
            string[] tmp = System.IO.Directory.GetDirectories(Path);
            for (int i = 0; i < tmp.Length; i++)
                if (includeDirectory(Plan, tmp[i]))
                    retVal.Add(tmp[i]);
            return retVal.ToArray();
        }

        public static string stripSource(string Source, string Path)
        {
            if (Path.ToUpper().StartsWith(Source.ToUpper()))
            {
                Path = Path.Substring(Source.Length);
                if (Path.StartsWith("/") || Path.StartsWith("\\"))
                    Path = Path.Substring(1);
            }

            return Path;
        }

        public static bool IsProjectDirectory(Plan Plan, string Path)
        {
            string[] files = System.IO.Directory.GetFiles(Path);
            foreach (string file in files)
                if (IsProjectFile(Plan, file))
                    return true;
            return false;
        }

        public static bool IsProjectFile(Plan Plan, string Path)
        {
            foreach (string projectFile in Plan.ProjectFiles)
            {
                if (CompareWildcard(projectFile, System.IO.Path.GetFileName(Path)))
                    return true;
            }

            return false;
        }
    }
}
