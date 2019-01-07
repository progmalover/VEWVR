using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TerminalWebVideoService
{
    public delegate bool MsgCallBack(int nCode);
    class WindowHook
    {
        protected List<int> filterMsgs;
        IntPtr hookWndHandle;
        MsgCallBack msgCallback;

        Win32.HookProc mHookCB;
        static Win32.HookProc win32Callback;
        public WindowHook()
        {
            filterMsgs = new List<int>();
            mHookCB = null;
            msgCallback = null;
            hookWndHandle = IntPtr.Zero;

            if (null == win32Callback)
            {
                win32Callback = new Win32.HookProc((nCode, wparam, lparam) =>
                {

                    if (nCode >= 0)
                    {
                        Win32.MSG msg = (Win32.MSG)Marshal.PtrToStructure(lparam, typeof(Win32.MSG));
                        System.Diagnostics.Debug.WriteLine("hook message: " + Convert.ToString(msg.message, 16));
                        foreach (var m in filterMsgs)
                        {
                            if (m == msg.message && msgCallback != null)
                            {
                                msgCallback(msg.wParam);
                                break;
                            }
                        }
                    }
                    return Win32.CallNextHookEx(null, nCode, wparam, lparam);
                });
            }
        }

        public bool UnHookWindow()
        {
            if (mHookCB == null)
                return false;
            int trdId = Win32.GetCurrentThreadId();
            Win32.SetWindowsHookEx(4, mHookCB, IntPtr.Zero, trdId);
            return true;
        }

        public bool HookWindow(IntPtr handle, int []msgs,MsgCallBack callback)
        {
            hookWndHandle = handle;
            msgCallback = callback;
            foreach ( var i in msgs)
            {
                filterMsgs.Add(i);
            }
            
            try
            {
               // int trdId =  Win32.GetWindowThreadProcessId(this.hookWndHandle,out procId);
                int trdId = Win32.GetCurrentThreadId();
                mHookCB= Win32.SetWindowsHookEx(4, win32Callback, IntPtr.Zero, trdId);
                if(mHookCB == null)
                {
                    int err = Win32.GetLastError();
                    string errstr = string.Format("hook message failed:{0}!", err);
                    Log.Logger.Instance.WriteLog(errstr);
                }
               
            }
            catch { }

            return true;
        }
       


    }
}
