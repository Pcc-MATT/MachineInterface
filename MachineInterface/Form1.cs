using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using log4net;
using System.Reflection;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace MachineInterface
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
        #region 调用外部程序，并传参数
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
        #endregion
        ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public string PiWebServer = "http://10.202.120.59:8888/";
        public string calypsoInspectionPlanPath = @"C:\Users\Public\Documents\Zeiss\CALYPSO\workarea\inspections";
        public string easyFormInspectionPlanPath = @"C:\Temp\EasyForm";
        public string marSurfInspectionPlanPath = @"C:\Temp\EasyForm";
        private bool readJsonPara()
        {
            try
            {
                DirectoryInfo path_exe = new DirectoryInfo(Application.StartupPath); //exe目录
                String path = path_exe.Parent.FullName; //上级的目录
                StreamReader sr = new StreamReader(path + @"\relative_files\config.json");
                Dictionary<string, object> valuePairs = new Dictionary<string, object>();
                valuePairs = JsonConvert.DeserializeObject<Dictionary<string, object>>(sr.ReadToEnd());
                sr.Close();
                var contentPut = JsonConvert.SerializeObject(valuePairs, Formatting.Indented);
                log.Info("读取带config.json内容:" + contentPut);
                Dictionary<string, object> data = new Dictionary<string, object>();
                var ss1 = JsonConvert.SerializeObject(valuePairs["data"], Formatting.Indented);
                data = JsonConvert.DeserializeObject<Dictionary<string, object>>(ss1);
                calypsoInspectionPlanPath = data["calypso"].ToString();
                easyFormInspectionPlanPath = data["EasyForm"].ToString();
                marSurfInspectionPlanPath = data["MarSurf"].ToString();

                Dictionary<string, object> Host = new Dictionary<string, object>();
                var ss = JsonConvert.SerializeObject(valuePairs["host"], Formatting.Indented);
                Host = JsonConvert.DeserializeObject<Dictionary<string, object>>(ss);
                PiWebServer = Host["main"].ToString();
                string piwebServerIP = Host["ip"].ToString();
                if (!Ping(piwebServerIP))
                {
                    log.Error("PiWeb服务器 " + piwebServerIP + "连接失败");
                    MessageBox.Show("PiWeb服务器 " + piwebServerIP + "连接失败");
                    return false;
                }
                else
                {
                    log.Info("PiWeb服务器 " + piwebServerIP + "连接成功");
                    return true;
                }
            }
            catch(Exception ex)
            {
                log.Error("读取config.json出错" + ex);
                MessageBox.Show("读取config.json出错" + ex);
                return false;
            }
        }
       
        /// <summary>  
        /// 是否能 Ping 通指定的主机  
        /// </summary>  
        /// <param name="ip">ip 地址或主机名或域名</param>  
        /// <returns>true 通，false 不通</returns>  
        private bool Ping(string ip)
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
        string currentOpenInspectionPlanName;
        //读取job号文件，获取设备名称/job号/待测量总件数
        private Dictionary<string, string> getJobInfo(string jobNumberFile)
        {
            Dictionary<string, string> jobParaList = new Dictionary<string, string>();
            if (File.Exists(jobNumberFile))
            {
                StreamReader sr = new StreamReader(jobNumberFile, Encoding.GetEncoding("gb2312"));
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] lineS = line.Split('=');
                    jobParaList.Add(lineS[0].Trim(), lineS[1].Trim());
                }
                sr.Close();
                return jobParaList;
            }
            else
            {
                return null;
            }

        }
        public string equipmentSN;
        public string equipmentData;
        public string gageEquipment;
        public string currentOperator;
        //查询数据库里对应测量程序的part下，measurement里job号对应的个数
        private int getPartMeasurementCount(string inspectionPlanName,string jobNumber,string taskNumber,string customer,string temperature,string humidity,string productStatus,string productName,string reworkNo,string reworkPartID,string runState)
        {
            string partUuid = "";
            bool equipmentSNBool=false;
            //获取inspectionPlan所在位置
            using (var httpClient = new HttpClient())
            {
                var requestUrl = string.Format("{0}//dataServiceRest/parts?partPath=/Programs/{1}&depth=1&requestedPartAttributes=1002", PiWebServer,productName);
                var response = httpClient.GetAsync(requestUrl).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var obj = JsonConvert.DeserializeObject(content);
                    content = JsonConvert.SerializeObject(obj, Formatting.Indented);
                    PartDetails = content;
                    List<Dictionary<string, object>> parts1 = PartDetailList;
                    foreach (var item in parts1)
                    {
                        foreach (var key in item.Keys)
                        {
                            if (key == "attributes")
                            {
                                var subItem =item[key];
                                //string ss= subItem.ToString();
                                var ss = JsonConvert.SerializeObject(subItem, Formatting.Indented);
                                Dictionary<string, string> tt= JsonConvert.DeserializeObject <Dictionary<string, string>> (ss);
                                foreach(var t in tt)
                                {
                                    if (t.Value == inspectionPlanName)
                                    {
                                        partUuid = item["uuid"].ToString();
                                        break;
                                    }
                                }
                            }
                        }
                    }

                }

            }
            //获取measurement数量
            List<string> partIDList = new List<string>();
            List<partRework> partReworks = new List<partRework>();
            Dictionary<string, object> attributesList = new Dictionary<string, object>();
            List<Dictionary<string, object>> parts = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> valuePairs = new List<Dictionary<string, object>>();
            string machineUuid = "";
            bool wrongPartid = false;
            bool firstWrongPartID = false;
            List<string> wrongPartIdList = new List<string>();
            using (var httpClient = new HttpClient())
            {
                if (partUuid != "")
                {
                    var requestUrl = string.Format("{0}//dataServiceRest/measurements?partUuids={1}&searchCondition=22250In[{2}]%2B22253In[{3}]", PiWebServer, partUuid, jobNumber, taskNumber);
                    var response = httpClient.GetAsync(requestUrl).Result;
                    var content = response.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var obj = JsonConvert.DeserializeObject(content);
                        content = JsonConvert.SerializeObject(obj, Formatting.Indented);
                        PartDetails = content;
                        parts = PartDetailList;
                        int temp = 0;
                        string rework = "";
                        string partid = "";
                        string wrPartid = "";
                        foreach (var item in parts)
                        {
                            foreach (var key in item.Keys)
                            {

                                rework = "";
                                partid = "";
                                if (key == "attributes")
                                {
                                    var subItem = item[key];
                                    //string ss= subItem.ToString();
                                    var ss = JsonConvert.SerializeObject(subItem, Formatting.Indented);
                                    attributesList = JsonConvert.DeserializeObject<Dictionary<string, object>>(ss);
                                    Dictionary<string, string> tt = JsonConvert.DeserializeObject<Dictionary<string, string>>(ss);
                                    foreach (var t in tt)
                                    {
                                        if (t.Key == "12" && !equipmentSNBool)
                                        {
                                            equipmentData = t.Value;
                                            equipmentSNBool = true;
                                        }
                                        if (t.Key == "8"&&temp==0)
                                        {
                                            temp++;
                                            currentOperator = t.Value;
                                        }
                                        //
                                        if (reworkNo != "")
                                        {
                                            if (t.Key == "9")
                                            {
                                                rework = t.Value;
                                            }
                                        }
                                        else
                                        {
                                            rework = "";
                                            //if (t.Key == "14")//partID
                                            //{
                                            //    if (partIDList.FindAll(n => n == t.Value).Count() == 0)
                                            //    {
                                            //        partIDList.Add(t.Value);
                                            //    }
                                            //    break;
                                            //}
                                        }
                                        if (t.Key == "14")//partID
                                        {
                                            partid = t.Value;
                                            log.Info("partid=" + partid);
                                        }
                                    }
                                    if (rework == reworkNo)
                                    {
                                        log.Info("rework=" + rework);
                                        if (partIDList.FindAll(n => n == partid).Count() == 0)
                                        {
                                            log.Info("reworkPartID=" + reworkPartID);
                                            if (!reworkPartID.Contains(partid)&&rework!="")
                                            {
                                                wrongPartid = true;
                                                wrongPartIdList.Add(partid);
                                                //if (!firstWrongPartID)
                                                //{
                                                //    firstWrongPartID = true;
                                                //    wrPartid = partid;
                                                //}
                                            }
                                            else
                                            {
                                                partIDList.Add(partid);
                                            }
                                            
                                        }
                                    }
                                }
                                if (key == "uuid")
                                {
                                    var subItem = item[key];
                                    machineUuid = subItem.ToString();
                                }
                                if (key == "partUuid")
                                {
                                    var subItem = item[key];
                                    partUuid = subItem.ToString();
                                }
                            }
                            Dictionary<string, object> chrDic = new Dictionary<string, object>();
                            chrDic.Add("uuid", machineUuid);
                            chrDic.Add("partUuid", partUuid);
                            if (attributesList.ContainsKey("96")) //Approval
                            {
                                if (attributesList["96"].ToString() == "0" || attributesList["96"].ToString() == "")
                                {
                                    attributesList["96"] = 1;
                                }
                            }
                            else
                            {
                                attributesList.Add("96", 1);
                            }

                            if (attributesList.ContainsKey("22259")) //Rework enable
                            {
                                if (attributesList["22259"].ToString() == "0" || attributesList["22259"].ToString() == "")
                                {
                                    attributesList["22259"] = 0;
                                }
                            }
                            else
                            {
                                attributesList.Add("22259", 0);
                            }

                            if (attributesList.ContainsKey("22221")) //Temperature
                            {
                                if (attributesList["22221"].ToString() == "0" || attributesList["22221"].ToString() == "")
                                {
                                    attributesList["22221"] = temperature;
                                }
                            }
                            else
                            {
                                attributesList.Add("22221", temperature);
                            }
                            if (attributesList.ContainsKey("22222")) //humidity
                            {
                                if (attributesList["22222"].ToString() == "0" || attributesList["22222"].ToString() == "")
                                {
                                    attributesList["22222"] = humidity;
                                }
                            }
                            else
                            {
                                attributesList.Add("22222", humidity);
                            }
                            if (attributesList.ContainsKey("22033")) //Product status
                            {
                                if (attributesList["22033"].ToString() == "0" || attributesList["22033"].ToString() == "")
                                {
                                    attributesList["22033"] = productStatus;

                                }
                            }
                            else
                            {
                                attributesList.Add("22033", productStatus);
                            }
                            if (attributesList.ContainsKey("22031")) //Product Name
                            {
                                if (attributesList["22031"].ToString() == "0" || attributesList["22031"].ToString() == "")
                                {
                                    attributesList["22031"] = productName;

                                }
                            }
                            else
                            {
                                attributesList.Add("22031", productName);
                            }
                            if (attributesList.ContainsKey("22200")) //Inspection Name
                            {
                                if (attributesList["22200"].ToString() == "0" || attributesList["22200"].ToString() == "")
                                {
                                    attributesList["22200"] = inspectionPlanName;

                                }
                            }
                            else
                            {
                                attributesList.Add("22200", inspectionPlanName);
                            }
                            if (attributesList.ContainsKey("22001")) //Equipment type
                            {
                                if (attributesList["22001"].ToString() == "0" || attributesList["22001"].ToString() == "")
                                {
                                    attributesList["22001"] = "三坐标测量机";

                                }
                            }
                            else
                            {
                                attributesList.Add("22001", "三坐标测量机");
                            }
                            chrDic.Add("attributes", attributesList);
                            valuePairs.Add(chrDic);
                        }
                        if (wrongPartid&&runState=="Stop")
                        {
                            string pt = "";
                            foreach(var wr in wrongPartIdList)
                            {
                                pt += wr+",";
                            }
                            log.Info("当前测量的part ID:" + pt + "不在需要复测的列表里");
                            MessageBox.Show("当前测量的part ID:" + pt + "不在需要复测的列表里");
                        }
                    }
                }
                else
                {
                    
                }
            }
            //修改approval状态
            var contentPut = JsonConvert.SerializeObject(valuePairs, Formatting.Indented);
            log.Info("更新Measurement状态语句:" + contentPut);
            using (var httpClient1 = new HttpClient())
            {
                var requestUrl1 = string.Format("{0}/dataServiceRest/measurements?partPath=/Programs/{1}/{2}/", PiWebServer,productName,inspectionPlanName);
                try
                {
                    var response1 = httpClient1.PutAsync(requestUrl1, new StringContent(contentPut)).Result;
                    if (response1.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        log.Error("更新" + inspectionPlanName + "测量程序的Measurement部分内容出错");
                        //MessageBox.Show("更新approval出错");
                    }
                }catch(Exception ex)
                {
                    log.Error("更新" + inspectionPlanName + "测量程序的Measurement部分内容出错,异常内容："+ex.ToString());
                }
               

            }          
            return partIDList.Count();
        }
        public string PartDetails { get; set; }
        public List<Dictionary<string, object>> PartDetailList
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PartDetails))
                {
                    return null;
                }
                try
                {
                    var obj = JToken.Parse(PartDetails);
                }
                catch (Exception)
                {
                    throw new FormatException("ProductDetails不符合json格式.");
                }
                return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(PartDetails);
            }
        }
        //put 数据库中对应的machine里的key的值
        private void putPartIndex2Machine(string machineName,string jobNumber,int totalCount,int alreadyMeasureCount,string machineState,string taskNumber,string productStatus,string productName,string user,string inspectionName,string inspectionType)
        {
            string machineUuid = "";
            string partUuid = "";
            //查询设备所在machine的path和uuid
            Dictionary<string, object> attributesList = new Dictionary<string, object>();
            using (var httpClient = new HttpClient())
            {
                var requestUrl = string.Format("{0}/dataServiceRest/measurements?partPath=/Process/Equipment/" + machineName + "&searchCondition=8In[Machine]", PiWebServer);//&requestedCharacteristicAttributes=22007
                var response = httpClient.GetAsync(requestUrl).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var obj = JsonConvert.DeserializeObject(content);
                    content = JsonConvert.SerializeObject(obj, Formatting.Indented);
                    PartDetails = content;
                    List<Dictionary<string, object>> parts = PartDetailList;
                    if (parts.Count() == 0)
                    {
                        log.Warn("未在Equipment里查询到指定的设备 " + machineName);
                        MessageBox.Show("未在Equipment里查询到指定的设备 " + machineName);
                    }
                    else
                    {
                        foreach (var item in parts)
                        {
                            foreach (var key in item.Keys)
                            {
                                if (key == "uuid")
                                {
                                    var subItem = item[key];
                                    machineUuid = subItem.ToString();
                                }
                                if (key == "partUuid")
                                {
                                    var subItem = item[key];
                                    partUuid = subItem.ToString();
                                }
                                if (key == "attributes")
                                {
                                    var subItem = item[key];
                                    var ss = JsonConvert.SerializeObject(subItem, Formatting.Indented);
                                    attributesList = JsonConvert.DeserializeObject<Dictionary<string, object>>(ss);
                                }
                            }
                        }

                        //修改machine的key
                        Dictionary<string, object> chrDic = new Dictionary<string, object>();

                        chrDic.Add("uuid", machineUuid);
                        chrDic.Add("partUuid", partUuid);
                        if (attributesList.ContainsKey("22250")) //Job number
                        {
                            attributesList["22250"] = jobNumber;
                        }
                        else
                        {
                            attributesList.Add("22250", jobNumber);
                        }
                        if (attributesList.ContainsKey("22035")) //Product quantity
                        {
                            attributesList["22035"] = totalCount;
                        }
                        else
                        {
                            attributesList.Add("22035", totalCount);
                        }
                        if (attributesList.ContainsKey("22253")) //task Number
                        {
                            attributesList["22253"] = taskNumber;
                        }
                        else
                        {
                            attributesList.Add("22253", taskNumber);
                        }
                        if (machineState == "Warning")
                        {
                            if (attributesList.ContainsKey("22254")) //Product quantity
                            {
                                attributesList["22254"] = attributesList["22254"];
                            }
                            else
                            {
                                attributesList.Add("22254", attributesList["22254"]);
                            }
                        }
                        else
                        {
                            if (attributesList.ContainsKey("22254")) //Product quantity
                            {
                                attributesList["22254"] = alreadyMeasureCount;
                            }
                            else
                            {
                                attributesList.Add("22254", alreadyMeasureCount);
                            }
                        }

                        if (attributesList.ContainsKey("22036")) //machineState
                        {
                            attributesList["22036"] = machineState;
                        }
                        else
                        {
                            attributesList.Add("22036", machineState);
                        }
                        if (attributesList.ContainsKey("22033")) //product status
                        {
                            attributesList["22033"] = productStatus;
                        }
                        else
                        {
                            attributesList.Add("22033", productStatus);
                        }
                        if (attributesList.ContainsKey("22031")) //product name
                        {
                            attributesList["22031"] = productName;
                        }
                        else
                        {
                            attributesList.Add("22031", productName);
                        }
                        if (attributesList.ContainsKey("22200")) //inspection name
                        {
                            attributesList["22200"] = inspectionName;
                        }
                        else
                        {
                            attributesList.Add("22200", inspectionName);
                        }
                        if (attributesList.ContainsKey("22258")) //inspection type
                        {
                            attributesList["22258"] = inspectionType;
                        }
                        else
                        {
                            attributesList.Add("22258", inspectionType);
                        }
                        if (attributesList.ContainsKey("801")) //operator
                        {
                            attributesList["801"] = user;
                        }
                        else
                        {
                            attributesList.Add("801", user);
                        }
                        chrDic.Add("attributes", attributesList);

                        //chrDic.Add("")
                        List<Dictionary<string, object>> valuePairs = new List<Dictionary<string, object>>();
                        valuePairs.Add(chrDic);
                        var contentPut = JsonConvert.SerializeObject(valuePairs, Formatting.Indented);
                        using (var httpClient1 = new HttpClient())
                        {
                            var requestUrl1 = string.Format("{0}/dataServiceRest/measurements?partPath=/Process/Equipment/" + machineName + "/machine/", PiWebServer);
                            var response1 = httpClient1.PutAsync(requestUrl1, new StringContent(contentPut)).Result;
                        }
                    }
                   
                }
                else
                {

                }
            }

        }
        private void updateStatus2Machine(string machineName,string machineState)
        {
            string machineUuid = "";
            string partUuid = "";
            //查询设备所在machine的path和uuid
            Dictionary<string, object> attributesList = new Dictionary<string, object>();
            using (var httpClient = new HttpClient())
            {
                var requestUrl = string.Format("{0}/dataServiceRest/measurements?partPath=/Process/Equipment/" + machineName + "&searchCondition=8In[Machine]", PiWebServer);//&requestedCharacteristicAttributes=22007
                var response = httpClient.GetAsync(requestUrl).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var obj = JsonConvert.DeserializeObject(content);
                    content = JsonConvert.SerializeObject(obj, Formatting.Indented);
                    PartDetails = content;
                    List<Dictionary<string, object>> parts = PartDetailList;
                    if (parts.Count() == 0)
                    {
                        log.Warn("未在Equipment里查询到指定的设备 " + machineName);
                        MessageBox.Show("未在Equipment里查询到指定的设备 " + machineName);
                    }
                    else
                    {
                        foreach (var item in parts)
                        {
                            foreach (var key in item.Keys)
                            {
                                if (key == "uuid")
                                {
                                    var subItem = item[key];
                                    machineUuid = subItem.ToString();
                                }
                                if (key == "partUuid")
                                {
                                    var subItem = item[key];
                                    partUuid = subItem.ToString();
                                }
                                if (key == "attributes")
                                {
                                    var subItem = item[key];
                                    var ss = JsonConvert.SerializeObject(subItem, Formatting.Indented);
                                    attributesList = JsonConvert.DeserializeObject<Dictionary<string, object>>(ss);
                                }
                            }
                        }

                        //修改machine的key
                        Dictionary<string, object> chrDic = new Dictionary<string, object>();

                        chrDic.Add("uuid", machineUuid);
                        chrDic.Add("partUuid", partUuid);
                        
                 

                        if (attributesList.ContainsKey("22036")) //machineState
                        {
                            attributesList["22036"] = machineState;
                        }
                        else
                        {
                            attributesList.Add("22036", machineState);
                        }
                       
                        chrDic.Add("attributes", attributesList);

                        //chrDic.Add("")
                        List<Dictionary<string, object>> valuePairs = new List<Dictionary<string, object>>();
                        valuePairs.Add(chrDic);
                        var contentPut = JsonConvert.SerializeObject(valuePairs, Formatting.Indented);
                        using (var httpClient1 = new HttpClient())
                        {
                            var requestUrl1 = string.Format("{0}/dataServiceRest/measurements?partPath=/Process/Equipment/" + machineName + "/machine/", PiWebServer);
                            var response1 = httpClient1.PutAsync(requestUrl1, new StringContent(contentPut)).Result;
                        }
                    }

                }
                else
                {

                }
            }

        }
        //
        Dictionary<string, object> jobAttributesList;
        private bool checkJobInfo(string jobNumber, string taskNumber,string reworkLabel)
        {
            bool jobInfoState = true ;
            Dictionary<string, object> attributesList = new Dictionary<string, object>();
            using (var httpClient = new HttpClient())
            {
                var requestUrl="";
                if (reworkLabel != "")
                {
                    requestUrl = string.Format("{0}/dataServiceRest/measurements?partPath=/Process/Job_Management&searchCondition=22250In[{1}]%2B22253In[{2}]%2B9In[{3}]", PiWebServer, jobNumber, taskNumber, reworkLabel);//&requestedCharacteristicAttributes=22007
                }
                else
                {
                    requestUrl = string.Format("{0}/dataServiceRest/measurements?partPath=/Process/Job_Management&searchCondition=22250In[{1}]%2B22253In[{2}]", PiWebServer, jobNumber, taskNumber);//&requestedCharacteristicAttributes=22007

                }

                var response = httpClient.GetAsync(requestUrl).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var obj = JsonConvert.DeserializeObject(content);
                    content = JsonConvert.SerializeObject(obj, Formatting.Indented);
                    PartDetails = content;
                    List<Dictionary<string, object>> parts = PartDetailList;
                    if (parts.Count() != 0)
                    {
                        log.Info("查询到JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " &Rework=" + reworkLabel+ " 的任务信息");
                        jobInfoState= true;

                        foreach (var item in parts)
                        {
                            foreach (var key in item.Keys)
                            {
                                if (key == "attributes")
                                {
                                    var subItem = item[key];
                                    var ss = JsonConvert.SerializeObject(subItem, Formatting.Indented);
                                    attributesList = JsonConvert.DeserializeObject<Dictionary<string, object>>(ss);
                                }
                            }
                        }
                        if (attributesList["22251"].ToString() =="5")
                        {
                            log.Warn("查询到JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " 的任务信息已经处于完成状态");
                            MessageBox.Show("查询到JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " 的任务信息已经处于完成状态");
                        }
                        else
                        {
                            jobAttributesList = attributesList;
                        }
                    }
                    else
                    {
                        
                        //未查询到该任务信息
                        jobInfoState = false;
                       
                        
                    }

                }
            }
            return jobInfoState;
        }
        private bool updateJobInfo(string jobNumber, string taskNumber, string reworkLabel,int JobStatus)
        {
            string machineUuid = "";
            string partUuid = "";
            bool jobInfoState = true;
            Dictionary<string, object> attributesList = new Dictionary<string, object>();
            using (var httpClient = new HttpClient())
            {
                var requestUrl = "";
                if (reworkLabel != "")
                {
                    requestUrl = string.Format("{0}/dataServiceRest/measurements?partPath=/Process/Job_Management&searchCondition=22250In[{1}]%2B22253In[{2}]%2B9In[{3}]", PiWebServer, jobNumber, taskNumber, reworkLabel);//&requestedCharacteristicAttributes=22007
                }
                else
                {
                    requestUrl = string.Format("{0}/dataServiceRest/measurements?partPath=/Process/Job_Management&searchCondition=22250In[{1}]%2B22253In[{2}]", PiWebServer, jobNumber, taskNumber);//&requestedCharacteristicAttributes=22007

                }

                var response = httpClient.GetAsync(requestUrl).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var obj = JsonConvert.DeserializeObject(content);
                    content = JsonConvert.SerializeObject(obj, Formatting.Indented);
                    PartDetails = content;
                    List<Dictionary<string, object>> parts = PartDetailList;
                    if (parts.Count() != 0)
                    {
                        log.Info("查询到JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " &Rework=" + reworkLabel + " 的任务信息");
                        foreach (var item in parts)
                        {
                            foreach (var key in item.Keys)
                            {
                                if (key == "uuid")
                                {
                                    var subItem = item[key];
                                    machineUuid = subItem.ToString();
                                }
                                if (key == "partUuid")
                                {
                                    var subItem = item[key];
                                    partUuid = subItem.ToString();
                                }
                                if (key == "attributes")
                                {
                                    var subItem = item[key];
                                    var ss = JsonConvert.SerializeObject(subItem, Formatting.Indented);
                                    attributesList = JsonConvert.DeserializeObject<Dictionary<string, object>>(ss);
                                }
                            }
                        }
                        if (attributesList["22251"].ToString() == "5")
                        {
                            log.Warn("查询到JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " 的任务信息已经处于完成状态");
                            MessageBox.Show("查询到JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " 的任务信息已经处于完成状态");
                        }
                        else
                        {
                            jobAttributesList = attributesList;
                        }
                    }
                    else
                    {
                        //未查询到该任务信息
                        jobInfoState = false;
                    }

                }
            }
            Dictionary<string, object> chrDic = new Dictionary<string, object>();
            chrDic.Add("uuid", machineUuid);
            chrDic.Add("partUuid", partUuid);
            if (attributesList.ContainsKey("22251")) //JobStatus
            {
                attributesList["22251"] = JobStatus;
            }
            else
            {
                attributesList.Add("22251", JobStatus);
            } 
            chrDic.Add("attributes", attributesList);

            List<Dictionary<string, object>> valuePairs = new List<Dictionary<string, object>>();
            valuePairs.Add(chrDic);
            var contentPut = JsonConvert.SerializeObject(valuePairs, Formatting.Indented);
            log.Info("updateJobinfo:" + contentPut);
            using (var httpClient = new HttpClient())
            {
                var requestUrl = string.Format("{0}/dataServiceRest/measurements?partPath=/Process/Job_Management/", PiWebServer);
                var response = httpClient.PutAsync(requestUrl, new StringContent(contentPut)).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    log.Error("更新JobNumber:"+jobNumber+",TaksNumber:"+taskNumber+"信息的jobstatus："+JobStatus+"出错");
                }
                else
                {
                    jobInfoState = true;
                    log.Info("更新JobNumber:" + jobNumber + ",TaksNumber:" + taskNumber + "信息的jobstatus：" + JobStatus + "成功");
                }
            }
            return jobInfoState;
        }
        //添加一次测量
        private void creatNewMeasurement()
        {
            string productName = jobAttributesList["22031"].ToString();
            string equipmentType = jobAttributesList["22001"].ToString();
            string partName="";
            if(equipmentType!= "三坐标测量机")
            {
                partName = jobAttributesList["22000"].ToString();
            }
            else
            {
                partName = jobAttributesList["22200"].ToString();
            }
            string partUuid="";
            //获取inspectionPlan所在位置
            using (var httpClient = new HttpClient())
            {
                var requestUrl = string.Format("{0}//dataServiceRest/parts?partPath=/Programs/{1}&depth=1&requestedPartAttributes=1002", PiWebServer, productName);
                var response = httpClient.GetAsync(requestUrl).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var obj = JsonConvert.DeserializeObject(content);
                    content = JsonConvert.SerializeObject(obj, Formatting.Indented);
                    PartDetails = content;
                    List<Dictionary<string, object>> parts1 = PartDetailList;
                    foreach (var item in parts1)
                    {
                        foreach (var key in item.Keys)
                        {
                            if (key == "attributes")
                            {
                                var subItem = item[key];
                                var ss = JsonConvert.SerializeObject(subItem, Formatting.Indented);
                                Dictionary<string, string> tt = JsonConvert.DeserializeObject<Dictionary<string, string>>(ss);
                                foreach (var t in tt)
                                {
                                    if (t.Value == partName)
                                    {
                                        partUuid = item["uuid"].ToString();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            List<Dictionary<string, object>> valuePairs = new List<Dictionary<string, object>>();
            string machineUuid = "";
            Dictionary<string, object> chrDic = new Dictionary<string, object>();
            machineUuid= System.Guid.NewGuid().ToString();
            chrDic.Add("uuid", machineUuid);
            chrDic.Add("partUuid", partUuid);
            if (jobAttributesList.ContainsKey("96")) //Approval
            {
                if (jobAttributesList["96"].ToString() == "0" || jobAttributesList["96"].ToString() == "")
                {
                    jobAttributesList["96"] = 1;
                }
            }
            else
            {
                jobAttributesList.Add("96", 1);
            }
            if (jobAttributesList.ContainsKey("4")) //time 
            {
                jobAttributesList["4"] = DateTime.UtcNow;
            }
            else
            {
                jobAttributesList.Add("4", DateTime.UtcNow);
            }

            if (jobAttributesList.ContainsKey("22259")) //Rework enable
            {
                if (jobAttributesList["22259"].ToString() == "0" || jobAttributesList["22259"].ToString() == "")
                {
                    jobAttributesList["22259"] = 0;
                }
            }
            else
            {
                jobAttributesList.Add("22259", 0);
            }
            if (jobAttributesList.ContainsKey("22200")) //inspection Name
            {
                if (jobAttributesList["22200"].ToString() == "0" || jobAttributesList["22200"].ToString() == "")
                {
                    jobAttributesList["22200"] = partName;
                }
            }
            else
            {
                jobAttributesList.Add("22200", partName);
            }
            chrDic.Add("attributes", jobAttributesList);
            valuePairs.Add(chrDic);
            var contentPut = JsonConvert.SerializeObject(valuePairs, Formatting.Indented);
            log.Info("添加" + partName + "测量程序的Measurement内容:" + contentPut);
            using (var httpClient1 = new HttpClient())
            {
                var requestUrl1 = string.Format("{0}/dataServiceRest/measurements?partPath=/Programs/{1}/{2}/", PiWebServer, productName, partName);
                var response1 = httpClient1.PostAsync(requestUrl1, new StringContent(contentPut)).Result;
                if (response1.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    log.Error("添加" + partName + "测量程序的Measurement内容出错");
                    //MessageBox.Show("更新approval出错");
                }
            }
        }
        //删除一次测量
        int manualMeasurementCount = 0;
        private void deleteMeasurement(string jobNumber, string taskNumber, string reworkLabel)
        {
            string productName = jobAttributesList["22031"].ToString();
            string equipmentType = jobAttributesList["22001"].ToString();
            string partName = "";
            if (equipmentType != "三坐标测量机")
            {
                partName = jobAttributesList["22000"].ToString();
            }
            else
            {
                partName = jobAttributesList["22200"].ToString();
            }
            string partUuid = "";
            //获取inspectionPlan所在位置
            using (var httpClient = new HttpClient())
            {
                var requestUrl = string.Format("{0}//dataServiceRest/parts?partPath=/Programs/{1}&depth=1&requestedPartAttributes=1002", PiWebServer, productName);
                var response = httpClient.GetAsync(requestUrl).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var obj = JsonConvert.DeserializeObject(content);
                    content = JsonConvert.SerializeObject(obj, Formatting.Indented);
                    PartDetails = content;
                    List<Dictionary<string, object>> parts1 = PartDetailList;
                    foreach (var item in parts1)
                    {
                        foreach (var key in item.Keys)
                        {
                            if (key == "attributes")
                            {
                                var subItem = item[key];
                                var ss = JsonConvert.SerializeObject(subItem, Formatting.Indented);
                                Dictionary<string, string> tt = JsonConvert.DeserializeObject<Dictionary<string, string>>(ss);
                                foreach (var t in tt)
                                {
                                    if (t.Value == partName)
                                    {
                                        partUuid = item["uuid"].ToString();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            List<Dictionary<string, object>> parts = new List<Dictionary<string, object>>();
            Dictionary<string, object> attributesList = new Dictionary<string, object>();
            List<string> delMeasurementUuidList = new List<string>();
            bool notWritePartID = false;
            using (var httpClient = new HttpClient())
            {
                if (partUuid != "")
                {
                    var requestUrl = "";
                    if (reworkLabel == "")
                    {
                        requestUrl = string.Format("{0}//dataServiceRest/measurements?partUuids={1}&searchCondition=22250In[{2}]%2B22253In[{3}]", PiWebServer, partUuid, jobNumber, taskNumber);
                    }
                    else
                    {
                        requestUrl = string.Format("{0}//dataServiceRest/measurements?partUuids={1}&searchCondition=22250In[{2}]%2B22253In[{3}]%2B9In[{4}]", PiWebServer, partUuid, jobNumber, taskNumber,reworkLabel);
                    }
                    var response = httpClient.GetAsync(requestUrl).Result;
                    var content = response.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var obj = JsonConvert.DeserializeObject(content);
                        content = JsonConvert.SerializeObject(obj, Formatting.Indented);
                        PartDetails = content;
                        parts = PartDetailList;
                        int temp = 0;
                        string rework = "";
                        string partid = "";
                        string wrPartid = "";
                        string measurementUuid = "";
                        bool delFlag = false ;
                        foreach (var item in parts)
                        {
                            manualMeasurementCount++;
                            foreach (var key in item.Keys)
                            {
                                rework = "";
                                partid = "";
                                if (key == "attributes")
                                {
                                    var subItem = item[key];
                                    //string ss= subItem.ToString();
                                    var ss = JsonConvert.SerializeObject(subItem, Formatting.Indented);
                                    attributesList = JsonConvert.DeserializeObject<Dictionary<string, object>>(ss);
                                    Dictionary<string, string> tt = JsonConvert.DeserializeObject<Dictionary<string, string>>(ss);
                                    foreach (var t in tt)
                                    {
                                        if (t.Key == "22202")
                                        {
                                            if (t.Value == "1")
                                            {
                                                delFlag = false;
                                                break;
                                            }
                                            else { delFlag = true; }
                                        }
                                        else
                                        {
                                            delFlag = true;
                                        }
                                    }
                                }
                                if (key == "uuid")
                                {
                                    var subItem = item[key];
                                    measurementUuid = subItem.ToString();
                                }  
                            }
                            if (delFlag)
                            {
                                delFlag = false;
                                delMeasurementUuidList.Add(measurementUuid);
                            }
                            if (attributesList.ContainsKey("14")) //partID
                            {
                                
                            }
                            else
                            {
                                notWritePartID = true;
                            }
                        }
                    }
                }
                else
                {

                }
            }
            manualMeasurementCount = manualMeasurementCount - delMeasurementUuidList.Count();
            foreach (var uuid in delMeasurementUuidList)
            {
                using (var httpClient1 = new HttpClient())
                {
                    var requestUrl1 = string.Format("{0}/dataServiceRest/measurements/{1}", PiWebServer, uuid);
                    var response1 = httpClient1.DeleteAsync(requestUrl1).Result;
                    if (response1.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        log.Error("删除" + partName + "测量程序的Measurement内容出错");
                    }
                }
            }
            if (notWritePartID)
            {
                log.Warn("存在没有输入PartID的测量，请再次确认并输入正确的PartID!");
                MessageBox.Show("存在没有输入PartID的测量，请再次确认并输入正确的PartID!");
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //Dictionary<string, string> jobPara = getJobInfo();
            //if (jobPara != null)
            //{
            //    int alreadyMeasureIndex = getPartMeasurementCount(currentOpenInspectionPlanName, jobPara["22250"], jobPara["22253"], jobPara["22037"]);
            //    if(jobPara["12"].Contains(' '))
            //    {
            //        jobPara["12"] = jobPara["12"].Split(' ')[0]+"%20"+ jobPara["12"].Split(' ')[1];
            //    }
            //    putPartIndex2Machine(jobPara["12"], jobPara["22250"],int.Parse(jobPara["22035"]), alreadyMeasureIndex, "Start", jobPara["22253"]);
            //}
           
        }
        //获取当前测量程序名称
        private void getCurrentOpenInsepctionName()
        {
            foreach (Process p in Process.GetProcessesByName("VWNT"))
            {
                try
                {
                    currentOpenInspectionPlanName = p.MainWindowTitle.Substring(p.MainWindowTitle.IndexOf("-") + 1, p.MainWindowTitle.Count() - 1 - p.MainWindowTitle.IndexOf("-")).Trim();
                }
                catch
                {
                }
            }
        }
        //更新Job信息
        public void updateJobInfo(string jobNumber,string taskNumber,string equipmentSN,string gageEquipment,string JobStatus,bool updateGageSN,int currentPartCount,string inspectionPlanName)
        {
            string machineUuid = "";
            string partUuid = "";
            //
            Dictionary<string, object> attributesList = new Dictionary<string, object>();
            using (var httpClient = new HttpClient())
            {
                var requestUrl = string.Format("{0}/dataServiceRest/measurements?partPath=/Process/Job_Management&searchCondition=22250In[{1}]%2B22253In[{2}]", PiWebServer, jobNumber, taskNumber);//&requestedCharacteristicAttributes=22007

                var response = httpClient.GetAsync(requestUrl).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var obj = JsonConvert.DeserializeObject(content);
                    content = JsonConvert.SerializeObject(obj, Formatting.Indented);
                    PartDetails = content;
                    List<Dictionary<string, object>> parts = PartDetailList;
                    if (parts.Count() != 0)
                    {
                        foreach (var item in parts)
                        {
                            foreach (var key in item.Keys)
                            {
                                if (key == "uuid")
                                {
                                    var subItem = item[key];
                                    machineUuid = subItem.ToString();
                                }
                                if (key == "partUuid")
                                {
                                    var subItem = item[key];
                                    partUuid = subItem.ToString();
                                }
                                if (key == "attributes")
                                {
                                    var subItem = item[key];
                                    var ss = JsonConvert.SerializeObject(subItem, Formatting.Indented);
                                    attributesList = JsonConvert.DeserializeObject<Dictionary<string, object>>(ss);
                                    if (!attributesList.ContainsKey("22200")) //JobStatus
                                    {
                                        log.Error("当前JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " 的任务信息 没有对应的测量程序名称");
                                        //未查询到该任务信息
                                        MessageBox.Show("当前JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " 的任务信息 没有对应的测量程序名称");
                                    }
                                    else
                                    {
                                        if (attributesList["22200"].ToString() != inspectionPlanName)
                                        {
                                            log.Error("当前JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " 的任务信息对应的测量程序名称 " + attributesList["22200"] + " 与实际运行的测量程序 " + inspectionPlanName + " 不符");
                                            //未查询到该任务信息
                                            MessageBox.Show("当前JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " 的任务信息对应的测量程序名称 " + attributesList["22200"] + " 与实际运行的测量程序 " + inspectionPlanName + " 不符");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        log.Error("未查询到JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " 的任务信息");
                        //未查询到该任务信息
                        MessageBox.Show("未查询到JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " 的任务信息");
                    }

                }
            }

            Dictionary<string, object> chrDic = new Dictionary<string, object>();
            chrDic.Add("uuid", machineUuid);
            chrDic.Add("partUuid", partUuid);
            if (updateGageSN)
            {
                if (attributesList.ContainsKey("22000")) //equipmentSN
                {
                    attributesList["22000"] = equipmentSN;
                }
                else
                {
                    attributesList.Add("22000", equipmentSN);
                }
                if (attributesList.ContainsKey("12")) //gageEquipment
                {
                    attributesList["12"] = gageEquipment;
                }
                else
                {
                    attributesList.Add("12", gageEquipment);
                }
            }
            if (attributesList.ContainsKey("22251")) //JobStatus
            {
                attributesList["22251"] = JobStatus;
            }
            else
            {
                attributesList.Add("22251", JobStatus);
            }
            if (attributesList.ContainsKey("22254")) //JobStatus
            {
                attributesList["22254"] = currentPartCount;
            }
            else
            {
                attributesList.Add("22254", currentPartCount);
            }
            if (attributesList.ContainsKey("22256")) //JobStartTime
            {
                if (attributesList["22256"].ToString() == "")
                {
                    attributesList["22256"] = DateTime.UtcNow;//DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)
                }
            }
            else
            {
                attributesList.Add("22256", DateTime.UtcNow);
            }
            chrDic.Add("attributes", attributesList);

            List<Dictionary<string, object>> valuePairs = new List<Dictionary<string, object>>();
            valuePairs.Add(chrDic);
            var contentPut = JsonConvert.SerializeObject(valuePairs, Formatting.Indented);
            using (var httpClient = new HttpClient())
            {
                var requestUrl = string.Format("{0}/dataServiceRest/measurements?partPath=/Process/Job_Management/", PiWebServer);
                var response = httpClient.PutAsync(requestUrl, new StringContent(contentPut)).Result;
            }
        }
        Dictionary<string, string> jobPara;
        int alreadyMeasureIndex;
        public string piwebInterface;
        public string ptxPath;
        string[] piwebInterfaceParas;
        string openPtxStr;
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                if (readJsonPara())
                {
                    if (args.Count() >= 2)
                    {
                        string jobnumber = "";
                        string tasknumber = "";
                        string reworkLabel = "";
                        string productIdent = "";
                        string software = "";
                        string funLabel = "";
                        string inspectionPath = "";
                        string inspectionName = "";
                        string gageEquipment = "";
                        string machineStatus = "";
                        funLabel = args[0];
                        if (funLabel == "CreatMeasurement")
                        {
                            if (args.Count() == 3)
                            {
                                jobnumber = args[1];
                                tasknumber = args[2];
                                reworkLabel = "";
                            }
                            else if (args.Count() == 4)
                            {
                                jobnumber = args[1];
                                tasknumber = args[2];
                                reworkLabel = args[3];
                            }
                            jobAttributesList = new Dictionary<string, object>();
                            checkJobInfo(jobnumber, tasknumber, reworkLabel);
                            creatNewMeasurement();
                            log.Info("新增测量完成！");
                            Application.Exit();
                        }
                        else if (funLabel == "DeleteMeasurement")
                        {
                            if (args.Count() == 3)
                            {
                                jobnumber = args[1];
                                tasknumber = args[2];
                                reworkLabel = "";
                            }
                            else if (args.Count() == 4)
                            {
                                jobnumber = args[1];
                                tasknumber = args[2];
                                reworkLabel = args[3];
                            }
                            jobAttributesList = new Dictionary<string, object>();
                            checkJobInfo(jobnumber, tasknumber, reworkLabel);
                            deleteMeasurement(jobnumber, tasknumber, reworkLabel);
                            log.Info("删除测量完成！");
                            log.Info("开始更新Job状态信息");
                            updateJobInfo(jobAttributesList["22250"].ToString(), jobAttributesList["22253"].ToString(), "", "", "3", false, manualMeasurementCount, "");
                            log.Info("更新设备Job信息完成，jobNumber:" + jobAttributesList["22250"].ToString() + " JobStatus:3  updateGageFlag:false");
                            log.Info("开始更新设备状态信息");
                            putPartIndex2Machine(jobAttributesList["12"].ToString(), jobAttributesList["22250"].ToString(), int.Parse(jobAttributesList["22035"].ToString()), manualMeasurementCount, "Avaiable", jobAttributesList["22253"].ToString(), jobAttributesList["22033"].ToString(), jobAttributesList["22031"].ToString(), jobAttributesList["8"].ToString(), "", jobAttributesList["22258"].ToString());
                            log.Info("设备状态信息更新完成,machineName:" + jobAttributesList["12"].ToString() + " JobNumber:" + jobAttributesList["22250"].ToString() + "  machineState:" + "Avaibale" + " /ProductStatus" + jobAttributesList["22033"].ToString() + " /ProductName" + jobAttributesList["22031"].ToString() + " /currentOperator" + jobAttributesList["8"].ToString() + " /currentInspectionName:" + "" + "/inspectionType:" + jobAttributesList["22258"].ToString());
                            Application.Exit();
                        }
                        else if (funLabel == "SwitchTaskPage")
                        {
                            if (args.Count() == 5)
                            {
                                jobnumber = args[1];
                                tasknumber = args[2];
                                productIdent = args[3];
                                software = args[4];
                                reworkLabel = "";
                            }
                            else if (args.Count() == 6)
                            {
                                jobnumber = args[1];
                                tasknumber = args[2];
                                productIdent = args[3];
                                software = args[4];
                                reworkLabel = args[5];
                            }
                            jobAttributesList = new Dictionary<string, object>();
                            checkJobInfo(jobnumber, tasknumber, reworkLabel);//获取job信息
                            //updateJobInfo(jobnumber, tasknumber, reworkLabel, "3");
                            DirectoryInfo path_exe = new DirectoryInfo(Application.StartupPath); //exe目录
                            String path = path_exe.Parent.FullName; //上级的目录
                            String parentPath = path_exe.Parent.Parent.FullName;
                            piwebInterface = path_exe + @"\PiWebCommandLineInterface.exe";
                            switch (jobAttributesList["10030"].ToString())
                            {
                                case "1"://calypso三坐标
                                    string mesPath = path + @"\qFlow_Programs\mes_pros.exe";
                                    string para = "filter -filterkey 22030=" + jobAttributesList["22030"] + ",10030=" + jobAttributesList["10030"] + " -filterptx Task_Confirm\\Inspection_List.ptx -openptx -splash big";
                                    log.Info("切换到三坐标程序下载界面 指令:"+para);
                                    RealAction(mesPath, para);
                                    //string operatorKanban = parentPath + @"\Kanbans\Operator_Kanban.ptx";
                                    //RealAction("C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe", operatorKanban);
                                    break;
                                case "4"://量规
                                    if (reworkLabel != "")
                                    {
                                        ptxPath = parentPath + @"\Manual\Remeasure_Manual_FirstPage.ptx";
                                    }
                                    else
                                    {
                                        ptxPath = parentPath + @"\Manual\Manual_FirstPage.ptx";//
                                    }
                                    //string openPtxStr = " -open " + "\"" + ptxPath + "\"" + " -initiallyCheckedGenericDataBindingPart " + "\"" + @"/Process/Job_Management" + "\"" + " -searchCriteria " + "\"" + path_exe + "\\JobNum&TaskNumNew.msel" + " -nosplash -maximize";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe\"" +                                 
                                    piwebInterfaceParas = new string[6];
                                    piwebInterfaceParas[0] = jobnumber;
                                    piwebInterfaceParas[1] = tasknumber;
                                    piwebInterfaceParas[2] = jobAttributesList["22251"].ToString();
                                    piwebInterfaceParas[3] = "/Process/Job_Management/";//Job_Management
                                    piwebInterfaceParas[4] = ptxPath;
                                    piwebInterfaceParas[5] = "3000";
                                    openPtxStr = piwebInterfaceParas[0] + " " + piwebInterfaceParas[1] + " " + piwebInterfaceParas[2] + " " + piwebInterfaceParas[3] + " \"" + piwebInterfaceParas[4] + "\" " + piwebInterfaceParas[5];
                                    log.Info(openPtxStr);
                                    RealAction(piwebInterface, openPtxStr);
                                    log.Info("量规测量模板打开完成");
                                    break;
                                case "2"://圆度仪
                                    ptxPath = parentPath + @"\Task_Confirm\Inspection_List.ptx";//\Self_Check\Task_Conclusion_ProgramDisenable.ptx
                                                                                                //string openPtxStr = " -open " + "\"" + ptxPath + "\"" + " -initiallyCheckedGenericDataBindingPart " + "\"" + @"/Process/Job_Management" + "\"" + " -searchCriteria " + "\"" + path_exe + "\\JobNum&TaskNumNew.msel" + " -nosplash -maximize";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe\"" +                                 
                                    piwebInterfaceParas = new string[6];
                                    piwebInterfaceParas[0] = productIdent;
                                    piwebInterfaceParas[1] = software;
                                    piwebInterfaceParas[2] = jobAttributesList["22251"].ToString();
                                    piwebInterfaceParas[3] = "/Process/Inspection_List/";//Job_Management
                                    piwebInterfaceParas[4] = ptxPath;
                                    piwebInterfaceParas[5] = "3000";
                                    openPtxStr = piwebInterfaceParas[0] + " " + piwebInterfaceParas[1] + " " + piwebInterfaceParas[2] + " " + piwebInterfaceParas[3] + " \"" + piwebInterfaceParas[4] + "\" " + piwebInterfaceParas[5];
                                    log.Info(openPtxStr);
                                    log.Info(piwebInterface);
                                    RealAction(piwebInterface, openPtxStr);
                                    log.Info("圆度仪程序选择模板打开完成");
                                    break;
                                case "3"://粗糙度仪
                                    ptxPath = parentPath + @"\Task_Confirm\Inspection_List.ptx";//\Self_Check\Task_Conclusion_ProgramDisenable.ptx
                                                                                                //string openPtxStr = " -open " + "\"" + ptxPath + "\"" + " -initiallyCheckedGenericDataBindingPart " + "\"" + @"/Process/Job_Management" + "\"" + " -searchCriteria " + "\"" + path_exe + "\\JobNum&TaskNumNew.msel" + " -nosplash -maximize";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe\"" +                                 
                                    piwebInterfaceParas = new string[6];
                                    piwebInterfaceParas[0] = productIdent;
                                    piwebInterfaceParas[1] = software;
                                    piwebInterfaceParas[2] = jobAttributesList["22251"].ToString();
                                    piwebInterfaceParas[3] = "/Process/Inspection_List/";
                                    piwebInterfaceParas[4] = ptxPath;
                                    piwebInterfaceParas[5] = "3000";
                                    openPtxStr = piwebInterfaceParas[0] + " " + piwebInterfaceParas[1] + " " + piwebInterfaceParas[2] + " " + piwebInterfaceParas[3] + " \"" + piwebInterfaceParas[4] + "\" " + piwebInterfaceParas[5];
                                    log.Info(openPtxStr);
                                    log.Info(piwebInterface);
                                    RealAction(piwebInterface, openPtxStr);
                                    log.Info("粗糙度仪程序选择模板打开完成");
                                    break;
                            }

                            Application.Exit();
                        }
                        else if (funLabel == "UploadSelect")
                        {
                            log.Info("Start UploadSelect...args count"+args.Count());
                            if (args.Count() == 4)
                            {
                                log.Info("UploadSelect args:"+args[1] + "," + args[2] + "," + args[3]);
                                productIdent = args[1];
                                software = args[2];
                                inspectionName = args[3];
                                inspectionPath = "";
                            }
                            else if (args.Count() == 5)
                            {
                                log.Info("UploadSelect args:" + args[1] + "," + args[2] + "," + args[3] + "," + args[4]);
                                productIdent = args[1];
                                software = args[2];
                                inspectionName = args[3];
                                inspectionPath = args[4];
                            }
                            DirectoryInfo path_exe = new DirectoryInfo(Application.StartupPath); //exe目录
                            String path = path_exe.Parent.FullName; //上级的目录
                            String parentPath = path_exe.Parent.Parent.FullName;
                            piwebInterface = path_exe + @"\PiWebCommandLineInterface.exe";

                            Dictionary<string, string> jobInfo = getJobInfo(path + @"\temp\job_info.para");
                            jobAttributesList = new Dictionary<string, object>();
                            jobnumber = jobInfo["22250"].ToString();
                            tasknumber = jobInfo["22253"].ToString();
                            reworkLabel = jobInfo["9"].ToString();
                            gageEquipment = jobInfo["12"].ToString();
                            checkJobInfo(jobnumber, tasknumber, reworkLabel);//获取job信息
                            //updateJobInfo(jobnumber, tasknumber, reworkLabel, 3);
                           //更新设备为Busy
                            updateStatus2Machine(gageEquipment, "Busy");
                            switch (jobAttributesList["10030"].ToString())
                            {
                                case "1"://calypso三坐标
                                    string mesPath = path + @"\qFlow_Programs\mes_pros.exe";
                                    inspectionPath = inspectionPath.Replace(" ", "%20");
                                    string para= "download -path "+inspectionPath + " -inspection "+inspectionName;
                                    //string para = "filter -filterkey 22030=" + jobAttributesList["22030"] + ",10030=" + jobAttributesList["10030"] + " -filterptx Task_Confirm\\Inspection_List.ptx -openptx -splash big";
                                    log.Info("三坐标下载程序指令:"+mesPath+para);
                                    RealAction(mesPath, para);
                                   
                                    //string operatorKanban = parentPath + @"\Kanbans\Operator_Kanban.ptx";
                                    //RealAction("C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe", operatorKanban);
                                    break;
                                case "4"://量规
                                    if (reworkLabel != "")
                                    {
                                        ptxPath = parentPath + @"\Manual\Remeasure_Manual_FirstPage.ptx";
                                    }
                                    else
                                    {
                                        ptxPath = parentPath + @"\Manual\Manual_FirstPage.ptx";//
                                    }
                                    //string openPtxStr = " -open " + "\"" + ptxPath + "\"" + " -initiallyCheckedGenericDataBindingPart " + "\"" + @"/Process/Job_Management" + "\"" + " -searchCriteria " + "\"" + path_exe + "\\JobNum&TaskNumNew.msel" + " -nosplash -maximize";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe\"" +                                 
                                    piwebInterfaceParas = new string[6];
                                    piwebInterfaceParas[0] = jobnumber;
                                    piwebInterfaceParas[1] = tasknumber;
                                    piwebInterfaceParas[2] = jobAttributesList["22251"].ToString();
                                    piwebInterfaceParas[3] = "/Process/Job_Management/";//Job_Management
                                    piwebInterfaceParas[4] = ptxPath;
                                    piwebInterfaceParas[5] = "3000";
                                    openPtxStr = piwebInterfaceParas[0] + " " + piwebInterfaceParas[1] + " " + piwebInterfaceParas[2] + " " + piwebInterfaceParas[3] + " \"" + piwebInterfaceParas[4] + "\" " + piwebInterfaceParas[5];
                                    log.Info(openPtxStr);
                                    RealAction(piwebInterface, openPtxStr);
                                    log.Info("量规测量模板打开完成");
                                    break;
                                case "2"://圆度仪
                                    if (reworkLabel != "")
                                    {
                                        ptxPath = parentPath + @"\Self_Check\Remeasure_Task_Conclusion_NonZeissProgram.ptx";//Remeasure_Task_Conclusion_ProgramDisenable.ptx
                                    }
                                    else
                                    {
                                        ptxPath = parentPath + @"\Self_Check\Task_Conclusion_NonZeissProgram.ptx";// Task_Conclusion_ProgramDisenable.ptx
                                    }
                                   //\Self_Check\Task_Conclusion_ProgramDisenable.ptx
                                                                                                               //string openPtxStr = " -open " + "\"" + ptxPath + "\"" + " -initiallyCheckedGenericDataBindingPart " + "\"" + @"/Process/Job_Management" + "\"" + " -searchCriteria " + "\"" + path_exe + "\\JobNum&TaskNumNew.msel" + " -nosplash -maximize";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe\"" +                                 
                                    piwebInterfaceParas = new string[6];
                                    piwebInterfaceParas[0] = jobnumber;
                                    piwebInterfaceParas[1] = tasknumber;
                                    piwebInterfaceParas[2] = jobAttributesList["22251"].ToString();
                                    piwebInterfaceParas[3] = "/Process/Job_Management/";//Job_Management
                                    piwebInterfaceParas[4] = ptxPath;
                                    piwebInterfaceParas[5] = "3000";
                                    openPtxStr = piwebInterfaceParas[0] + " " + piwebInterfaceParas[1] + " " + piwebInterfaceParas[2] + " " + piwebInterfaceParas[3] + " \"" + piwebInterfaceParas[4] + "\" " + piwebInterfaceParas[5];
                                    log.Info(openPtxStr);
                                    log.Info(piwebInterface);
                                    RealAction(piwebInterface, openPtxStr);
                                    log.Info("圆度仪测量模板打开完成");
                                    log.Info("圆度仪服务器测量程序路径：" + inspectionPath + ",测量程序名称：" + inspectionName);
                                    if (inspectionPath != ""&&inspectionName!="None")
                                    {
                                        //下载程序到本地
                                        log.Info("准备下载圆度仪测量程序到本地");
                                        if (CopyOldLabFilesToNewLab(inspectionPath, easyFormInspectionPlanPath+@"\"+inspectionName))
                                        {
                                            log.Info("下载圆度仪测量程序到本地完成");
                                            System.Diagnostics.Process.Start("explorer.exe", easyFormInspectionPlanPath + @"\" + inspectionName);
                                            log.Info("打开EasyForm测量程序目录:" + easyFormInspectionPlanPath + @"\" + inspectionName);
                                        }
                                        
                                    }
                                    break;
                                case "3"://粗糙度仪
                                    if (reworkLabel != "")
                                    {
                                        ptxPath = parentPath + @"\Self_Check\Remeasure_Task_Conclusion_NonZeissProgram.ptx";
                                    }
                                    else
                                    {
                                        ptxPath = parentPath + @"\Self_Check\Task_Conclusion_NonZeissProgram.ptx";
                                    }
                                    //\Self_Check\Task_Conclusion_ProgramDisenable.ptx
                                                                                                               //string openPtxStr = " -open " + "\"" + ptxPath + "\"" + " -initiallyCheckedGenericDataBindingPart " + "\"" + @"/Process/Job_Management" + "\"" + " -searchCriteria " + "\"" + path_exe + "\\JobNum&TaskNumNew.msel" + " -nosplash -maximize";//"\"C:\\Program Files\\Zeiss\\PiWeb\\Monitor.exe\"" +                                 
                                    piwebInterfaceParas = new string[6];
                                    piwebInterfaceParas[0] = jobnumber;
                                    piwebInterfaceParas[1] = tasknumber;
                                    piwebInterfaceParas[2] = jobAttributesList["22251"].ToString();
                                    piwebInterfaceParas[3] = "/Process/Job_Management/";
                                    piwebInterfaceParas[4] = ptxPath;
                                    piwebInterfaceParas[5] = "3000";
                                    openPtxStr = piwebInterfaceParas[0] + " " + piwebInterfaceParas[1] + " " + piwebInterfaceParas[2] + " " + piwebInterfaceParas[3] + " \"" + piwebInterfaceParas[4] + "\" " + piwebInterfaceParas[5];
                                    log.Info(openPtxStr);
                                    log.Info(piwebInterface);
                                    RealAction(piwebInterface, openPtxStr);
                                    log.Info("粗糙度仪测量模板打开完成");
                                    log.Info("粗糙度仪服务器测量程序路径：" + inspectionPath + ",测量程序名称：" + inspectionName);
                                    if (inspectionPath != "" && inspectionName != "None")
                                    {
                                        //下载程序到本地
                                        log.Info("准备下载粗糙度仪测量程序到本地");
                                        if (CopyOldLabFilesToNewLab(inspectionPath, marSurfInspectionPlanPath + @"\" + inspectionName))
                                        {
                                            log.Info("下载粗糙度仪测量程序到本地完成");
                                            System.Diagnostics.Process.Start("explorer.exe", marSurfInspectionPlanPath + @"\" + inspectionName);
                                            log.Info("打开MarSurf测量程序目录:" + marSurfInspectionPlanPath + @"\" + inspectionName);
                                        }
                                       
                                    }
                                    break;
                            }
                            updateJobInfo(jobnumber, tasknumber, reworkLabel, 3);
                            Application.Exit();
                        }
                        else if (funLabel == "ChangeMachineStatus")
                        {
                            if (args.Count() == 3)
                            {
                                gageEquipment = args[1];
                                machineStatus = args[2];
                            }
                            log.Info("开始更新设备:" + gageEquipment + " 状态为:" + machineStatus);
                            updateStatus2Machine(gageEquipment, machineStatus);
                            log.Info("完成更新设备:" + gageEquipment + " 状态为:" + machineStatus);
                        }

                    }
                    else if (args.Count() == 1)
                    {
                            log.Info("测量程序开始执行");
                            //getCurrentOpenInsepctionName();
                            currentOpenInspectionPlanName = readStartFile();
                            log.Info("正在执行测量程序是：" + currentOpenInspectionPlanName);
                            jobPara = getJobInfo(calypsoInspectionPlanPath + @"\" + currentOpenInspectionPlanName + @"\job_number.para");
                            log.Info("获取的jobNumber.para数据是" + jobPara.ToString());
                            if (jobPara != null)
                            {
                                if (jobPara["12"].Contains(' '))
                                {
                                    jobPara["12"] = jobPara["12"].Split(' ')[0] + "%20" + jobPara["12"].Split(' ')[1];
                                }
                                if (checkJobInfo(jobPara["22250"], jobPara["22253"], jobPara["9"]))//检查数据库里是否存在正在测量的Job和Task
                                {
                                    if (args[0] == "Start")
                                    {
                                        log.Info("Start 开始获取已经测量完成的件数");
                                        alreadyMeasureIndex = getPartMeasurementCount(currentOpenInspectionPlanName, jobPara["22250"], jobPara["22253"], jobPara["22037"], jobPara["22221"], jobPara["22222"], jobPara["22033"], jobPara["22031"], jobPara["9"], jobPara["14"], args[0]);
                                        log.Info("Start 获取已经测量完成件数完成，" + currentOpenInspectionPlanName + "对应的jobNumber" + jobPara["22250"] + "/TaskNumber" + jobPara["22253"] + "/Customer" + jobPara["22037"] + " /Temp " + jobPara["22221"] + " /humidity" + jobPara["22222"] + " /ProductStatus" + jobPara["22033"] + " /ProductName" + jobPara["22031"]);
                                        log.Info("Start 开始更新Job状态信息");
                                        updateJobInfo(jobPara["22250"], jobPara["22253"], "", "", "3", false, alreadyMeasureIndex, currentOpenInspectionPlanName);
                                        log.Info("Start 更新设备Job信息完成，jobNumber:" + jobPara["22250"] + " JobStatus:3  updateGageFlag:false");

                                        log.Info("Start 开始更新设备状态信息");
                                        if (alreadyMeasureIndex == 0)
                                        {
                                            currentOperator = jobPara["8"];
                                        }
                                        putPartIndex2Machine(jobPara["12"], jobPara["22250"], int.Parse(jobPara["22035"]), alreadyMeasureIndex, "Busy", jobPara["22253"], jobPara["22033"], jobPara["22031"], currentOperator, currentOpenInspectionPlanName, jobPara["22258"]);
                                        log.Info("Start 设备状态信息更新完成,machineName:" + jobPara["12"] + " JobNumber:" + jobPara["22250"] + "  machineState:" + "Busy" + " /ProductStatus" + jobPara["22033"] + " /ProductName" + jobPara["22031"] + " /currentOperator" + currentOperator + " /currentInspectionName" + currentOpenInspectionPlanName + "/inspectionType:" + jobPara["22258"]);
                                    }
                                    if (args[0] == "Stop")
                                    {
                                        log.Info("Stop 开始获取测量程序名称StartFile");
                                        currentOpenInspectionPlanName = readStartFile();
                                        log.Info("Stop 获取测量程序名称StartFile完成；inspectionName:" + currentOpenInspectionPlanName);
                                        if (currentOpenInspectionPlanName != "")
                                        {
                                            //timerCheckSyncData.Enabled = true;
                                            checkSyncDataCount++;
                                            bool updateGageFlag = false;
                                            string jobStatus = "";
                                            log.Info("Stop 开始获取已经测量完成的件数");
                                            alreadyMeasureIndex = getPartMeasurementCount(currentOpenInspectionPlanName, jobPara["22250"], jobPara["22253"], jobPara["22037"], jobPara["22221"], jobPara["22222"], jobPara["22033"], jobPara["22031"], jobPara["9"], jobPara["14"], args[0]);
                                            log.Info("Stop 获取已经测量完成件数" + alreadyMeasureIndex + " 完成，" + currentOpenInspectionPlanName + "对应的jobNumber" + jobPara["22250"] + "/TaskNumber" + jobPara["22253"] + "/Customer" + jobPara["22037"] + " /Temp " + jobPara["22221"] + " /humidity" + jobPara["22222"] + " /ProductStatus" + jobPara["22033"] + " /ProductName" + jobPara["22031"]);
                                            if (int.Parse(jobPara["22035"]) == alreadyMeasureIndex)
                                            {
                                                jobStatus = "3";
                                            }
                                            else
                                            {
                                                jobStatus = "3";
                                            }

                                            if (alreadyMeasureIndex != 0)
                                            {

                                                //if (jobPara["22000"] != equipmentData.Split('-')[0].Trim())
                                                //{
                                                //    DialogResult dr = MessageBox.Show("正在使用的测量设备与分配的任务中指定设备不一致,是否更新任务中的设备信息？", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                                                //    if (dr == DialogResult.OK)
                                                //    {
                                                //        //更新任务中的设备信息
                                                //        updateGageFlag = true;
                                                //    }
                                                //    else
                                                //    {
                                                updateGageFlag = false;
                                                //    }
                                                //}
                                                log.Info("Stop 开始更新Job状态信息");
                                                updateJobInfo(jobPara["22250"], jobPara["22253"], equipmentData.Split('-')[0].Trim(), equipmentData.Split('-')[1].Trim(), jobStatus, updateGageFlag, alreadyMeasureIndex, currentOpenInspectionPlanName);
                                                log.Info("Stop 更新设备Job信息完成，jobNumber:" + jobPara["22250"] + " equipmentSN:" + equipmentData.Split('-')[0].Trim() + " gageEquipment:" + equipmentData.Split('-')[1].Trim() + " JobStatus:" + jobStatus + " updateGageFlag:" + updateGageFlag + "");
                                                if (updateGageFlag)
                                                {
                                                    log.Info("Stop 开始更新jobnumber.para");
                                                    string jobNumberFile = calypsoInspectionPlanPath + @"\" + currentOpenInspectionPlanName + @"\job_number.para";
                                                    StreamWriter sw = new StreamWriter(jobNumberFile);
                                                    jobPara["22000"] = equipmentData.Split('-')[0].Trim();
                                                    foreach (var para in jobPara)
                                                    {
                                                        sw.WriteLine(para.Key + "=" + para.Value);
                                                    }
                                                    sw.Close();
                                                    log.Info("Stop 更新jobnumber.para完成");
                                                }
                                                log.Info("Stop 开始更新设备状态信息");
                                                putPartIndex2Machine(jobPara["12"], jobPara["22250"], int.Parse(jobPara["22035"]), alreadyMeasureIndex, "Busy", jobPara["22253"], jobPara["22033"], jobPara["22031"], currentOperator, currentOpenInspectionPlanName, jobPara["22258"]);//Available
                                                log.Info("Stop 设备状态信息更新完成,machineName:" + jobPara["12"] + " JobNumber:" + jobPara["22250"] + "  machineState:" + "Busy" + " /ProductStatus" + jobPara["22033"] + " /ProductName" + jobPara["22031"] + " /currentOperator" + currentOperator + " /currentInspectionName:" + currentOpenInspectionPlanName + "/inspectionType:" + jobPara["22258"]);
                                                log.Info("Stop 完成，退出程序");
                                                Application.Exit();
                                            }
                                            //else
                                            //{
                                            //    if (checkSyncDataCount == 5)
                                            //    {
                                            //        Application.Exit();
                                            //        //timerCheckSyncData.Enabled = false;
                                            //    }
                                            //}
                                        }
                                        else
                                        {
                                            log.Error("Stop 获取测量程序名称出错");
                                            MessageBox.Show("获取测量程序名称出错");
                                            log.Error("Stop 获取测量程序名称出错后，更新设备信息");
                                            putPartIndex2Machine(jobPara["12"], jobPara["22250"], int.Parse(jobPara["22035"]), alreadyMeasureIndex, "Warning", jobPara["22253"], jobPara["22033"], jobPara["22031"], currentOperator, currentOpenInspectionPlanName, jobPara["22258"]);
                                            log.Error("Stop 获取测量程序名称出错后，设备状态信息更新完成,machineName:" + jobPara["12"] + " JobNumber:" + jobPara["22250"] + "  machineState:Warning" + " /ProductStatus" + jobPara["22033"] + " /ProductName" + jobPara["22031"] + " /currentOperator" + currentOperator + " /currentInspectionName" + currentOpenInspectionPlanName + "/inspectionType:" + jobPara["22258"]);
                                            log.Error("Stop 完成，退出程序");
                                            Application.Exit();
                                        }
                                    }
                                    Application.Exit();
                                }
                                else
                                {
                                    log.Error("数据库中不存在正在测量的JobNumber:" + jobPara["22250"] + " & TaskNumber:" + jobPara["22253"] + "");
                                    MessageBox.Show("数据库中不存在正在测量的JobNumber:" + jobPara["22250"] + " & TaskNumber:" + jobPara["22253"] + "");
                                    Application.Exit();
                                }

                            }
                            else
                            {
                                log.Error("jobNumber.para文件出错");
                                Application.Exit();
                            }
                            if (args[0] == "Start")
                            {
                                log.Info("Start 完成，退出程序");
                                Application.Exit();
                            }
                    }
                    else
                    {
                        log.Warn("输入的运行参数错误");
                        MessageBox.Show("输入的运行参数错误");
                        Application.Exit();
                    }
                }
                else
                {
                    Application.Exit();
                }
            }
            catch(Exception ex)
            {
                log.Error("machineInterface程序运行出错" + ex.ToString());
                MessageBox.Show("machineInterface程序运行出错" + ex.ToString());
                Application.Exit();
            }
            

        }
        /// <summary>
        /// 拷贝oldlab的文件到newlab下面
        /// </summary>
        /// <param name="sourcePath">lab文件所在目录(@"~\labs\oldlab")</param>
        /// <param name="savePath">保存的目标目录(@"~\labs\newlab")</param>
        /// <returns>返回:true-拷贝成功;false:拷贝失败</returns>
        public bool CopyOldLabFilesToNewLab(string sourcePath, string savePath)
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            #region //拷贝labs文件夹到savePath下
            try
            {
                string[] labDirs = Directory.GetDirectories(sourcePath);//目录
                string[] labFiles = Directory.GetFiles(sourcePath);//文件
                if (labFiles.Length > 0)
                {
                    for (int i = 0; i < labFiles.Length; i++)
                    {
                        if (Path.GetExtension(labFiles[i]) != ".lab")//排除.lab文件
                        {
                            File.Copy(sourcePath + "\\" + Path.GetFileName(labFiles[i]), savePath + "\\" + Path.GetFileName(labFiles[i]), true);
                        }
                    }
                }
                if (labDirs.Length > 0)
                {
                    for (int j = 0; j < labDirs.Length; j++)
                    {
                        Directory.GetDirectories(sourcePath + "\\" + Path.GetFileName(labDirs[j]));

                        //递归调用
                        CopyOldLabFilesToNewLab(sourcePath + "\\" + Path.GetFileName(labDirs[j]), savePath + "\\" + Path.GetFileName(labDirs[j]));
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            #endregion
            return true;
        }
        int checkSyncDataCount;
        private void timerCheckSyncData_Tick(object sender, EventArgs e)
        {
            string jobNumberFile1 = @"C:\ZJQ\qFlow\1.para";
            StreamWriter sw1 = new StreamWriter(jobNumberFile1);
       
            checkSyncDataCount++;
            bool updateGageFlag = false;
            string jobStatus = "";
            alreadyMeasureIndex = getPartMeasurementCount(currentOpenInspectionPlanName, jobPara["22250"], jobPara["22253"], jobPara["22037"], jobPara["22221"], jobPara["22222"], jobPara["22033"], jobPara["22031"], jobPara["9"], jobPara["14"], args[0]);
            if (int.Parse(jobPara["22035"]) == alreadyMeasureIndex)
            {
                jobStatus = "Job_Done";
            }
            else
            {
                jobStatus = "Job_Running";
            }
            sw1.WriteLine(alreadyMeasureIndex);
            sw1.Close();

            if (alreadyMeasureIndex != 0)
            {

                if (jobPara["22000"] != equipmentData.Split('-')[0].Trim())
                {
                    DialogResult dr = MessageBox.Show("正在使用的测量设备与分配的任务中指定设备不一致,是否更新任务中的设备信息？", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (dr == DialogResult.OK)
                    {
                        //更新任务中的设备信息
                        updateGageFlag = true;
                    }
                    else
                    {
                        updateGageFlag = false;
                    }
                }
                updateJobInfo(jobPara["22250"], jobPara["22253"], equipmentData.Split('-')[0].Trim(), equipmentData.Split('-')[1].Trim(), jobStatus, updateGageFlag, alreadyMeasureIndex, currentOpenInspectionPlanName);
                if (updateGageFlag)
                {
                    string jobNumberFile = @"C:\ZJQ\qFlow\job_number.para";
                    StreamWriter sw = new StreamWriter(jobNumberFile);
                    jobPara["22000"] = equipmentData.Split('-')[0].Trim();
                    foreach (var para in jobPara)
                    {
                        sw.WriteLine(para.Key + "=" + para.Value);
                    }
                    sw.Close();
                }
                putPartIndex2Machine(jobPara["12"], jobPara["22250"], int.Parse(jobPara["22035"]), alreadyMeasureIndex, args[0], jobPara["22253"], jobPara["22033"], jobPara["22031"], jobPara["8"], currentOpenInspectionPlanName,jobPara["22258"]);
                Application.Exit();
            }
            else
            {
                if (checkSyncDataCount == 5)
                {
                    Application.Exit();
                    //timerCheckSyncData.Enabled = false;
                }
            }
        }

        //读取startFile里的测量程序名称
        //string calypsoStartFilePath = @"C:\Users\Public\Documents\Zeiss\CALYPSO\workarea\inspections\startfile";
        private string readStartFile()
        {
            string inspectionName = "";
            StreamReader sr = new StreamReader(calypsoInspectionPlanPath + @"\startfile");
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains("planid"))
                {
                    inspectionName = line.Split('\t')[1];
                }
            }
            sr.Close();
            return inspectionName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //getPartMeasurementCount("8", "JOB190305185327", "1", "Zeiss");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            updateJobInfo("JOB190308095551", "1", "", "", "3", false, alreadyMeasureIndex, currentOpenInspectionPlanName);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            jobAttributesList = new Dictionary<string, object>();
            checkJobInfo("JOB190325094406", "1", "");
            deleteMeasurement("JOB190325094406", "1", "");
            putPartIndex2Machine(jobAttributesList["12"].ToString(), jobAttributesList["22250"].ToString(), int.Parse(jobAttributesList["22035"].ToString()), manualMeasurementCount, "Avaiable", jobAttributesList["22253"].ToString(), jobAttributesList["22033"].ToString(), jobAttributesList["22031"].ToString(), jobAttributesList["8"].ToString(), "", jobAttributesList["22258"].ToString());

        }
    }
}
