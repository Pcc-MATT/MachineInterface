using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        List<string> ownerList=new List<string>();
        List<string> salesList = new List<string>();
        List<string> demoEngineerList = new List<string>();
        List<string> customerList = new List<string>();
        List<string> cmmList = new List<string>();
        List<string> sensorList = new List<string>();
        private async void Form1_Load(object sender, EventArgs e)
        {
            //读取配置
            DirectoryInfo path_exe = new DirectoryInfo(Application.StartupPath); //exe目录
            String path = path_exe.Parent.FullName; //上级的目录
            string filepath = path + @"\Relative_files\config.json";
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
            //初始化绑定默认关键词
            List<string> listOnit = new List<string>();
            //将数据项添加到listOnit中
            for (int i = 0; i < cb.Items.Count; i++)
            {
                listOnit.Add(cb.Items[i].ToString());
            }
            return listOnit;
        }
        //模糊查询Combobox
        public void selectCombobox(ComboBox cb, List<string> listOnit)
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
        private List<string> listCombobox1;//Combobox的最初Item项
        private List<string> listCombobox2;//Combobox的最初Item项
        private List<string> listCombobox3;//Combobox的最初Item项
        private List<string> listCombobox4;//Combobox的最初Item项
        private List<string> listCombobox7;//Combobox的最初Item项
        private List<string> listCombobox8;//Combobox的最初Item项
        private void comboBox2_TextUpdate(object sender, EventArgs e)
        {
            selectCombobox(comboBox2, listCombobox2);
        }

        private void comboBox1_TextUpdate(object sender, EventArgs e)
        {
            selectCombobox(comboBox1, listCombobox1);
        }

        private void comboBox3_TextUpdate(object sender, EventArgs e)
        {
            selectCombobox(comboBox3, listCombobox3);
        }

        private void comboBox4_TextUpdate(object sender, EventArgs e)
        {
            selectCombobox(comboBox4, listCombobox4);
        }

        private void comboBox7_TextUpdate(object sender, EventArgs e)
        {
            selectCombobox(comboBox7, listCombobox7);
        }

        private void comboBox8_TextUpdate(object sender, EventArgs e)
        {
            selectCombobox(comboBox8, listCombobox8);
        }
        #endregion

        Dictionary<string, object> PiWebHost_Data;
        private bool readJsonPara(string filePath)
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("GB2312"));
                Dictionary<string, object> valuePairs = new Dictionary<string, object>();
                valuePairs = JsonConvert.DeserializeObject<Dictionary<string, object>>(sr.ReadToEnd());
                sr.Close();
                var contentPut = JsonConvert.SerializeObject(valuePairs, Formatting.Indented);
                var ss3 = JsonConvert.SerializeObject(valuePairs["PiWebHost"], Formatting.Indented);
                PiWebHost_Data = JsonConvert.DeserializeObject<Dictionary<string, object>>(ss3);
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
                pictureBox5.Image = Properties.Resources.删除;
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
                foreach(var mes in _Measurements)
                {
                    if (mes.GetAttributeValue((ushort)20028) != null)
                    {
                        if (ownerList.FindAll(n => n == mes.GetAttributeValue((ushort)20028)).Count() == 0)
                        {
                            ownerList.Add(mes.GetAttributeValue((ushort)20028));
                            comboBox1.Items.Add(mes.GetAttributeValue((ushort)20028));
                        }
                    }
                   if (mes.GetAttributeValue((ushort)20004)!= null)
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
    }
}
