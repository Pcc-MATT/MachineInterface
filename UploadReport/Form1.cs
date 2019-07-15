using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zeiss.IMT.PiWeb.Api.Common.Data;
using Zeiss.IMT.PiWeb.Api.DataService.Rest;

namespace UploadReport
{
    public partial class Form1 : Form
    {
        string[] args = null;
        public Form1(string[] args)
        {
            InitializeComponent();
            this.args = args;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                if (args.Count() >0)
                {
                    //读取配置
                    DirectoryInfo path_exe = new DirectoryInfo(Application.StartupPath); //exe目录
                    String path = path_exe.Parent.FullName; //上级的目录
                    string filepath = path + @"\Relative_Files\config.json";
                    readJsonPara(filepath);
                    //连接piweb server
                    checkConnect2PiWebServer();
                    //加载Piweb数据库数据
                    var characteristicPath = PathHelper.RoundtripString2PathInformation("P:/Demo/");
                    if (args.Count() == 1)
                    {
                        //args 第一位传serial number
                        await fetchMeasurements(characteristicPath, args[0]);
                    }
                }
                else
                {
                    MessageBox.Show("PiWeb传入参数有误，请查看PiWeb");
                    Application.Exit();
                }
                
                
            }catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
           
            
        }
        Dictionary<string, object> PiWebHost_Data;
        Dictionary<string, object> BackupData = new Dictionary<string, object>();
        private bool readJsonPara(string filePath)
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fs, new UTF8Encoding(false));
                Dictionary<string, object> valuePairs = new Dictionary<string, object>();
                valuePairs = JsonConvert.DeserializeObject<Dictionary<string, object>>(sr.ReadToEnd());
                sr.Close();
                var contentPut = JsonConvert.SerializeObject(valuePairs, Formatting.Indented);
                var ss3 = JsonConvert.SerializeObject(valuePairs["PiWebHost"], Formatting.Indented);
                PiWebHost_Data = JsonConvert.DeserializeObject<Dictionary<string, object>>(ss3);
                var ss4 = JsonConvert.SerializeObject(valuePairs["BackupData"], Formatting.Indented);
                BackupData= JsonConvert.DeserializeObject<Dictionary<string, object>>(ss4);
                return true;
            }
            catch (Exception ex)
            {

                MessageBox.Show("读取config.json出错" + ex);
                return false;
            }
        }
        private Uri _ServerUri;
        private DataServiceRestClient _RestDataServiceClient;
        bool connect2PiWebServer;
        private async void checkConnect2PiWebServer()
        {
            _ServerUri = new Uri(PiWebHost_Data["Main"].ToString());
            _RestDataServiceClient = new DataServiceRestClient(_ServerUri);
            await CheckConnection();
        }
        /// <summary>
        /// This methods fetches the service information from both the data service and the raw data service.
        /// The service information contains general information about the services like its version and the 
        /// servers feature set. Fetching the service information can also be used for connection check purposes
        /// since it's guaranteed that the check is fast and does not cause any noticeable server load.
        /// </summary>
        private async Task CheckConnection()
        {
            // Data Service
            try
            {
                //label18.Text = "PiWeb连接开始...";
                //pictureBox5.Image = null;
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var serviceInformatrion = await _RestDataServiceClient.GetServiceInformation();
                sw.Stop();
                //pictureBox5.Image = Properties.Resources.选中;
                //label18.Text = "PiWeb连接成功";
                connect2PiWebServer = true;
            }
            catch (Exception ex)
            {
                pictureBox1.Image = Properties.Resources.删除;
                label2.Text = "PiWeb连接失败";
                connect2PiWebServer = false;
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
		/// This method fetches the most recent 100 measurements for the selected part. Please have a look at the other properties inside 
		/// the filter class to understand all possibilities of filtering.
		/// </summary>
		private async Task updateMeasurements2CheckDemoReport(PathInformation partPath,string serialNumber)
        {
            SimpleMeasurement[] _Measurements = new SimpleMeasurement[0];
            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                _Measurements = (await _RestDataServiceClient.GetMeasurements(partPath, new MeasurementFilterAttributes
                {
                    SearchCondition = new GenericSearchAttributeCondition
                    {
                        Attribute=(ushort)14,
                        Operation=Operation.Equal,
                        Value=serialNumber
                    }
                })).ToArray();//
                sw.Stop();
                foreach(var mes in _Measurements)
                {
                    mes.SetAttribute((ushort)20057, "已完成");
                }
                if (_Measurements.Count() > 0)
                {
                    await _RestDataServiceClient.UpdateMeasurements(_Measurements);
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        string demoType = "";
        string customer = "";
        string serialNumber = "";
        private async Task fetchMeasurements(PathInformation partPath, string serialNumber)
        {
            SimpleMeasurement[] _Measurements = new SimpleMeasurement[0];
            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                _Measurements = (await _RestDataServiceClient.GetMeasurements(partPath, new MeasurementFilterAttributes
                {
                    SearchCondition = new GenericSearchAttributeCondition
                    {
                        Attribute = (ushort)14,
                        Operation = Operation.Equal,
                        Value = serialNumber
                    }
                })).ToArray();//
                sw.Stop();
                foreach (var mes in _Measurements)
                {
                    if (mes.GetAttribute((ushort)20041).Value == "1")
                    {
                        demoType = mes.GetAttribute((ushort)20043).Value;
                        customer = mes.GetAttribute((ushort)1062).Value;
                    }
                   
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in ofd.FileNames)
                {
                    textBox1.Text += file + ";";
                }
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            
            //check file server connect
            string fileServerIP = BackupData["IP"].ToString();
            string fileServerParentPath = BackupData["ParentPath"].ToString();
            string fileServerUserName = BackupData["UserName"].ToString();
            string fileServerUserPwd = BackupData["Password"].ToString();
            string fileServerPath =@"/" + fileServerParentPath;
            fileServerPath= fileServerPath.Replace("\\","/");
            fileServerPath = "\\\\" + fileServerIP + fileServerPath;
            string ss= Path.Combine("\\\\" + fileServerIP, fileServerParentPath);
            //注册共享文件夹
            connectState(fileServerPath, fileServerUserName, fileServerUserPwd);
            //
            if (checkFileServerConnect(fileServerIP))
            {
                //copy local File to File server
                string childPath = demoType + "\\" + demoType.Substring(0, demoType.IndexOf("_")) + "-" + args[0] + "-" + customer+"\\Demo_Report";
               if(copyFile2FileServer("\\\\" + fileServerIP + "\\" + fileServerParentPath+"\\"+ childPath))
                {
                    //更新key状态
                    //加载Piweb数据库数据
                    var characteristicPath = PathHelper.RoundtripString2PathInformation("P:/Demo/");
                    if (args.Count() == 1)
                    {
                        //args 第一位传serial number
                        await updateMeasurements2CheckDemoReport(characteristicPath, args[0]);
                    }
                    MessageBox.Show("上传完成");
                   
                }
                else
                {
                    MessageBox.Show("上传失败"+ "\\\\" + fileServerIP + "\\" + fileServerParentPath + "\\" + childPath);
                }
            }
            else
            {
                MessageBox.Show("无法连接到文件服务器，请检查网络连接");
            }
            Application.Exit();
        }

        /// <summary>  
        /// 是否能 Ping 通指定的主机  
        /// </summary>  
        /// <param name="ip">ip 地址或主机名或域名</param>  
        /// <returns>true 通，false 不通</returns>  
        private bool checkFileServerConnect(string ip)
        {
            try
            {
                System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
                System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions();
                options.DontFragment = true;
                string data = "Test Data!";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 3000; // Timeout 时间，单位：毫秒  
                System.Net.NetworkInformation.PingReply reply = p.Send(ip, timeout, buffer, options);
                if (reply == null || reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    return true;
                return false;
            }
            catch (System.Net.NetworkInformation.PingException e)
            {
                throw new Exception("找不到服务器");
            }
        }
        /// <summary>
        /// 连接远程共享文件夹
        /// </summary>
        /// <param name="path">远程共享文件夹的路径</param>
        /// <param name="userName">用户名</param>
        /// <param name="passWord">密码</param>
        /// <returns></returns>
        public static bool connectState(string path, string userName, string passWord)
        {
            bool Flag = false;
            Process proc = new Process();
            try
            {
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                string dosLine = "";
                if (userName == "None")
                {
                    dosLine = "net use " + path;
                }
                else
                {
                    dosLine = "net use " + path + " /user:" + userName +" "+ passWord;
                }
                proc.StandardInput.WriteLine("net use {0} /delete /y", path);
                proc.StandardInput.WriteLine(dosLine);
                proc.StandardInput.WriteLine("exit");
                //proc.StandardInput.AutoFlush = true;
                while (!proc.HasExited)
                {
                    proc.WaitForExit(1000);
                }
                //string errormsg = proc.StandardError.ReadToEnd();
                //proc.StandardError.Close();

                proc.WaitForExit();
                //if (string.IsNullOrEmpty(errormsg))
                //{
                   Flag = true;
                //}
                //else
                //{
                //    throw new Exception(errormsg);
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }
            return Flag;
        }
        /// <summary>
        /// 向远程文件夹保存本地内容，或者从远程文件夹下载文件到本地
        /// </summary>
        /// <param name="src">要保存的文件的路径，如果保存文件到共享文件夹，这个路径就是本地文件路径如：@"D:\1.avi"</param>
        /// <param name="dst">保存文件的路径，不含名称及扩展名</param>
        /// <param name="fileName">保存文件的名称以及扩展名</param>
        public bool Transport(string src, string dst, string fileName)
        {
            FileStream inFileStream = new FileStream(src, FileMode.Open);

            if (!Directory.Exists(dst))
            {
                Directory.CreateDirectory(dst);
            }
            dst = dst +"\\"+ fileName;
            FileStream outFileStream = new FileStream(dst, FileMode.OpenOrCreate);
            try
            {
                byte[] buf = new byte[inFileStream.Length];

                int byteCount;

                while ((byteCount = inFileStream.Read(buf, 0, buf.Length)) > 0)
                {
                    outFileStream.Write(buf, 0, byteCount);
                }

                inFileStream.Flush();
                outFileStream.Flush();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
            finally
            {
                inFileStream.Close();
                outFileStream.Close();
                
            }

        }

        private bool copyFile2FileServer(string dstPath)
        {
            List<string> demoReportFiles = new List<string>();
            bool transportFlag = true;
            if (textBox1.Text != "")
            {
                demoReportFiles = textBox1.Text.Trim().Split(';').ToList();
                foreach(var file in demoReportFiles)
                {
                    if (file != "")
                    {
                        string fileName = file.Substring(file.LastIndexOf("\\")+1, file.Length - file.LastIndexOf("\\") - 1);
                        try
                        {
                            if (!Directory.Exists(dstPath))
                                Directory.CreateDirectory(dstPath);
                            File.Copy(file, dstPath + "\\" + fileName, true);
                        }
                        catch
                        {
                            transportFlag = false;
                        }  
                    }
                }
            }
            return transportFlag;
        }
        public void UpLoadFile(string fileNamePath, string urlPath)
        {
            string newFileName = fileNamePath.Substring(fileNamePath.LastIndexOf(@"\") + 1);//取文件名称
            MessageBox.Show(newFileName);
            if (urlPath.EndsWith(@"\") == false) urlPath = urlPath + @"\";
            urlPath = urlPath + newFileName;
            WebClient myWebClient = new WebClient();
            NetworkCredential cread = new NetworkCredential();
            myWebClient.Credentials = cread;
            FileStream fs = new FileStream(fileNamePath, FileMode.Open, FileAccess.Read);
            BinaryReader r = new BinaryReader(fs);
            try
            {
                byte[] postArray = r.ReadBytes((int)fs.Length);
                Stream postStream = myWebClient.OpenWrite(urlPath);
                // postStream.m
                if (postStream.CanWrite)
                {
                    postStream.Write(postArray, 0, postArray.Length);
                    MessageBox.Show("文件上传成功！", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("文件上传错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                postStream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误");
            }

        }
    }
}
