using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPend_Backup.DestTypes
{
    public abstract class DestType
    {
        public abstract void SaveDirectory(Plan Plan, PlanExecutionStatus Status, string Path, List<string> NewDirectories, List<string> NewFiles);
        public abstract void SaveFile(Plan Plan, PlanExecutionStatus Status, string Path);
    }
}
