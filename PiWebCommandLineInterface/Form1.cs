using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace PiWebCommandLineInterface
{
    // 1.定义委托
    public delegate void DelReadStdOutput(string result);
    public delegate void DelReadErrOutput(string result);

    public partial class Form1 : Form
    {
        // 2.定义委托事件
        public event DelReadStdOutput ReadStdOutput;
        public event DelReadErrOutput ReadErrOutput;

        string[] args = null;
        public Form1(string[] args)
        {
            InitializeComponent();
            this.args = args;
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
        private bool excuteCommandLine()
        {
            bool paraCorrect = true;
            if (args[0] == "SerialNumber")
            {
                DirectoryInfo path_exe = new DirectoryInfo(Application.StartupPath); //exe目录
                string currentpath = path_exe.FullName;
                String path = path_exe.Parent.FullName; //上一级的目录
                string ptxPath = args[1];
                if (File.Exists(ptxPath))
                {
                    string openPtxStr = "";
                    changeMESLfile(currentpath + @"\SerialNumber.msel", currentpath + @"\SerialNumberNew.msel");
                    openPtxStr = " -open " + "\"" + ptxPath + "\"" + " -searchCriteria " + "\"" + currentpath + "\\SerialNumberNew.msel\"" + " -nosplash -maximize";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe\"" + 
                    RealAction("C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe", openPtxStr);
                    //this.BackgroundImage = Properties.Resources.qFlowLoading;
                    Thread.Sleep(int.Parse(args[3]));

                    paraCorrect = true;
                }
            }
            else
            {
                if (args.Count() == 6)//修改common part并修改jobnumber和tasknumber的筛选条件
                {
                    if (args[2] != null && args[2] != "None")//args[2]
                    {
                        if (int.Parse(args[2]) >= 2)//jobStatus
                        {
                            if (args[3] != null && args[3] != "None" && args[0] != null && args[0] != "None" && args[1] != null && args[1] != "None" && args[4] != null && args[4] != "None")//args[3]=commonPart
                            {
                                DirectoryInfo path_exe = new DirectoryInfo(Application.StartupPath); //exe目录
                                string currentpath = path_exe.FullName;
                                String path = path_exe.Parent.Parent.FullName; //上两级的目录
                                string ptxPath = args[4];
                                if (File.Exists(ptxPath))
                                {
                                    string openPtxStr = "";
                                    //改变搜索里的jobNumber和taskNumber
                                    if (args[3].Contains("Job_Management"))
                                    {
                                        changeMESLfile(currentpath + @"\JobNum&TaskNum.msel", currentpath + @"\JobNum&TaskNumNew.msel");
                                        openPtxStr = " -open " + "\"" + ptxPath + "\"" + " -initiallyCheckedGenericDataBindingPart " + "\"" + args[3] + "\"" + " -searchCriteria " + "\"" + currentpath + "\\JobNum&TaskNumNew.msel\"" + " -nosplash -maximize";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe\"" + 
                                    }
                                    else if (args[3].Contains("Inspection_List"))
                                    {
                                        changeMESLfile(currentpath + @"\ProductIdent&Software.msel", currentpath + @"\ProductIdent&SoftwareNew.msel");
                                        openPtxStr = " -open " + "\"" + ptxPath + "\"" + " -initiallyCheckedGenericDataBindingPart " + "\"" + args[3] + "\"" + " -searchCriteria " + "\"" + currentpath + "\\ProductIdent&SoftwareNew.msel\"" + " -nosplash -maximize";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe\"" + 
                                    }
                                    else
                                    {
                                        changeMESLfile(currentpath + @"\JobNum&TaskNum.msel", currentpath + @"\JobNum&TaskNumNew.msel");
                                        openPtxStr = " -open " + "\"" + ptxPath + "\"" + " -initiallyCheckedGenericDataBindingPart " + "\"" + args[3] + "\"" + " -searchCriteria " + "\"" + currentpath + "\\JobNum&TaskNumNew.msel\"" + " -nosplash -maximize";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe\"" + 
                                    }

                                    //string commandStr = " -open " + "\"" + ptxPath + "\"" + " -changeCommonParentPartPath " + "\"" + args[3] + "\"" + " -searchCriteria " + "\"" + currentpath + "\\JobNum&TaskNumNew.msel" + "\"" + " -Save " + "\"" + ptxPath + "\"";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Cmdmon.exe\"" + 

                                    //RealAction("C:\\Program Files\\Zeiss\\PiWeb\\Cmdmon.exe", commandStr);
                                    //this.BackgroundImage = Properties.Resources.qFlowConnecting;
                                    //Thread.Sleep(1500);
                                    RealAction("C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe", openPtxStr);
                                    //this.BackgroundImage = Properties.Resources.qFlowLoading;
                                    Thread.Sleep(int.Parse(args[5]));

                                    paraCorrect = true;
                                }
                            }
                        }
                        else
                        {
                            timer1.Enabled = false;
                            MessageBox.Show("JobStatus状态不在应该的状态，请检查JobStatus！");

                        }
                    }
                }
                else if (args.Count() == 4)//修改jobnumber 和tasknumber的筛选条件
                {
                    string ptxPath = args[2];
                    if (File.Exists(ptxPath))
                    {
                        DirectoryInfo path_exe = new DirectoryInfo(Application.StartupPath); //exe目录
                        string currentpath = path_exe.FullName;
                        //改变搜索里的jobNumber和taskNumber
                        changeMESLfile(currentpath + @"\JobNum&TaskNum.msel", currentpath + @"\JobNum&TaskNumNew.msel");
                        string commandStr = " -open " + "\"" + ptxPath + "\"" + " -searchCriteria " + "\"" + currentpath + "\\JobNum&TaskNumNew.msel" + "\"" + " -Save " + "\"" + ptxPath + "\"";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Cmdmon.exe\"" + 
                        string openPtxStr = " -open " + "\"" + ptxPath + "\"" + " -nosplash -maximize";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe\"" + 
                        RealAction("C:\\Program Files\\Zeiss\\PiWeb\\Cmdmon.exe", commandStr);
                        Thread.Sleep(1500);
                        RealAction("C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe", openPtxStr);

                        Thread.Sleep(int.Parse(args[3]));
                        paraCorrect = true;
                    }
                }
                else if (args.Count() == 2)//打开report
                {
                    string ptxPath = args[0];
                    if (File.Exists(ptxPath))
                    {
                        //改变搜索里的jobNumber和taskNumber
                        string openPtxStr = " -open " + "\"" + ptxPath + "\"" + " -nosplash -maximize";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe\"" + 
                        RealAction("C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe", openPtxStr);
                        Thread.Sleep(int.Parse(args[1]));
                        paraCorrect = true;
                    }
                }
                else if (args.Count() == 3)//修改part ident
                {
                    string ptxPath = args[1];
                    if (File.Exists(ptxPath))
                    {
                        DirectoryInfo path_exe = new DirectoryInfo(Application.StartupPath); //exe目录
                        string currentpath = path_exe.FullName;
                        //改变搜索里的jobNumber和taskNumber
                        changeMESLfile(currentpath + @"\Product_ident.msel", currentpath + @"\Product_identNew.msel");
                        string commandStr = " -open " + "\"" + ptxPath + "\"" + " -searchCriteria " + "\"" + currentpath + "\\Product_identNew.msel" + "\"" + " -Save " + "\"" + ptxPath + "\"";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Cmdmon.exe\"" + 
                        string openPtxStr = " -open " + "\"" + ptxPath + "\"" + " -nosplash -maximize";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe\"" + 
                        RealAction("C:\\Program Files\\Zeiss\\PiWeb\\Cmdmon.exe", commandStr);
                        //this.BackgroundImage = Properties.Resources.qFlowConnecting;
                        Thread.Sleep(1500);
                        RealAction("C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe", openPtxStr);
                        //this.BackgroundImage = Properties.Resources.qFlowLoading;
                        Thread.Sleep(int.Parse(args[2]));
                        paraCorrect = true;
                    }
                }
                else
                {
                    paraCorrect = false;
                }
            }

            return paraCorrect;
        }
        /// <summary>
        /// 打开控制台执行拼接完成的批处理命令字符串
        /// </summary>
        /// <param name="inputAction">需要执行的命令委托方法：每次调用 <paramref name="inputAction"/> 中的参数都会执行一次</param>
        private string ExecBatCommand(Action<Action<string>> inputAction)
        {
            Process pro = null;
            StreamWriter sIn = null;
            StreamReader sOut = null;
            try
            {
                pro = new Process();
                pro.StartInfo.FileName = "cmd.exe";
                pro.StartInfo.UseShellExecute = false;
                pro.StartInfo.CreateNoWindow = true;
                pro.StartInfo.RedirectStandardInput = true;
                pro.StartInfo.RedirectStandardOutput = true;
                pro.StartInfo.RedirectStandardError = true;

                pro.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                pro.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

                pro.Start();
                sIn = pro.StandardInput;
                sIn.AutoFlush = true;

                pro.BeginOutputReadLine();
                inputAction(value => sIn.WriteLine(value));

                //pro.WaitForExit();
                //pro.Close();
                StreamReader reader = pro.StandardOutput;//截取输出流
                StreamReader error = pro.StandardError;//截取错误信息
                string str = reader.ReadToEnd() + error.ReadToEnd();
                pro.WaitForExit();//等待程序执行完退出进程
                pro.Close();
                return str;
                // return pro.HasExited;

            }
            finally
            {
                if (pro != null && !pro.HasExited)
                    pro.Kill();

                if (sIn != null)
                    sIn.Close();
                if (sOut != null)
                    sOut.Close();
                if (pro != null)
                    pro.Close();
            }
        }

        private void changeMESLfile(string filePath, string newFilePath)
        {
            List<string> lines = new List<string>();
            StreamReader sr = new StreamReader(filePath);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                lines.Add(line);
            }
            sr.Close();
            List<string> newLines = new List<string>();
            foreach (var str in lines)
            {
                string temp = str;
                if (str.Contains("TaskNumberValue"))
                {
                    temp = str.Replace("TaskNumberValue", args[1]);
                }
                if (str.Contains("JobNumberValue"))
                {
                    temp = str.Replace("JobNumberValue", args[0]);
                }
                if (str.Contains("000000000000"))
                {
                    temp = str.Replace("000000000000", args[0]);
                }
                if (str.Contains("ProductIdentValue"))
                {
                    temp = str.Replace("ProductIdentValue", args[0]);
                }
                if (str.Contains("SoftwareValue"))
                {
                    temp = str.Replace("SoftwareValue", args[1]);
                }
                if (str.Contains("serial_Number"))
                {
                    temp = str.Replace("serial_Number", args[2]);
                }
                newLines.Add(temp);
            }
            StreamWriter sw = new StreamWriter(newFilePath);
            foreach (var tmp in newLines)
            {
                sw.WriteLine(tmp);
            }
            sw.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Opacity = 1;
            this.BackgroundImage = Properties.Resources.loading;
            KillProcess("Cmdmon");

            //args = new String[6];
            //args[0] = "JOB190305134616";//JOB190305134616  JOB190305185327
            //args[1] = "1";
            //args[2] = "3";
            //args[3] = "/Programs/Zeiss/9";
            //args[4] = @"C:\Users\ZCQIZHAO\source\repos\MachineInterface\PiWebCommandLineInterface\Self_Check\Self_Check.ptx";
            //args[5] = "4000";
            //args = new String[4];
            //args[0] = "JOB190305204552";//JOB190305134616  JOB190305185327
            //args[1] = "2";
            //args[2] = @"C:\Users\ZCQIZHAO\source\repos\MachineInterface\PiWebCommandLineInterface\Self_Check\Self_Check_Done.ptx";
            //args[3] = "4000";
            timer1.Enabled = true;
        }
        private void KillProcess(string processName)
        {
            Process[] myproc = Process.GetProcesses();
            foreach (Process item in myproc)
            {
                if (item.ProcessName == processName)
                {
                    item.Kill();
                }
            }
        }
        int count = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                count++;


                if (excuteCommandLine())
                {

                    Application.Exit();
                }
                else
                {
                    if (count == 1)
                    {
                        MessageBox.Show("传入参数不完整！");
                        timer1.Enabled = false;
                        Application.Exit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            //count++;
            //switch (count)
            //{
            //    case 1:
            //        label2.Text = "连接数据库...";
            //        break;
            //    case 2:
            //        label2.Text = "数据库连接成功！";
            //        break;
            //    case 3:
            //        label2.Text = "导入测量路径...";
            //        break;
            //    case 4:
            //        label2.Text = "测量路径导入完成！";
            //        break;
            //    case 5:
            //        label2.Text = "导入过滤条件...";
            //        break;
            //    case 6:
            //        label2.Text = "开始加载测量程序...";
            //        break;
            //    default:
            //        label2.Text = "加载测量程序中...";
            //        break;
            //}
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
