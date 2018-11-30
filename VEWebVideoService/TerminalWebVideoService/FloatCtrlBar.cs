using System;
using System.Drawing;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using static TerminalWebVideoService.Win32;

namespace TerminalWebVideoService
{
    public partial class FloatCtrlBar : Form
    {

        private const int bar_width = 131;
        private const int bar_high = 45;

        IntPtr hWndOwner;
        IntPtr hBrowser;
        IntPtr hBProcess;
        string strBPath;
        System.Timers.Timer aTimer;
        bool bMoveOutStatus;
        Rect orgRect;
        WindowHook mHook;
        public FloatCtrlBar(IntPtr Owner,IntPtr Browser ,IntPtr browserProcess ,string browserPath)
        {
            InitializeComponent();
          
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.TopMost = true;

            hWndOwner = Owner;
            hBrowser =  Browser;
            hBProcess = browserProcess;
            strBPath = browserPath;

            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimer);
           
            bMoveOutStatus = true;
            mHook = new WindowHook();
        }

        ~FloatCtrlBar()
        {
            aTimer = null;
            mHook = null;
        }

        public void OnTimer(object source, System.Timers.ElapsedEventArgs e)
        {
            aTimer.Stop();
            try
            {
                this.Invoke(new Action(() => { MoveIn(); }));
            }
            catch { }
        }

        public void SetTimerParam(int intval = 2000)
        {
            //到时间的时候执行事件  
            aTimer.Interval = intval;
            aTimer.Start();
        }

        public void MoveIn()
        {
            if (bMoveOutStatus == false)
                return;
            Win32.MoveWindow(this.Handle, orgRect.Left, orgRect.Top, bar_width, 2, true);
            bMoveOutStatus = false;
        }

        public void MoveOut()
        {
            if (bMoveOutStatus == true)
                return;
            Win32.MoveWindow(this.Handle, orgRect.Left, orgRect.Top, orgRect.Right - orgRect.Left, bar_high, true);
            bMoveOutStatus = true;
        }

        private void FloatCtrlBar_Load(object sender, EventArgs e)
        {
            if (!Win32.IsWindow(this.Handle))
                return;

            Win32.SetParent(this.Handle, Win32.GetParent(hWndOwner));

            try
            {
                Icon appIcon = Icon.ExtractAssociatedIcon(strBPath);
                this.pictureBox1.Image = appIcon.ToBitmap();
            }
            catch { }

            Win32.SetWindowPos(this.Handle.ToInt32(), -1, 0, 0, 0, 0, 0x001 | 0x002 | 0x040);

            //guard thread for FloatBar exit.
            new Thread(() => {
                Win32.WaitForSingleObject(hBProcess, 0xffffffff);
                try
                {
                    this.Invoke(new Action(() =>
                    {
                        if (!Win32.IsWindow(this.Handle))
                            return;
                        this.Close();
                    }));
                }
                catch { }
            }).Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Win32.SendMessage(hBrowser, 0x0010, 0, 0);
        }

        private void btn_Mini_Click(object sender, EventArgs e)
        {
            Win32.ShowWindow(hBrowser,0);
        }

        private void btn_max_Click(object sender, EventArgs e)
        {
            Win32.ShowWindow(hBrowser, 3);
        }

        private void FloatCtrlBar_MouseEnter(object sender, EventArgs e)
        {
            MoveOut();
            aTimer.Stop();
        }

        private void FloatCtrlBar_MouseLeave(object sender, EventArgs e)
        {
            aTimer.Start();
        }

        private void SetOffRect(ref Rect rect ,int offx,int offy)
        {
            rect.Left += offx;
            rect.Top += offy;
            rect.Right += offx;
            rect.Bottom += offy;
        }

        private void SetBarPos(ref Rect rect)
        {
            Rect orect = new Rect();
            Rect prect = new Rect();

            IntPtr mainWnd = Win32.GetParent(hWndOwner);
            Win32.GetWindowRect(mainWnd, out orect);
            Win32.GetWindowRect(hWndOwner, out prect);

            SetOffRect(ref orect, 0, prect.Top - orect.Top);

            int offx = -orect.Left;
            int offy = -orect.Top;
            SetOffRect(ref orect, offx, offy);

            if ((Win32.GetWindowLong(mainWnd, -16).ToInt32() & 0x00C00000) == 0x00C00000L) //has caption
            {
                SetOffRect(ref orect, 0, 23);
                rect.Left = (orect.Right + orect.Left - bar_high) / 2;
            }
            else
                rect.Left = (orect.Right + orect.Left) / 2 + 120;
            rect.Top = orect.Top;
            rect.Right = rect.Left + Bounds.Width;
            rect.Bottom = rect.Top + Bounds.Height;
           
            Win32.MoveWindow(this.Handle, rect.Left, rect.Top, bar_width, bar_high, true);
        }

        private void FloatCtrlBar_OnShown(object sender, EventArgs e)
        {
            SetBarPos(ref orgRect);
            SetTimerParam(3000);

            int[] msgs = new int[] { 3,5, 0x0047, 0x5706be,0x04 };
            mHook.HookWindow(this.hWndOwner, msgs, new MsgCallBack((nCode) =>
            {
                this.Invoke(new Action(()=>
                {
                    SetBarPos(ref orgRect);
                }));
                return true;
            }));
        }

      
    }

}
