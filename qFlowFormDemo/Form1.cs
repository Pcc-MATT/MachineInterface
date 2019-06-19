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

namespace qFlowFormDemo
{
    // 1.定义委托
    public delegate void DelReadStdOutput(string result);
    public delegate void DelReadErrOutput(string result);
    public partial class Form1 : Form
    {
        string[] args = null;
        // 2.定义委托事件
        public event DelReadStdOutput ReadStdOutput;
        public event DelReadErrOutput ReadErrOutput;
        public Form1(string[] args)
        {
            InitializeComponent();
            this.args = args;
            Init();
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
                    if (args[1] == "initializing")
                    {
                       this.BackgroundImage = Properties.Resources.Initializing;
                    }
                    if (args[1] == "connecting")
                    {
                        this.BackgroundImage = Properties.Resources.connecting;
                    }
                    if (args[1] == "loading")
                    {
                        this.BackgroundImage = Properties.Resources.loading;
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
                    if (args[1] == "initializing")
                    {
                        this.BackgroundImage = Properties.Resources.Initializing;
                    }
                    if (args[1] == "connecting")
                    {
                        this.BackgroundImage = Properties.Resources.connecting;
                    }
                    if (args[1] == "loading")
                    {
                        this.BackgroundImage = Properties.Resources.loading;
                    }
                   
                    foreach (Process p in Process.GetProcessesByName("qFlowFormDemo"))
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
                    foreach (Process p in Process.GetProcessesByName("qFlowFormDemo"))
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
            else if (args.Count() == 0)
            {
                this.BackgroundImage = Properties.Resources.Initializing;
                timer1.Interval = Properties.Settings.Default.startImageHoldTime;
                timer1.Enabled = true;
                //System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();

                ////设置启动进程的初始目录

                //info.WorkingDirectory = Properties.Settings.Default.startPtxPath;//Application.StartupPath

                ////设置启动进程的应用程序或文档名

                //info.FileName = Properties.Settings.Default.startPtxName;

                ////设置启动进程的参数

                //info.Arguments = "-nosplash";

                ////启动由包含进程启动信息的进程资源

                try

                {

                    //System.Diagnostics.Process.Start(info);
                    string ptxPath = Properties.Settings.Default.startPtxPath + Properties.Settings.Default.startPtxName;
                    string openPtxStr = " -open " + "\"" + ptxPath + "\"" + " -nosplash -maximize";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe\"" + 
                    Thread.Sleep(1500);
                    RealAction("C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe", openPtxStr);
                }
                catch (System.ComponentModel.Win32Exception we)
                {
                    MessageBox.Show(this, we.Message);
                    return;

                }
            }
        }
        private void Init()
        {
            //3.将相应函数注册到委托事件中
            ReadStdOutput += new DelReadStdOutput(ReadStdOutputAction);
            ReadErrOutput += new DelReadErrOutput(ReadErrOutputAction);
        }
        private void RealAction(string StartFileName, string StartFileArg)
        {
            Process CmdProcess = new Process();
            CmdProcess.StartInfo.FileName = StartFileName;      // 命令
            CmdProcess.StartInfo.Arguments = StartFileArg;      // 参数

            CmdProcess.StartInfo.CreateNoWindow = true;         // 不创建新窗口
            CmdProcess.StartInfo.UseShellExecute = false;
            CmdProcess.StartInfo.RedirectStandardInput = true;  // 重定向输入
            CmdProcess.StartInfo.RedirectStandardOutput = true; // 重定向标准输出
            CmdProcess.StartInfo.RedirectStandardError = true;  // 重定向错误输出
                                                                //CmdProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            CmdProcess.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);
            CmdProcess.ErrorDataReceived += new DataReceivedEventHandler(p_ErrorDataReceived);

            CmdProcess.EnableRaisingEvents = true;                      // 启用Exited事件
            CmdProcess.Exited += new EventHandler(CmdProcess_Exited);   // 注册进程结束事件

            CmdProcess.Start();
            CmdProcess.BeginOutputReadLine();
            CmdProcess.BeginErrorReadLine();

            // 如果打开注释，则以同步方式执行命令，此例子中用Exited事件异步执行。
            // CmdProcess.WaitForExit();     
        }
        private void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                // 4. 异步调用，需要invoke
                this.Invoke(ReadStdOutput, new object[] { e.Data });
            }
        }

        private void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                this.Invoke(ReadErrOutput, new object[] { e.Data });
            }
        }

        private void ReadStdOutputAction(string result)
        {
            //this.label2.Text+=(result + "\r\n");
        }

        private void ReadErrOutputAction(string result)
        {
            //this.label2.Text+=(result + "\r\n");
        }

        private void CmdProcess_Exited(object sender, EventArgs e)
        {
            // 执行结束后触发
        }
        private void killProcess()
        {
            string ss;
            string formName = "";
            List<string> forms = new List<string>();
            foreach (Process p in Process.GetProcessesByName("qFlowFormDemo"))
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            Application.Exit();
        }
    }
}
