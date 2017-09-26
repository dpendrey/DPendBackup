using System;
using System.Collections.Generic;
using System.Text;

namespace DPend_Backup
{
    public enum LocationType : int
    {
        Directory = 0,
        FTP = 1,
        SqlServer = 2,
        CompressedDirectory = 100,
        CompressedFTP = 101,
        CompressedSqlServer = 102
    }
}
