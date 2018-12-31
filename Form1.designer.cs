using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using DrawBehindDesktopIcons;

namespace PenduleSNCF
{
    partial class Form1
    {

        private ElementHost ctrlHost;
        private PenduleWPF.UserControl1 ucpendule;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        /// 

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOACTIVATE = 0x0010;
        const int WM_WINDOWPOSCHANGING = 0x0046;


        public void SetBottom()
        {
            SetWindowPos(this.Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResizeEnd += Form1_OnResize;
            this.SetStyle(
             ControlStyles.ResizeRedraw |
             ControlStyles.OptimizedDoubleBuffer |
             ControlStyles.AllPaintingInWmPaint |
             ControlStyles.SupportsTransparentBackColor |
             ControlStyles.UserPaint, true);
            this.BackColor = System.Drawing.Color.Transparent;
           // this.TransparencyKey = System.Drawing.Color.Magenta;
            this.StartPosition = FormStartPosition.Manual;
            this.MinimumSize = new System.Drawing.Size(450, 450);
            ctrlHost = new ElementHost();
            ctrlHost.Dock = DockStyle.None;
            ctrlHost.BackColor = System.Drawing.Color.Transparent;
            //VisualBrush bg = new VisualBrush();
            //bg.Opacity = 0.0;
            ucpendule = new PenduleWPF.UserControl1();
            //ucpendule.Background = bg;
            if (Properties.Settings.Default.top != 0 & Properties.Settings.Default.left != 0 & Properties.Settings.Default.height != 0 & Properties.Settings.Default.width != 0)
            {
                this.Top = Properties.Settings.Default.top;
                this.Left = Properties.Settings.Default.left;
                this.Height = Properties.Settings.Default.height;
                this.Width = Properties.Settings.Default.width;
                ctrlHost.Height = Properties.Settings.Default.height;
                ctrlHost.Width = Properties.Settings.Default.width;
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
                ctrlHost.Height = 450;
                ctrlHost.Width = 450;
                ucpendule.MinHeight = 450;
                ucpendule.MinWidth = 450;
                ucpendule.MaxHeight = 450;
                ucpendule.MaxWidth = 450;
            }
            if (Program.reglage)
            {
                this.Text = PenduleSNCF.Properties.Resources.String1;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
                this.ControlBox = true;
                this.Show();
            } else { 
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.ControlBox = false;
                this.Text = String.Empty;
                this.SendToBack();
            }
            ucpendule.InitializeComponent();
            ctrlHost.Child = ucpendule;
            ctrlHost.BackColorTransparent = true;

            ctrlHost.SendToBack();

            this.Controls.Add(ctrlHost);
            if (Program.reglage == false & workerw != IntPtr.Zero & timeout > 0)
            {
                W32.SetParent(this.Handle, workerw);
            }
            this.ResumeLayout(false);
        }
    }
}