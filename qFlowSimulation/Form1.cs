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

namespace qFlowSimulation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public string PiWebServer = "http://10.202.120.59:8888/";
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
        List<Charateristic> charateristics;
        private void getChrNominal(string partName,string inspectionPlanName)
        {
            charateristics = new List<Charateristic>();
            Charateristic chr;
            Dictionary<string, object> attributesList = new Dictionary<string, object>();
            using (var httpClient = new HttpClient())
            {
                var requestUrl = string.Format("{0}/dataServiceRest/characteristics?partPath=/Programs/{1}/{2}", PiWebServer, partName, inspectionPlanName);//&requestedCharacteristicAttributes=22007

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
                            chr = new Charateristic();
                            foreach (var key in item.Keys)
                            {
                                if (key == "path")
                                {
                                    var subItem = item[key];
                                    chr.name= subItem.ToString().Split('/')[subItem.ToString().Split('/').Count()-2];
                                }
                                //if (key == "partUuid")
                                //{
                                //    var subItem = item[key];
                                //    partUuid = subItem.ToString();
                                //}
                                if (key == "attributes")
                                {
                                    var subItem = item[key];
                                    var ss = JsonConvert.SerializeObject(subItem, Formatting.Indented);
                                    attributesList = JsonConvert.DeserializeObject<Dictionary<string, object>>(ss);
                                    if (attributesList.ContainsKey("2101"))
                                    {
                                        chr.nominal =Convert.ToDouble(attributesList["2101"].ToString());
                                    }
                                    if (attributesList.ContainsKey("2112"))
                                    {
                                        chr.lowerTol = Convert.ToDouble(attributesList["2112"].ToString());
                                    }
                                    if (attributesList.ContainsKey("2113"))
                                    {
                                        chr.uppTol = Convert.ToDouble(attributesList["2113"].ToString());
                                    }

                                    //if (!attributesList.ContainsKey("22200")) //JobStatus
                                    //{
                                    //    log.Error("当前JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " 的任务信息 没有对应的测量程序名称");
                                    //    //未查询到该任务信息
                                    //    MessageBox.Show("当前JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " 的任务信息 没有对应的测量程序名称");
                                    //}
                                    //else
                                    //{
                                    //    if (attributesList["22200"].ToString() != inspectionPlanName)
                                    //    {
                                    //        log.Error("当前JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " 的任务信息对应的测量程序名称 " + attributesList["22200"] + " 与实际运行的测量程序 " + inspectionPlanName + " 不符");
                                    //        //未查询到该任务信息
                                    //        MessageBox.Show("当前JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " 的任务信息对应的测量程序名称 " + attributesList["22200"] + " 与实际运行的测量程序 " + inspectionPlanName + " 不符");
                                    //    }
                                    //}
                                }
                            }
                            charateristics.Add(chr);
                        }
                    }
                    else
                    {
                        //log.Error("未查询到JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " 的任务信息");
                        ////未查询到该任务信息
                        //MessageBox.Show("未查询到JobNumber=" + jobNumber + " &TaskNumber=" + taskNumber + " 的任务信息");
                    }

                }
            }
        }
        public void generateFakeData(List<Charateristic> chrs)
        {
            foreach(var chr in chrs)
            {
                Random rd = new Random(GetRandomSeed());
                
               
            }
        }
        //获取随机种子
        static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            getChrNominal("Blade_Shell_ZZC", "8.DrawingShell");
        }
    }
}
