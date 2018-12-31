
using System;
using DrawBehindDesktopIcons;
using Microsoft.Win32;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using RegistryUtils;
using System.ComponentModel;
using System.Configuration.Install;
using System.Collections;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace OffLine.Installer
{
    [RunInstaller(true)]
    public class InstallerClass : System.Configuration.Install.Installer
    {
        public InstallerClass()
          : base()
        {
            this.Committed += new InstallEventHandler(MyInstaller_Committed);
            this.Committing += new InstallEventHandler(MyInstaller_Committing);
        }
        
        private void MyInstaller_Committing(object sender, InstallEventArgs e)
        {
        }
        private void MyInstaller_Committed(object sender, InstallEventArgs e)
        {
            try
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName
                (Assembly.GetExecutingAssembly().Location));
                Process.Start(Path.GetDirectoryName(
                  Assembly.GetExecutingAssembly().Location) + "\\PenduleSNCF.exe");
            }
            catch
            {
            }
        }
        public override void Install(IDictionary savedState)
        {
            base.Install(savedState);
        }
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
        }
        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }
    }
}

namespace PenduleSNCF
{
    class Program
    {
        public static bool reglage = false;
        private static System.Windows.Forms.ContextMenu contextMenu1;
        private static System.Windows.Forms.MenuItem menuItem1;
        private static System.Windows.Forms.MenuItem menuItem2;
        private static System.Windows.Forms.MenuItem menuItem3;
        private static System.ComponentModel.IContainer component;
        private static NotifyIcon notifyIcon1;
        private static bool gooff = false;
        private static void CreateNotifyicon()
        {
            component = new System.ComponentModel.Container();
            contextMenu1 = new System.Windows.Forms.ContextMenu();
            menuItem1 = new System.Windows.Forms.MenuItem
            {
                Index = 0,
                Text = "Déplacer/Redimentionner"
            };
            menuItem1.Click += new System.EventHandler(MoveResize_Click);
            contextMenu1.MenuItems.AddRange(
                        new System.Windows.Forms.MenuItem[] { menuItem1 });
            if (Properties.Settings.Default.top != 0 & Properties.Settings.Default.left != 0 & Properties.Settings.Default.height != 0 & Properties.Settings.Default.width != 0)
            {
                menuItem2 = new System.Windows.Forms.MenuItem
                {
                    Index = 0,
                    Text = "Réinitialiser"
                };
                menuItem2.Click += new System.EventHandler(Reset_Click);
                contextMenu1.MenuItems.AddRange(
                            new System.Windows.Forms.MenuItem[] { menuItem2 });
            }
            menuItem3 = new System.Windows.Forms.MenuItem
            {
                Index = 0,
                Text = "Quitter"
            };
            menuItem3.Click += new System.EventHandler(Quit_Click);
            contextMenu1.MenuItems.AddRange(
                        new System.Windows.Forms.MenuItem[] { menuItem3 });
            notifyIcon1 = new System.Windows.Forms.NotifyIcon(component);
            notifyIcon1.Icon = Properties.Resources.icone;
            notifyIcon1.Text = @"Pendule SNCF";
            notifyIcon1.ContextMenu = contextMenu1;
            notifyIcon1.Visible = true;
        }

        private static void RemoveNotifyicon()
        {
            notifyIcon1.Visible = false;
            notifyIcon1.Dispose();

            Thread.Sleep(1);
        }
        public static void MoveResize_Click(object Sender, EventArgs e)
        {
            reglage = true;
            Application.Exit();
        }

        public static void Reset_Click(object Sender, EventArgs e)
        {
            Properties.Settings.Default.top = 0;
            Properties.Settings.Default.left = 0;
            Properties.Settings.Default.height = 0;
            Properties.Settings.Default.width = 0;
            Properties.Settings.Default.Save();
            Application.Exit();
        }

        public static void Quit_Click(object Sender, EventArgs e)
        {
            gooff = true;
            Application.Exit();
        }

        public static IntPtr GetDTHandle()
        {
            IntPtr hDesktopWin = Win32Functions.GetDesktopWindow();
            IntPtr hProgman = Win32Functions.FindWindow("Progman", "Program Manager");
            IntPtr hWorkerW = IntPtr.Zero;

            IntPtr hShellViewWin = Win32Functions.FindWindowEx(hProgman, IntPtr.Zero, "SHELLDLL_DefView", "");
            if (hShellViewWin == IntPtr.Zero)
            {
                do
                {
                    hWorkerW = Win32Functions.FindWindowEx(hDesktopWin, hWorkerW, "WorkerW", "");
                    hShellViewWin = Win32Functions.FindWindowEx(hWorkerW, IntPtr.Zero, "SHELLDLL_DefView", "");
                } while (hShellViewWin == IntPtr.Zero && hWorkerW != null);
            }
            return hShellViewWin;
        }

        public static void ToggleDesktopIcons()
        {
            Win32Functions.SendMessage(GetDTHandle(), 0x0111, (IntPtr)0x7402, (IntPtr)0);
        }
        private static byte[] SliceMe(byte[] source, int pos)
        {
            byte[] destfoo = new byte[source.Length - pos];
            Array.Copy(source, pos, destfoo, 0, destfoo.Length);
            return destfoo;
        }
        
