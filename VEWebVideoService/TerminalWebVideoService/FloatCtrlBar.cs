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
            if (mHook != null)
                mHook.UnHookWindow();
            if (aTimer != null)
                aTimer.Close();
                //aTimer.Close();
                
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
                    this.Invoke(new System.Action(() =>
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
           //// aTimer.Stop();
        }

        private bool PtInRect(Win32.POINT pt, Win32.Rect rct)
        {
            if(rct.Left <= pt.X && rct.Right >= pt.X && rct.Top <= pt.Y && rct.Bottom >= pt.Y )
            return true;
            return false;
        }

        private bool AdjustMoveIn()
        {
            ///aTimer.Start();
            Win32.POINT pt = new Win32.POINT();
            Win32.GetCursorPos(out pt);
            Rect orect = new Rect(0, 0, 0, 0);
            Win32.GetWindowRect(this.Handle, out orect);
            if (!PtInRect(pt, orect))
                MoveIn();
            return true;
        }
        private void FloatCtrlBar_MouseLeave(object sender, EventArgs e)
        {
            AdjustMoveIn();
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
            Rect orect = new Rect(0,0,0,0);
            Rect prect = new Rect(0,0,0,0);

            IntPtr mainWnd = Win32.GetParent(hWndOwner);
            Win32.GetWindowRect(mainWnd, out orect);
            Win32.GetWindowRect(hWndOwner, out prect);

            if (orect.IsEmpty())
                throw new Exception("Get View Main Wind Rect Empty!");

            if (prect.IsEmpty())
                throw new Exception("Get View Sub Wind Rect Empty!");
            //full screen mode
            if(prect.Top == orect.Top)
            {
                rect.Top = orect.Top;
                rect.Left = (orect.Right + orect.Left) / 2 + 120;
                rect.Bottom = rect.Top + Bounds.Height;
                rect.Right = rect.Left + Bounds.Width;

                if (rect.Top == this.Bounds.Top)
                    return;
                goto _end;
            }

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
            if (rect.Top == this.Bounds.Top)
                return;
           _end:
            Win32.MoveWindow(this.Handle, rect.Left, rect.Top, bar_width, bar_high, true);
        }
       
        private void InitBarPos()
        {
            while (true)
            {
                if ((Win32.GetWindowLong(this.Handle, -16).ToInt32() & 0x10000000L) != 0x10000000L)
                {
                    Thread.Sleep(200);
                    continue;
                }
                try
                {
                    SetBarPos(ref orgRect);
                    break;
                }
                catch(Exception e)
                {
                    Log.Logger.Instance.WriteLog(e.Message);
                    Thread.Sleep(200);
                }
               
            }

             
        }

        private void FloatCtrlBar_OnShown(object sender, EventArgs e)
        { 
            new Thread(() =>
                {
                    try
                    {
                        while (true)
                        {
                            this.Invoke(new Action(() =>
                            {
                                try
                                {
                                    InitBarPos();
                                    AdjustMoveIn();
                                }
                                catch { }
                            }
                              ));
                            Thread.Sleep(2000);
                        }

                    }
                    catch { }
                }
             ).Start();

#if false
            int[] msgs = new int[] { 3, 5, 0x0047, 0x5706be, 0x04, 0x20013862,0x34 };
            mHook.HookWindow(this.hWndOwner, msgs, new MsgCallBack((nCode) =>
            {
                try
                {
                    this.Invoke(new Action(() =>
                    {
                        try
                        {
                            SetBarPos(ref orgRect);
                        }
                        catch { }
                    }));
                }
                catch { }
                return true;
            }));
#endif
            //SetTimerParam(3000);

        }

        private void FloatCtrlBar_Move(object sender, EventArgs e)
        {
            
        }
    }

}
