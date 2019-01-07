using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
/*
* Notice
* This code is designed for vesystem,all right reserved. --maxwell.z
* 2018.12
*/
namespace VEWebUrlRedirectService
{
    public class LocalResourceManager
    {
        string SourceDirPath = "";
        public LocalResourceManager()
        {
            SourceDirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!SourceDirPath.EndsWith("/") && !SourceDirPath.EndsWith("\\"))
            {
                SourceDirPath += "\\";// for dos path
            }
        }
        //转换unicode路径
        string ConvertPath(string rawURL)
        {
            if (rawURL.IndexOf("%") < 0)
                return rawURL;

            return Uri.UnescapeDataString(rawURL);
        }

        public string UrlToLocalRef(string fullurl, string refurl)
        {
            string[] fullsecs = fullurl.Split('/');
            string[] refsecs = refurl.Split('/');
            int i = 0;
            while (true)
            {
                if (i >= refsecs.Length || i >= fullsecs.Length)
                    break;
                if (fullsecs[i].CompareTo(refsecs[i]) != 0)
                {
                    break;
                }
               
                i++;
            }

            if (i >= fullsecs.Length || i==0)
                return "/";

            string decrefurl = "";
            for (; i < fullsecs.Length; i++)
            {
                decrefurl += "/";
                decrefurl += fullsecs[i];
            }

            return decrefurl;
        }

        protected string PathMap(string rawURL)
        {
            string unescUrl = ConvertPath(rawURL);

            string beginToken = unescUrl[0].ToString();
            string strFullPath = SourceDirPath;
            //map to root file or insert source dir
            if (unescUrl == beginToken)
                strFullPath += "Source" + "//" + "index.html";
            else
                strFullPath += "Source" + unescUrl;

            return strFullPath;
        }

        public bool LocalFileExist(string strRefUrl)
        {
            string localPath = PathMap(strRefUrl);
            return File.Exists(localPath);
        }

        public FileStream GetFStream(string rawUrl, ref bool bBlockFile)
        {
            string[] raw_exts = new string[] { ".gif", ".bmp", ".png", ".jpg" };
            bBlockFile = false;
            string localPath = PathMap(rawUrl);
            try
            {
                if (File.Exists(localPath))
                {
                    string lcPath = localPath.ToLower();
                    foreach (var ext in raw_exts)
                    {
                        if (lcPath.Contains(ext))
                        {
                            bBlockFile = true;
                            break;
                        }
                    }

                    return File.Open(localPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                }

            }
            catch (Exception e)
            {
                Log.Logger.Instance.WriteException(e);
            }
            return null;
        }
    }
}