        private static Task StartSTATask(Action action)
        {
            TaskCompletionSource<object> source = new TaskCompletionSource<object>();
            Thread thread = new Thread(() =>
            {
                try
                {
                    action();
                    source.SetResult(null);
                }
                catch (Exception ex)
                {
                    source.SetException(ex);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return source.Task;
        }

        private static Task<TResult> StartSTATask<TResult>(Func<TResult> function)
        {
            TaskCompletionSource<TResult> source = new TaskCompletionSource<TResult>();
            Thread thread = new Thread(() =>
            {
                try
                {
                    source.SetResult(function());
                }
                catch (Exception ex)
                {
                    source.SetException(ex);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return source.Task;
        }
        
        public static void Main(string[] args)
        {
            var current = Process.GetCurrentProcess();
            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            if (userName == "System")
            {
                current.Close();
            }
            Process.GetProcessesByName(current.ProcessName)
                .Where(t => t.Id != current.Id)
                .ToList()
                .ForEach(t => t.Kill());
            W32.SetDesktopWallpaper(W32.GetDesktopWallpaper());
            Task boucle = StartSTATask(() =>
            {
                RegistryMonitor monitor = new
                RegistryMonitor(RegistryHive.CurrentUser, "Control Panel\\Desktop");
                monitor.RegChanged += new EventHandler(OnRegChanged);
                void OnRegChanged(object sender, EventArgs f)
                {
                    Application.Restart();
                    Thread.Sleep(100000000);
                }
                Form1 form = new Form1();
                form.Load += new EventHandler((s, e) =>
                {
                    SystemEvents.UserPreferenceChanging += new UserPreferenceChangingEventHandler(form.PreferenceChangedHandler);
                    SystemEvents.DisplaySettingsChanging += new EventHandler(form.SystemEvents_DisplaySettingsChanged);
                    if (Properties.Settings.Default.top != 0 & Properties.Settings.Default.left != 0 & Properties.Settings.Default.height != 0 & Properties.Settings.Default.width != 0)
                    {
                        form.Top = Properties.Settings.Default.top;
                        form.Left = Properties.Settings.Default.left;
                        form.Height = Properties.Settings.Default.height;
                        form.Width = Properties.Settings.Default.width;
                    }
                    else
                    {
                        form.Top = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 8.60);
                        if (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height > 959)
                        {
                            form.Left = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 1.518f);
                        }
                        else
                        {
                            if (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height < 800)
                            {
                                form.Left = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 2f);
                            }
                            else
                            {
                                form.Left = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 1.74f);
                            }
                        }
                        form.Height = 450;
                        form.Width = 450;
                    }
                    monitor.Start();
                });
                CreateNotifyicon();
                //SystemEvents.UserPreferenceChanging += new UserPreferenceChangingEventHandler(form.PreferenceChangedHandler);
                //SystemEvents.DisplaySettingsChanging += new EventHandler(form.SystemEvents_DisplaySettingsChanged);
                Application.Run(form);
                SystemEvents.UserPreferenceChanging -= new UserPreferenceChangingEventHandler(form.PreferenceChangedHandler);
                SystemEvents.DisplaySettingsChanging -= new EventHandler(form.SystemEvents_DisplaySettingsChanged);
                form.Visible = false;
                form.Dispose();
                Thread.Sleep(100);
            });

            boucle.Wait();
            RemoveNotifyicon();
            if (gooff == false)
            {
                if (reglage)
                {
                    W32.SetDesktopWallpaper(W32.GetDesktopWallpaper());
                    if (reglage)
                    {
                        Task newboucle = StartSTATask(() =>
                        {
                            Form1 form = new Form1();
                            form.Load += new EventHandler((s, e) =>
                            {
                                if (Properties.Settings.Default.top != 0 & Properties.Settings.Default.left != 0 & Properties.Settings.Default.height != 0 & Properties.Settings.Default.width != 0)
                                {
                                    form.Top = Properties.Settings.Default.top;
                                    form.Left = Properties.Settings.Default.left;
                                    form.Height = Properties.Settings.Default.height;
                                    form.Width = Properties.Settings.Default.width;
                                }
                                else
                                {
                                    form.Top = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 8.60);
                                    if (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height > 959)
                                    {
                                        form.Left = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 1.518f);
                                    }
                                    else
                                    {
                                        if (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height < 800)
                                        {
                                            form.Left = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 2f);
                                        }
                                        else
                                        {
                                            form.Left = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 1.74f);
                                        }
                                    }
                                    form.Height = 450;
                                    form.Width = 450;
                                }
                            });
                            form.FormClosed += new FormClosedEventHandler((s, e) =>
                            {
                                Properties.Settings.Default.top = form.Top;
                                Properties.Settings.Default.left = form.Left;
                                Properties.Settings.Default.height = form.Height;
                                Properties.Settings.Default.width = form.Height;
                                Properties.Settings.Default.Save();
                            });
                            Application.Run(form);
                            form.Visible = false;
                            form.Dispose();
                            Thread.Sleep(100);
                        });
                        newboucle.Wait();
                        Thread.Sleep(1000);
                        Application.Restart();
                        Thread.Sleep(100000000);
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                    Application.Restart();
                    Thread.Sleep(100000000);
                }
            }
            else
            {
                W32.SetDesktopWallpaper(W32.GetDesktopWallpaper());
            }
        }
    }
}
