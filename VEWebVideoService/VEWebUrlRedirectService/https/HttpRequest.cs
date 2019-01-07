using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using VEWebUrlRedirectService.OpensslWrapper;
/*
* Notice
* This code is designed for vesystem,all right reserved. --maxwell.z
* 2018.12
* ref:https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.10
*/

namespace VEWebUrlRedirectService.https
{
    class HttpRequest {

        #region header_strings
        string[] strHeaderCmds = {
            "GET","POST","UPDATE"
        };
        string []strHeaderItems = {
            "Host","Connection","Cache-Control","Upgrade-Insecure-Requests",
            "User-Agent","Accept","Accept-Encoding","Accept-Language","Cookie","Referer"
        };
        #endregion
        public HttpRequest(string strRequest)
        {
            //mCurStream = stream;
            ParseString(strRequest);
            
        }

        private void Assert(string []array, string item)
        {
            bool bin = false;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].StartsWith(item))
                {
                    bin = true;
                    break;
                }
            }

            if (!bin)
                throw new Exception("unknown http header tag!");
        }

        private void ParseString(string strRequest)
        {
            mHeaders = new Dictionary<string, string>();
            System.Diagnostics.Debug.Write(strRequest);
            //cmd
           
            string[] lines = strRequest.Split('\n');
            string strCmdLine = lines[0].Remove(lines[0].Length - 1); //remove '\r'
            string []cmdFields = strCmdLine.Split(' ');
            this.mCmd = cmdFields[0];
            this.mRawURL = cmdFields[1];
            this.mProtocal = cmdFields[2];
            Assert(this.strHeaderCmds, mCmd);

            //header item
            for (int i = 0; i < strHeaderItems.Length; i++)
            {
                string strHeaderItem = strHeaderItems[i];
                if (strHeaderItem.Length <= 0)
                    continue;
                for (int j = 1; j < lines.Length; j++)
                {
                    string strLine = lines[j];
                    if(strLine.StartsWith(strHeaderItem))
                    {
                        mHeaders.Add(strHeaderItem, strLine.Substring(strHeaderItem.Length +1,strLine.Length - strHeaderItem.Length -2).Trim());
                        break;
                    }
                }
            }

            //host
            string Line;
            if (mHeaders.TryGetValue("Host", out Line))
            {
                mHost = Line.Trim();
                mFullURL = mHost + mRawURL;
            }

            //conection
            if (mHeaders.TryGetValue("Connection", out Line))
            {

                if (Line != null || Line.Length <= 0)
                {
                    string strConnect = Line.Trim();
                    if (strConnect.ToLower().Equals("keep-alive"))
                        this.mIsKeepAlive = true;
                    else
                        this.mIsKeepAlive = false;
                    if (strConnect.ToLower().Equals("close"))
                        this.mNeedClose = true;
                }
            }

            mHeaders.TryGetValue("Cookie", out mCookie);
            mHeaders.TryGetValue("Referer", out mReferer);

        }
     
        public bool   mIsKeepAlive=false;
        public bool   mNeedClose = false;
        public string mCookie;
        public string mCmd;
        public string mHost;
        public string mRawURL;
        public string mFullURL;
        public string mProtocal;
        public string mReferer;
        public Dictionary<string, string> mHeaders;
        
    }
}
