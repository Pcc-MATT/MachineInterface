using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PiWebTest
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Depositor huangfeihong = new BeiJingDepositor("黄飞鸿", 3000);
            Depositor fangshiyu = new BeiJingDepositor("方式与", 1200);
            Depositor hongxiguan = new BeiJingDepositor("洪熙官", 2500);

            BankMessageSystem beijingBank = new BejingBankMessageSystem();
            beijingBank.Add(huangfeihong);
            beijingBank.Add(fangshiyu);
            beijingBank.Add(hongxiguan);

            huangfeihong.GetMoney(100);
            beijingBank.Notify();

            huangfeihong.GetMoney(200);
            fangshiyu.GetMoney(200);
            beijingBank.Notify();

            huangfeihong.GetMoney(320);
            fangshiyu.GetMoney(425);
            hongxiguan.GetMoney(332);
            beijingBank.Notify();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DelectDir( @"C:\ZJQ\qFlow\测量程序 2\测量程序 2");
        }
        /// <summary>
        /// 删除文件夹以及文件
        /// </summary>
        /// <param name="directoryPath"> 文件夹路径 </param>
        /// <param name="fileName"> 文件名称 </param>
        public static void DelectDir(string srcPath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)            //判断是否文件夹
                    {
                       
                        DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                        Recurse(subdir);
                        subdir.Delete(true);  //删除子目录和文件
                    }
                    else
                    {
                        File.SetAttributes(i.FullName, FileAttributes.Normal);
                        //if (i.Attributes != FileAttributes.Normal)
                        //    i.Attributes = FileAttributes.Normal;
                        //如果 使用了 streamreader 在删除前 必须先关闭流 ，否则无法删除 sr.close();
                        File.Delete(i.FullName);      //删除指定文件
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public static void CopyFile(string srcPath,string aimPath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)            //判断是否文件夹
                    {

                        DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                        Recurse(subdir);
                        
                    }
                    else
                    {
                        File.SetAttributes(i.FullName, FileAttributes.Normal);
                        //if (i.Attributes != FileAttributes.Normal)
                        //    i.Attributes = FileAttributes.Normal;
                        //如果 使用了 streamreader 在删除前 必须先关闭流 ，否则无法删除 sr.close();
                        File.Delete(i.FullName);      //删除指定文件
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public static void Recurse(DirectoryInfo directory)
        {
            foreach (FileInfo fi in directory.GetFiles())
            {
                fi.IsReadOnly = false; // or true
            }
            foreach (DirectoryInfo subdir in directory.GetDirectories())
            {
                Recurse(subdir);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            File.Copy(@"C:\ZJQ\qFlow\测量程序 2\测量程序 2\.git\objects\0a\fb4d55b6f4c556a266e79feed9262a22193c48", @"C:\ZJQ\qFlow\测量程序 2\fb4d55b6f4c556a266e79feed9262a22193c48");
        }
    }
}
