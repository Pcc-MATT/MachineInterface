using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;
using System.Threading;

namespace PiWebInterface
{
    public partial class Form1 : Form
    {
        string[] args = null;
        public Form1(string[] args)
        {
            InitializeComponent();
            this.args = args;
        }
        //自定义一个类，用来保存句柄信息，在遍历的时候，随便也用空上句柄来获取些信息，呵呵 
        public struct WindowInfo
        {
            public IntPtr hWnd;
            public string szWindowName;
            public string szClassName;
        }
        private delegate bool WNDENUMPROC(IntPtr hWnd, int lParam);
        //用来遍历所有窗口 
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(WNDENUMPROC lpEnumFunc, int lParam);
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow", SetLastError = true)]
        private static extern void SetForegroundWindow(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        //获取窗口Text 
        [DllImport("user32.dll")]
        private static extern int GetWindowTextW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpString, int nMaxCount);

        //获取窗口类名 
        [DllImport("user32.dll")]
        private static extern int GetClassNameW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpString, int nMaxCount);
        public List<WindowInfo> GetAllDesktopWindows()
        {
            //用来保存窗口对象 列表
            List<WindowInfo> wndList = new List<WindowInfo>();

            //enum all desktop windows 
            EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                WindowInfo wnd = new WindowInfo();
                StringBuilder sb = new StringBuilder(256);

                //get hwnd 
                wnd.hWnd = hWnd;

                //get window name  
                GetWindowTextW(hWnd, sb, sb.Capacity);
                wnd.szWindowName = sb.ToString();

                //get window class 
                GetClassNameW(hWnd, sb, sb.Capacity);
                wnd.szClassName = sb.ToString();

                //add it into list 
                wndList.Add(wnd);
                return true;
            }, 0);

            return wndList;
        }
        [DllImport("user32")]
        private static extern int SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int y, int w, int h, int flag);

        private void button1_Click(object sender, EventArgs e)
        {
            //模拟鼠标
            List<WindowInfo> wndListT = new List<WindowInfo>();
            wndListT = GetAllDesktopWindows();
            WindowInfo wiT = wndListT.Find(n => n.szWindowName.Contains("PiWeb Monitor"));
            IntPtr hwndPhotoT = wiT.hWnd; //查找拍照程序的句柄【任务管理器中的应用程序名称】"OWST2"
            if (hwndPhotoT != IntPtr.Zero)
            {
                SetForegroundWindow(hwndPhotoT);//窗体置顶

                ShowWindow(hwndPhotoT, 3);//最大化窗体
                var sim2 = new InputSimulator();
                sim2.Keyboard
                    .KeyPress(VirtualKeyCode.F5);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //模拟鼠标
            List<WindowInfo> wndListT = new List<WindowInfo>();
            wndListT = GetAllDesktopWindows();
            WindowInfo wiT = wndListT.Find(n => n.szWindowName.Contains("质量流"));
            IntPtr hwndPhotoT = wiT.hWnd; //查找拍照程序的句柄【任务管理器中的应用程序名称】"OWST2"
            if (hwndPhotoT != IntPtr.Zero)
            {
                SetForegroundWindow(hwndPhotoT);//窗体置顶

                ShowWindow(hwndPhotoT, 3);//最大化窗体
                var sim2 = new InputSimulator();
                sim2.Keyboard
                    .KeyPress(VirtualKeyCode.F5);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //模拟鼠标
            List<WindowInfo> wndListT = new List<WindowInfo>();
            wndListT = GetAllDesktopWindows();
            WindowInfo wiT = wndListT.Find(n => n.szWindowName.Contains("测量员界面"));
            IntPtr hwndPhotoT = wiT.hWnd; //查找拍照程序的句柄【任务管理器中的应用程序名称】"OWST2"
            if (hwndPhotoT != IntPtr.Zero)
            {
                SetForegroundWindow(hwndPhotoT);//窗体置顶

                ShowWindow(hwndPhotoT, 3);//最大化窗体
                var sim2 = new InputSimulator();
                sim2.Keyboard
                    .KeyPress(VirtualKeyCode.F5);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (args.Count() == 2)
            {
                Thread.Sleep(int.Parse(args[1]));
            }
            //模拟鼠标
            List<WindowInfo> wndListT = new List<WindowInfo>();
            wndListT = GetAllDesktopWindows();
            WindowInfo wiT = wndListT.Find(n => n.szWindowName.Contains(args[0]));
            IntPtr hwndPhotoT = wiT.hWnd; //查找拍照程序的句柄【任务管理器中的应用程序名称】"OWST2"
            if (hwndPhotoT != IntPtr.Zero)
            {
                SetForegroundWindow(hwndPhotoT);//窗体置顶
                ShowWindow(hwndPhotoT, 3);//最大化窗体
                var sim2 = new InputSimulator();
                sim2.Keyboard
                    .KeyPress(VirtualKeyCode.F5);
            }
            Application.Exit();
        }
    }
}
