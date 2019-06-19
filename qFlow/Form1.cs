﻿using System;
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

namespace qFlow
{
    // 1.定义委托
    public delegate void DelReadStdOutput(string result);
    public delegate void DelReadErrOutput(string result);
    public partial class Form1 : Form
    {
        // 2.定义委托事件
        public event DelReadStdOutput ReadStdOutput;
        public event DelReadErrOutput ReadErrOutput;
        public Form1()
        {
            InitializeComponent();
            Init();
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
        private void Form1_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.Role == "Manager")
            {
                this.BackgroundImage = Properties.Resources.managerLoading;
            }else if (Properties.Settings.Default.Role == "Management")
            {
                this.BackgroundImage = Properties.Resources.managementPage;
            }
            else if(Properties.Settings.Default.Role == "Engineer")
            {
                this.BackgroundImage = Properties.Resources.engineerLoading;
            }else
            {
                this.BackgroundImage = Properties.Resources.measurePage;
            }
            timer1.Interval = Properties.Settings.Default.startImageHoldTime;
            timer1.Enabled = true;
            try
            {
                string ptxPath = Properties.Settings.Default.startPtxPath + Properties.Settings.Default.startPtxName;
                string openPtxStr = " -open " + "\"" + ptxPath + "\"" + " -nosplash -maximize";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe\"" + 
                Thread.Sleep(1000);
                RealAction("C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe", openPtxStr);
            }
            catch (System.ComponentModel.Win32Exception we)
            {
                MessageBox.Show(this, we.Message);
                return;
            }

            //timer1.Interval = Properties.Settings.Default.startImageHoldTime;
            //timer1.Enabled = true;
            //System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();

            ////设置启动进程的初始目录

            //info.WorkingDirectory = Properties.Settings.Default.startPtxPath;//Application.StartupPath

            ////设置启动进程的应用程序或文档名

            //info.FileName = Properties.Settings.Default.startPtxName;

            ////设置启动进程的参数

            //info.Arguments = "";

            ////启动由包含进程启动信息的进程资源

            //try

            //{

            //    System.Diagnostics.Process.Start(info);
            //    //timer1.Enabled = false;
            //    //Application.Exit();
            //}

            //catch (System.ComponentModel.Win32Exception we)

            //{

            //    MessageBox.Show(this, we.Message);

            //    return;

            //}
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            Application.Exit();
        }
    }
}
