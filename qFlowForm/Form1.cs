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
using System.Threading;

namespace qFlowForm
{
    public partial class Form1 : Form
    {
        string[] args = null;
        public Form1(string[] args)
        {
            InitializeComponent();
            this.args = args;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.ShowInTaskbar = false;
            string ss;
            string formName = "";
            List<string> forms = new List<string>();
            if (args.Count() == 2)
            {
                if (args[0] == "start")
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
                else if (args[0] == "switch")
                {
                    //Form2 form2 = new Form2();
                    //switch (args[1])
                    //{
                    //    case "opening":
                    //        form2.BackgroundImage = Properties.Resources.opening;
                    //        break;
                    //    case "connecting":
                    //        form2.BackgroundImage = Properties.Resources.connecting;
                    //        break;
                    //    case "loading":
                    //        form2.BackgroundImage = Properties.Resources.loading;
                    //        break;
                    //    case "downloading":
                    //        form2.BackgroundImage = Properties.Resources.downloading;
                    //        break;
                    //    case "uploading":
                    //        form2.BackgroundImage = Properties.Resources.uploading;
                    //        break;
                    //    case "attachment":
                    //        form2.BackgroundImage = Properties.Resources.attachment;
                    //        break;
                    //    default:
                    //        form2.BackgroundImage = Properties.Resources.loading;
                    //        break;
                    //}
                    //form2.ShowDialog();
                    //this.Hide();
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
                    foreach (Process p in Process.GetProcessesByName("qFlowForm"))
                    {
                        try
                        {
                            ss = p.MainWindowTitle.Substring(p.MainWindowTitle.IndexOf("-") + 1, p.MainWindowTitle.Count() - 1 - p.MainWindowTitle.IndexOf("-")).Trim();
                            if (ss != "")
                            {
                                forms.Add(ss);
                                if (forms.Count() == 2)
                                {
                                    p.Kill();
                                    break;
                                }
                                formName = ss;
                            }
                        }
                        catch
                        {
                        }
                    }

                    //timer1.Enabled = true;
                    //if (forms.Count() == 2)
                    //{
                    //    foreach (Process p in Process.GetProcessesByName("qFlowForm"))
                    //    {
                    //        try
                    //        {
                    //            ss = p.MainWindowTitle.Substring(p.MainWindowTitle.IndexOf("-") + 1, p.MainWindowTitle.Count() - 1 - p.MainWindowTitle.IndexOf("-")).Trim();

                    //            if (ss == forms[0])
                    //            {
                    //                p.Kill();
                    //                break;
                    //            }
                    //        }
                    //        catch
                    //        {
                    //        }
                    //    }
                    //}
                }
                else if (args[0] == "stop")
                {
                    foreach (Process p in Process.GetProcessesByName("qFlowForm"))
                    {
                        try
                        {
                            ss = p.MainWindowTitle.Substring(p.MainWindowTitle.IndexOf("-") + 1, p.MainWindowTitle.Count() - 1 - p.MainWindowTitle.IndexOf("-")).Trim();
                            if (ss == "Form1" || ss == "Form2")
                            {
                                p.Kill();
                            }
                        }
                        catch
                        {
                        }
                    }
                   //Application.Exit();
                }
            }
        }

        private void killProcess()
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
                        if (forms.Count() == 2)
                        {
                            p.Kill();
                            break;
                        }
                        formName = ss;
                    }
                }
                catch
                {
                }
            }
        }
    }
}
