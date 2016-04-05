using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;

namespace DPend_Backup
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();

            /*
            Plan p = new Plan();
            p.Name = "DPend";
            p.Source = @"C:\DPend\";
            p.Destination = @"\\MEDIASERVER\DataStorage\DPend\Backups\DPend";
            p.TimeSpan = 1;
            p.TimeSpanType = TimeSpanType.Days;
            p.RetryTimeSpan = 10;
            p.RetryTimeSpanType = TimeSpanType.Minutes;
            p.NumberWorkers = 3;

            Server.AddPlan(p);
            Server.SaveSettings();
            */

            System.Threading.Thread thr = new System.Threading.Thread(new System.Threading.ThreadStart(Server.Start));
            thr.Priority = System.Threading.ThreadPriority.Lowest;
            thr.Start();
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            SystemStatus status = Server.Status;
            StringBuilder build=new StringBuilder();

            build.AppendLine("Status: " + status.Status.ToString());
            if (status.Plan == null)
                build.AppendLine("Not running any backups");
            else
            {
                build.AppendLine("Running: " + status.Plan.Name);
                build.AppendLine(status.NumberWorkers.ToString()+" workers");

                if (status.BytesChecked > 1024 * 1024 * 1024)
                    build.AppendLine("Checked " + status.FilesChecked.ToString() + " worth " + (status.BytesChecked / (1024 * 1024 * 1024.0)).ToString("0.00") + "GB");
                else if (status.BytesChecked > 1024 * 1024 )
                    build.AppendLine("Checked " + status.FilesChecked.ToString() + " worth " + (status.BytesChecked / (1024 * 1024.0)).ToString("0.00") + "MB");
                else if (status.BytesChecked > 1024 )
                    build.AppendLine("Checked " + status.FilesChecked.ToString() + " worth " + (status.BytesChecked / (1024.0)).ToString("0.00") + "kB");
                else 
                    build.AppendLine("Checked " + status.FilesChecked.ToString() + " worth " + status.BytesChecked .ToString("0") + "B");


                if (status.BytesCopied > 1024 * 1024 * 1024)
                    build.AppendLine("Copied " + status.FilesCopied.ToString() + " worth " + (status.BytesCopied / (1024 * 1024 * 1024.0)).ToString("0.00000") + "GB");
                else if (status.BytesCopied > 1024 * 1024)
                    build.AppendLine("Copied " + status.FilesCopied.ToString() + " worth " + (status.BytesCopied / (1024 * 1024.0)).ToString("0.00") + "MB");
                else if (status.BytesCopied > 1024)
                    build.AppendLine("Copied " + status.FilesCopied.ToString() + " worth " + (status.BytesCopied / (1024.0)).ToString("0.00") + "kB");
                else
                    build.AppendLine("Copied " + status.FilesCopied.ToString() + " worth " + status.BytesCopied.ToString("0") + "B");

                build.AppendLine(status.NumberWorkers.ToString() + " workers active");
                build.AppendLine(status.FilesWaiting.ToString() + " files waiting, and " + status.DirectoriesWaiting.ToString() + " waiting");
            }

            lbStatus.Text = build.ToString();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Server.Stop();
        }
    }
}
