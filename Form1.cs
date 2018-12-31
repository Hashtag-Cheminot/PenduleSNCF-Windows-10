
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using DrawBehindDesktopIcons;
using System.Runtime.InteropServices;
using static PenduleSNCF.Win32Functions;

namespace PenduleSNCF
{
    class Win32Functions
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWDEFAULT = 10;


        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWINFO
        {
            public uint cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;

            public WINDOWINFO(Boolean? filler) : this()
            {
                cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
            }

        }

        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(System.Drawing.Rectangle r)
                : this(r.Left, r.Top, r.Right, r.Bottom)
            {
            }

            public int X
            {
                get { return Left; }
                set { Right -= (Left - value); Left = value; }
            }

            public int Y
            {
                get { return Top; }
                set { Bottom -= (Top - value); Top = value; }
            }

            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
                set { X = value.X; Y = value.Y; }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator System.Drawing.Rectangle(RECT r)
            {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(System.Drawing.Rectangle r)
            {
                return new RECT(r);
            }

            public static bool operator ==(RECT r1, RECT r2)
            {
                return r1.Equals(r2);
            }

            public static bool operator !=(RECT r1, RECT r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(RECT r)
            {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is RECT)
                    return Equals((RECT)obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT2
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll", SetLastError = true)] public static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);
        public enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(HandleRef hWnd, [In, Out] ref RECT2 rect);


        public static bool IsForegroundFullScreen()
        {
            return IsForegroundFullScreen(null);
        }

        public static bool IsForegroundFullScreen(System.Windows.Forms.Screen screen)
        {
            if (screen == null)
            {
                screen = System.Windows.Forms.Screen.PrimaryScreen;
            }
            RECT2 rect = new RECT2();
            IntPtr hWnd = (IntPtr)GetForegroundWindow();
            GetWindowRect(new HandleRef(null, hWnd), ref rect);
            if (screen.Bounds.Width == (rect.right - rect.left) && screen.Bounds.Height == (rect.bottom - rect.top))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("User32.dll")]

        public static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
    }

    public partial class Form1 : Form
    {
        const string dsk = "Desktop";
        const string colour = "Color";
        private static bool first = true;


        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(',') ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");
            dllName = dllName.Replace(".", "_");
            if (dllName.EndsWith("_resources")) return null;
            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(GetType().Namespace + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
            byte[] bytes = (byte[])rm.GetObject(dllName);
            return System.Reflection.Assembly.Load(bytes);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            first = false;
            Restart();
        }

        public void Restart()
        {
            Application.ExitThread();
        }

        private int timeout = 10;
        public void SystemEvents_DisplaySettingsChanged(object sender, EventArgs f)
        {
            if (first)
            {
                first = false;
                if (Properties.Settings.Default.top != 0 & Properties.Settings.Default.left != 0 & Properties.Settings.Default.height != 0 & Properties.Settings.Default.width != 0)
                {
                    this.Top = Properties.Settings.Default.top;
                    this.Left = Properties.Settings.Default.left;
                    this.Height = Properties.Settings.Default.height;
                    this.Width = Properties.Settings.Default.width;
                    ctrlHost.Height = Properties.Settings.Default.height;
                    ctrlHost.Width = Properties.Settings.Default.width;
                    ucpendule.MinHeight = Properties.Settings.Default.height;
                    ucpendule.MinWidth = Properties.Settings.Default.width;
                    ucpendule.MaxHeight = Properties.Settings.Default.height;
                    ucpendule.MaxWidth = Properties.Settings.Default.width;
                }
                else
                {
                    this.Top = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 8.60);
                    if (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height > 959)
                    {
                        this.Left = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 1.518f);
                    }
                    else
                    {
                        if (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height < 800)
                        {
                            this.Left = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 2f);
                        }
                        else
                        {
                            this.Left = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 1.74f);
                        }
                    }
                    this.Height = 450;
                    this.Width = 450;
                }
                this.Invalidate(this.Bounds);
                this.Update();
            }
            else
            {
                this.Visible = false;
                Thread.Sleep(15000);
                Restart();
            }
        }

        private IntPtr workerw = IntPtr.Zero;
        public Form1()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(this.CurrentDomain_AssemblyResolve);
            IntPtr progman = W32.FindWindow("Progman", null);
            IntPtr result = IntPtr.Zero;
            uint WM_SPAWN_WORKER = 0x052C;
            W32.SendMessageTimeout(progman,
                                   0x052C,
                                   new IntPtr(0),
                                   IntPtr.Zero,
                                   W32.SendMessageTimeoutFlags.SMTO_NORMAL,
                                   5000,
                                   out result);
            W32.EnumWindows(new W32.EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = W32.FindWindowEx(tophandle,
                                            IntPtr.Zero,
                                            "SHELLDLL_DefView",
                                            IntPtr.Zero);
                if (p != IntPtr.Zero)
                {
                    workerw = W32.FindWindowEx(IntPtr.Zero,
                                               tophandle,
                                               "WorkerW",
                                               IntPtr.Zero);
                }
                return true;
            }), IntPtr.Zero);
            W32.SendMessage(progman, WM_SPAWN_WORKER, 0x0000000D, (IntPtr)0);
            W32.SendMessage(progman, WM_SPAWN_WORKER, 0x0000000D, (IntPtr)1);
            while(workerw == IntPtr.Zero & timeout > 0)
            {
                Console.WriteLine("WorkerW null");
                Thread.Sleep(1);
                W32.EnumWindows(new W32.EnumWindowsProc((tophandle, topparamhandle) =>
                {
                    IntPtr p = W32.FindWindowEx(tophandle,
                                                IntPtr.Zero,
                                                "SHELLDLL_DefView",
                                                IntPtr.Zero);

                    if (p != IntPtr.Zero)
                    {
                        workerw = W32.FindWindowEx(IntPtr.Zero,
                                                   tophandle,
                                                   "WorkerW",
                                                   IntPtr.Zero);
                    }
                    return true;
                }), IntPtr.Zero);
                timeout -= 1;
            }
            InitializeComponent();
            if(timeout == 0)
            {
                SetBottom();
                CenterToParent();
            }
            Thread.Sleep(1);
        }

        private void Form1_OnResize(object sender, System.EventArgs e)
        {
            if (Program.reglage)
            {
                if (this.Size.Height != this.Size.Width)
                {
                    this.Size = new Size(this.Size.Width, this.Size.Width);
                }
                ctrlHost.Size = this.Size;
                ctrlHost.Height = this.Height;
                ctrlHost.Width = this.Width;
                ucpendule.MinHeight = this.Height;
                ucpendule.MinWidth = this.Width;
                ucpendule.MaxHeight = this.Height;
                ucpendule.MaxWidth = this.Width; 
            }
        }

        public void PreferenceChangedHandler(object sender, UserPreferenceChangingEventArgs e)
        {
            if (e.Category.ToString() == dsk || e.Category.ToString() == colour)
            {
                Restart();
            }
        }
        
        public const int SPI_SETDESKWALLPAPER = 20;
        public const int WM_SETTINGCHANGE = 0x001A;
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                // The WM_WINDOWPOSCHANGING message occurs when a window whose
                // size, position, or place in the Z order is about to change.
                case WM_WINDOWPOSCHANGING:
                    if (Program.reglage == false)
                    {
                        SetWindowPos(this.Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
                    }
                    break;

                case  WM_SETTINGCHANGE:
                    if (m.WParam.ToInt32() == SPI_SETDESKWALLPAPER)
                    {
                        Restart();
                        return;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (timeout == 0)
            {
                IntPtr desktop = Win32Functions.GetDC(IntPtr.Zero);
                using (Graphics g = Graphics.FromHdc(desktop))
                {
                    g.FillRectangle(Brushes.Transparent, 0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                }
                Win32Functions.ReleaseDC(IntPtr.Zero, desktop);
            }
            else
            {
                base.OnPaint(e);
            }
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Program.reglage)
            {
                base.OnPaintBackground(e);
            }
            else
            {
                while (IsForegroundFullScreen())
                {
                    Thread.Sleep(1000);
                }
                if (IsForegroundFullScreen() == false)
                {
                    Program.ToggleDesktopIcons();
                    foreach (Process p in Process.GetProcesses().Where(p => (p.MainWindowHandle != IntPtr.Zero & String.IsNullOrEmpty(p.MainWindowTitle) == false)))
                    {
                        try
                        {
                            var current = Process.GetCurrentProcess();
                            if (p.StartInfo.WindowStyle != ProcessWindowStyle.Minimized & p.StartInfo.WindowStyle != ProcessWindowStyle.Hidden & p.Id != current.Id)
                            {
                                WINDOWINFO info = new WINDOWINFO();
                                info.cbSize = (uint)Marshal.SizeOf(info);
                                GetWindowInfo(p.MainWindowHandle, ref info);
                                if ((((info.rcWindow.Location.X <= this.Location.X-300 | info.rcWindow.Location.X <= this.Location.X + this.Width) &
                                    (info.rcWindow.Location.Y <= this.Location.Y - 300 | info.rcWindow.Location.Y <= this.Location.Y + this.Height)) &
                                    (info.rcWindow.Location.X + info.rcWindow.Width >= this.Location.X - 300 & info.rcWindow.Location.Y + info.rcWindow.Height >= this.Location.Y - 300)) |
                                    p.StartInfo.WindowStyle == ProcessWindowStyle.Maximized)
                                {
                                    ShowWindow(p.MainWindowHandle, 6);
                                }
                            }
                        }
                        catch
                        { }
                    }
                    Thread.Sleep(600);
                    Size sz = Size;
                    Point destination = this.Location;
                    e.Graphics.CopyFromScreen(destination, new Point(0, 0), sz);
                    Program.ToggleDesktopIcons(); 
                }
            }
        }
    }
}