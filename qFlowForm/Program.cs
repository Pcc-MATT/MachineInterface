using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace qFlowForm
{
    static class Program
    {
       
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //string[] args = new string[2];
            //args[0] = "switch";
            //args[1] = "opening";
            string ss;
            string formName="";
            List<string> forms = new List<string>();
            if (args.Count() == 2)
            {
                //foreach (Process p in Process.GetProcessesByName("qFlowForm"))
                //{
                //    try
                //    {
                //        ss = p.MainWindowTitle.Substring(p.MainWindowTitle.IndexOf("-") + 1, p.MainWindowTitle.Count() - 1 - p.MainWindowTitle.IndexOf("-")).Trim();
                //        if (ss != "")
                //        {
                //            forms.Add(ss);
                //            formName = ss;
                //        }
                //    }
                //    catch
                //    {
                //    }
                //}
                Application.Run(new Form1(args));
                //if (args[0] == "start")
                //{

                //    Application.Run(new Form1(args));
                //}
                //else if (args[0] == "switch")
                //{
                //    Application.Run(new Form2(args));
                //}
            }
            else if(args[0]=="stop")
            {
                foreach (Process p in Process.GetProcessesByName("qFlowForm"))
                {
                    try
                    {
                        ss = p.MainWindowTitle.Substring(p.MainWindowTitle.IndexOf("-") + 1, p.MainWindowTitle.Count() - 1 - p.MainWindowTitle.IndexOf("-")).Trim();
                        if (ss == "Form1"|| ss == "Form2")
                        {
                            p.Kill();
                        }
                    }
                    catch
                    {
                    }
                }
            }
           
        }
    }
}
