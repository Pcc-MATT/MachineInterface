using Newtonsoft.Json;
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
using Zeiss.IMT.PiWeb.Api.Common.Data;
using Zeiss.IMT.PiWeb.Api.DataService.Rest;

namespace CreatNewDemo
{
    // 1.定义委托
    public delegate void DelReadStdOutput(string result);
    public delegate void DelReadErrOutput(string result);
    public partial class MainForm : Form
    {
        // 2.定义委托事件
        public event DelReadStdOutput ReadStdOutput;
        public event DelReadErrOutput ReadErrOutput;
        public MainForm()
        {
            InitializeComponent();
            //3.将相应函数注册到委托事件中
            ReadStdOutput += new DelReadStdOutput(ReadStdOutputAction);
            ReadErrOutput += new DelReadErrOutput(ReadErrOutputAction);
        }
        List<string> ownerList = new List<string>();
        List<string> salesList = new List<string>();
        List<string> demoEngineerList = new List<string>();
        List<string> customerList = new List<string>();
        List<string> cmmList = new List<string>();
        List<string> sensorList = new List<string>();
        List<Customer> customers = new List<Customer>();

        private async void MainForm_Load(object sender, EventArgs e)
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
            await FetchMeasurements4FileServerIP(characteristicPath);
        }
        #region 设置Combobox的方法
        //得到Combobox的数据，返回一个List
        public List<string> getComboboxItems(ComboBox cb)
        {
            try
            {
                //初始化绑定默认关键词
                List<string> listOnit = new List<string>();
                //将数据项添加到listOnit中
                for (int i = 0; i < cb.Items.Count; i++)
                {
                    listOnit.Add(cb.Items[i].ToString());
                }
                return listOnit;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }


        }
        //模糊查询Combobox
        public void selectCombobox(ComboBox cb, List<string> listOnit)
        {
            try
            {
                //输入key之后返回的关键词
                List<string> listNew = new List<string>();
                //清空combobox
                cb.Items.Clear();
                //清空listNew
                listNew.Clear();
                //遍历全部备查数据
                foreach (var item in listOnit)
                {
                    if (item.Contains(cb.Text))
                    {
                        //符合，插入ListNew
                        listNew.Add(item);
                    }
                }

                //combobox添加已经查询到的关键字
                cb.Items.AddRange(listNew.ToArray());
                //设置光标位置，否则光标位置始终保持在第一列，造成输入关键词的倒序排列
                cb.SelectionStart = cb.Text.Length;
                //保持鼠标指针原来状态，有时鼠标指针会被下拉框覆盖，所以要进行一次设置
                Cursor = Cursors.Default;
                //自动弹出下拉框
                cb.DroppedDown = true;


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        private List<string> listCombobox1;//Combobox的最初Item项
        private List<string> listCombobox2;//Combobox的最初Item项
        private List<string> listCombobox3;//Combobox的最初Item项
        private List<string> listCombobox4;//Combobox的最初Item项
        private List<string> listCombobox7;//Combobox的最初Item项
        private List<string> listCombobox8;//Combobox的最初Item项
        private void comboBox2_TextUpdate(object sender, EventArgs e)
        {
            if (listCombobox2.Count() > 0)
            {
                selectCombobox(comboBox2, listCombobox2);
            }
            
        }

        private void comboBox1_TextUpdate(object sender, EventArgs e)
        {
            if (listCombobox1.Count() > 0)
            {
                selectCombobox(comboBox1, listCombobox1);
            }
               
        }

        private void comboBox3_TextUpdate(object sender, EventArgs e)
        {
            if (listCombobox3.Count > 0)
            {
                selectCombobox(comboBox3, listCombobox3);
            }
           
        }

        private void comboBox4_TextUpdate(object sender, EventArgs e)
        {
            if (listCombobox4.Count > 0)
            {
                selectCombobox(comboBox4, listCombobox4);
            }
           
        }

        private void comboBox7_TextUpdate(object sender, EventArgs e)
        {
            if (listCombobox7.Count > 0)
            {
                selectCombobox(comboBox7, listCombobox7);
            }
         
        }

        private void comboBox8_TextUpdate(object sender, EventArgs e)
        {
            if (listCombobox8.Count > 0)
            {
                selectCombobox(comboBox8, listCombobox8);
            }
            
        }
        #endregion

        Dictionary<string, object> PiWebHost_Data;
        Dictionary<string, object> CC_Data = new Dictionary<string, object>();
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
                var ss4 = JsonConvert.SerializeObject(valuePairs["CC"], Formatting.Indented);
                CC_Data.Add("CC", valuePairs["CC"].ToString());
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
                label18.Text = "PiWeb连接失败";
                connect2PiWebServer = false;
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
		/// This method fetches the most recent 100 measurements for the selected part. Please have a look at the other properties inside 
		/// the filter class to understand all possibilities of filtering.
		/// </summary>
		private async Task FetchMeasurements4FileServerIP(PathInformation partPath)
        {
            SimpleMeasurement[] _Measurements = new SimpleMeasurement[0];
            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                _Measurements = (await _RestDataServiceClient.GetMeasurements(partPath, new MeasurementFilterAttributes
                {
                    LimitResult = 1000
                })).ToArray();//
                sw.Stop();
                foreach (var mes in _Measurements)
                {
                    if (mes.GetAttributeValue((ushort)20028) != null)
                    {
                        if (ownerList.FindAll(n => n == mes.GetAttributeValue((ushort)20028)).Count() == 0)
                        {
                            ownerList.Add(mes.GetAttributeValue((ushort)20028));
                            comboBox1.Items.Add(mes.GetAttributeValue((ushort)20028));
                        }
                    }
                    if (mes.GetAttributeValue((ushort)20004) != null)
                    {
                        if (salesList.FindAll(n => n == mes.GetAttributeValue((ushort)20004)).Count() == 0)
                        {
                            salesList.Add(mes.GetAttributeValue((ushort)20004));
                            comboBox2.Items.Add(mes.GetAttributeValue((ushort)20004));
                        }
                    }
                    if (mes.GetAttributeValue((ushort)20003) != null)
                    {
                        if (demoEngineerList.FindAll(n => n == mes.GetAttributeValue((ushort)20003)).Count() == 0)
                        {
                            demoEngineerList.Add(mes.GetAttributeValue((ushort)20003));
                            comboBox3.Items.Add(mes.GetAttributeValue((ushort)20003));
                        }
                    }
                    if (mes.GetAttributeValue((ushort)1062) != null)
                    {
                        if (customerList.FindAll(n => n == mes.GetAttributeValue((ushort)1062)).Count() == 0)
                        {
                            customerList.Add(mes.GetAttributeValue((ushort)1062));
                            comboBox4.Items.Add(mes.GetAttributeValue((ushort)1062));
                            customers.Add(new Customer
                            {
                                name = mes.GetAttributeValue((ushort)1062),
                                province = mes.GetAttributeValue((ushort)20001),
                                city = mes.GetAttributeValue((ushort)20002)
                            });
                        }
                    }
                    if (mes.GetAttributeValue((ushort)20051) != null)
                    {
                        if (cmmList.FindAll(n => n == mes.GetAttributeValue((ushort)20051)).Count() == 0)
                        {
                            cmmList.Add(mes.GetAttributeValue((ushort)20051));
                            comboBox7.Items.Add(mes.GetAttributeValue((ushort)20051));
                        }
                    }
                    if (mes.GetAttributeValue((ushort)20052) != null)
                    {
                        if (sensorList.FindAll(n => n == mes.GetAttributeValue((ushort)20052)).Count() == 0)
                        {
                            sensorList.Add(mes.GetAttributeValue((ushort)20052));
                            comboBox8.Items.Add(mes.GetAttributeValue((ushort)20052));
                        }
                    }

                }
                listCombobox1 = getComboboxItems(this.comboBox1);//获取Item
                listCombobox2 = getComboboxItems(this.comboBox2);//获取Item
                listCombobox3 = getComboboxItems(this.comboBox3);//获取Item
                listCombobox4 = getComboboxItems(this.comboBox4);//获取Item
                listCombobox7 = getComboboxItems(this.comboBox7);//获取Item
                listCombobox8 = getComboboxItems(this.comboBox8);//获取Item
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox4.Text != "" && this.comboBox4.Text != null)
            {
                Customer selectCustomer = customers.Find(n => n.name == comboBox4.Text);
                if (selectCustomer != null)
                {
                    comboBox5.Text = selectCustomer.province;
                    comboBox6.Text = selectCustomer.city;
                }
            }

        }

        private async void button2_Click(object sender, EventArgs e)
        {
            //if (comboBox9.Text == "")
            //{
            //    MessageBox.Show("未选择Demo类型");
            //}
            //else
            string demoType = "CT";
            if (radioButton1.Checked)
            {
                demoType = "CT";
            }
            if (radioButton2.Checked)
            {
                demoType = "CMM";
            }
            if (radioButton3.Checked)
            {
                demoType = "GEAR";
            }
            if (radioButton4.Checked)
            {
                demoType = "BLADE";
            }
            if (radioButton5.Checked)
            {
                demoType = "OPTICS";
            }
            {
                DialogResult dialogResult = MessageBox.Show("是否需要创建Demo？", "创建Demo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    creatNewDemo(demoType);
                }
            }

        }
        bool creatSuccessed;
        private async void creatNewDemo(string demoType)
        {
            try
            {
                creatSuccessed = true;
                var partPath = PathHelper.String2PartPathInformation("/Demo/");
                var parts = await _RestDataServiceClient.GetParts(partPath);
                if (parts.Count() != 0)
                {
                    var Part = (InspectionPlanPart)parts.ToList()[0];
                    var attributes = new List<Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute>();
                    //demo type
                    attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)20043, demoType + "_Demo"));
                    //serial number
                    string date = DateTime.Now.ToString("yyyyMMddHHmmss");
                    attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)14, CC_Data["CC"] + date));
                    //CC
                    attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)801, CC_Data["CC"]));
                    //owner
                    if (comboBox1.Text != "")
                    {
                        attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)20028, comboBox1.Text.Trim()));
                    }
                    else
                    {
                        MessageBox.Show("专案负责人未填写！");
                        creatSuccessed = false;
                    }
                    //sales
                    if (comboBox2.Text != "")
                    {
                        attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)20004, comboBox2.Text.Trim()));
                    }
                    else
                    {
                        MessageBox.Show("销售联系人未填写！");
                        creatSuccessed = false;
                    }
                    //application engineer
                    if (comboBox3.Text != "")
                    {
                        attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)20003, comboBox3.Text.Trim()));
                    }
                    else
                    {
                        MessageBox.Show("Demo工程师未填写！");
                        creatSuccessed = false;
                    }
                    //customer
                    if (comboBox4.Text != "")
                    {
                        attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)1062, comboBox4.Text.Trim()));
                    }
                    else
                    {
                        MessageBox.Show("客户名称未填写！");
                        creatSuccessed = false;
                    }
                    //province
                    if (comboBox5.Text != "")
                    {
                        attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)20001, comboBox5.Text.Trim()));
                    }
                    else
                    {
                        MessageBox.Show("客户所在省份未填写！");
                        creatSuccessed = false;
                    }
                    //city
                    if (comboBox6.Text != "")
                    {
                        attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)20002, comboBox6.Text.Trim()));
                    }
                    else
                    {
                        MessageBox.Show("客户所在城市未填写！");
                        creatSuccessed = false;
                    }
                    //customer Contact person
                    attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)20053, textBox1.Text.Trim()));
                    //customer phone number
                    attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)20054, textBox2.Text.Trim()));
                    //customer address
                    attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)20055, textBox3.Text.Trim()));
                    //customer person count
                    attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)20056, textBox4.Text.Trim()));
                    //cmm info
                    if (comboBox7.Text != "")
                    {
                        attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)20051, comboBox7.Text.Trim()));
                    }
                    else
                    {
                        MessageBox.Show("三坐标信息未填写！");
                        creatSuccessed = false;
                    }
                    //sensor info
                    if (comboBox8.Text != "")
                    {
                        attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)20052, comboBox8.Text.Trim()));
                    }
                    else
                    {
                        MessageBox.Show("传感器信息未填写！");
                        creatSuccessed = false;
                    }
                    //time K4
                    attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)4, DateTime.Now));
                    //check report
                    if (checkBox1.Checked)
                    {
                        attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)20048, "已完成"));
                    }
                    //check handover
                    attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)20029, textBox5.Text));
                    //part number
                    attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)20041, 1));
                    //process status
                    if (checkBox2.Checked)
                    {
                        attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)20027, "技术交接完成"));
                    }
                    else
                    {
                        attributes.Add(new Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute((ushort)20027, "创建完成"));
                    }
                    var Measurement = new DataMeasurement
                    {
                        Uuid = Guid.NewGuid(),
                        PartUuid = Part.Uuid,
                        Time = DateTime.Now,
                        Attributes = attributes.ToArray()
                    };
                    if (creatSuccessed)
                    {
                        //Create measurement on the server
                        await _RestDataServiceClient.CreateMeasurementValues(new[] { Measurement });
                    }
                    else
                    {
                        creatSuccessed = false;
                    }
                }
                else
                {
                    creatSuccessed = false;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                creatSuccessed = false;
            }
            finally
            {
                if (creatSuccessed)
                {
                    DirectoryInfo path_exe = new DirectoryInfo(Application.StartupPath); //exe目录
                    RealAction(path_exe + @"\PiWebInterface.exe", " 主界面");
                    //写入完成
                    DialogResult dr = MessageBox.Show("创建Demo完成,是否继续创建Demo?", "创建Demo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        //询问是否清空界面内容
                        comboBox2.Text = "";
                        comboBox3.Text = "";
                        comboBox4.Text = "";
                        comboBox5.Text = "";
                        comboBox6.Text = "";
                        comboBox7.Text = "";
                        comboBox8.Text = "";
                        textBox1.Text = "";
                        textBox2.Text = "";
                        textBox3.Text = "";
                        textBox4.Text = "";
                        checkBox1.Checked = false;
                        checkBox2.Checked = false;
                        textBox5.Text = "";
                    }
                    else
                    {
                        Application.Exit();
                    }
                }
            }

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                if (textBox5.Text == "")
                {
                    textBox5.Text = "已完成";
                }
                //textBox5.Enabled = false;
            }
            else
            {
                textBox5.Enabled = true;
            }
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar)) && e.KeyChar != (char)13 && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        #region run exe
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

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (textBox5.Text != "")
            {
                checkBox2.CheckState = CheckState.Checked;
            }
        }
    }
}
