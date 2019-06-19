using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PiWebProcessManager
{
    public partial class Form1 : Form
    {
        string[] args = null;
        public Form1(string[] args)
        {
            InitializeComponent();
            this.args = args;
        }
        private void killProcess(string processName,string piwebProcessName)
        {
            string ss;
            foreach (Process p in Process.GetProcessesByName(piwebProcessName))
            {
                try
                {
                    if (processName == "All")
                    {
                         p.Kill();
                    }
                    else
                    {
                        ss = p.MainWindowTitle.Trim().Substring(0, p.MainWindowTitle.IndexOf("- PiWeb " + piwebProcessName));
                        if (ss != "" && ss.Contains(processName))
                        {
                            p.Kill();
                        }
                    }
                   
                }
                catch
                {
                }
            }
        }
        List<string> strs=new List<string>();
        private void button1_Click(object sender, EventArgs e)
        {

           // killProcess("工程师", "Monitor");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (args.Count() != 0)
            {
                if (args.Count() == 2)
                {
                    if (args[0] == "Monitor")
                    {
                        killProcess(args[1], "Monitor");
                    }
                    Application.Exit();
                }
            }
            Application.Exit();
        }
    }
}
