using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace qFlowForm
{
    public partial class Form2 : Form
    {
        string[] args = null;
        public Form2(string[] args)
        {
            InitializeComponent();
            this.args = args;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
        
            if (args.Count() == 2)
            {
                if (args[0] == "start")
                {
                  
                }
                else if (args[0] == "switch")
                {
                    if (args[1] == "opening")
                    {
                        this.BackgroundImage = Properties.Resources.opening;
                    }
                    if (args[1] == "connecting")
                    {
                        this.BackgroundImage = Properties.Resources.connecting;
                    }
                    if (args[1] == "loading")
                    {
                        this.BackgroundImage = Properties.Resources.loading;
                    }
                    if (args[1] == "downloading")
                    {
                        this.BackgroundImage = Properties.Resources.downloading;
                    }
                    if (args[1] == "uploading")
                    {
                        this.BackgroundImage = Properties.Resources.uploading;
                    }
                    if (args[1] == "attachment")
                    {
                        this.BackgroundImage = Properties.Resources.attachment;
                    }
                }
                else if (args[0] == "stop")
                {
                    Application.Exit();
                }
            }
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string ss;
            string formName = "";
            List<string> forms = new List<string>();
            foreach (Process p in Process.GetProcessesByName("qFlowForm"))
            {
                try
                {
                    ss = p.MainWindowTitle.Substring(p.MainWindowTitle.IndexOf("-") + 1, p.MainWindowTitle.Count() - 1 - p.MainWindowTitle.IndexOf("-")).Trim();
                    if (ss != "")
                    {
                        forms.Add(ss);
                        formName = ss;
                    }
                }
                catch
                {
                }
            }
            foreach (Process p in Process.GetProcessesByName("qFlowForm"))
            {
                try
                {
                    ss = p.MainWindowTitle.Substring(p.MainWindowTitle.IndexOf("-") + 1, p.MainWindowTitle.Count() - 1 - p.MainWindowTitle.IndexOf("-")).Trim();
                    if (ss == forms[0])
                    {
                        p.Kill();
                        break;
                    }
                }
                catch
                {
                }
            }
            timer1.Enabled = false;
        }
    }
}
