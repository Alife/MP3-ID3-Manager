using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ID3;
using ID3.ID3v2Frames.BinaryFrames;
using System.IO;
using ID3.ID3v2Frames.StreamFrames;
using System.Collections;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net;
using System.Web;

namespace 歌曲标签自动整理
{
    class GetHtml
    {
        int time_out;
        public GetHtml(int t)
        {
            time_out = t;
        }
        public string getHtml(string url, bool showError)//url是要访问的网站地址，charSet是目标网页的编码，如果传入的是null或者""，那就自动分析网页的编码 
        {
            HttpWebRequest myRequest;
            HttpWebResponse myResp = null;
            System.IO.StreamReader myReader = null;

            myRequest = (HttpWebRequest)WebRequest.Create(url);
            myRequest.ReadWriteTimeout = time_out * 1000;
            myRequest.Timeout = time_out * 1000;

            myRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";
            //myRequest.Headers.Add("Cookie", cookie);
            myRequest.KeepAlive = true;


            int chongshi = 0;//重试次数
            Exception er = null;
            while (chongshi != 2)
            {
                try
                {
                    //WebClient myWebClient = new WebClient();
                    //byte[] myDataBuffer = myWebClient.DownloadData(url);
                    //return Encoding.Default.GetString(myDataBuffer);

                    myResp = (HttpWebResponse)myRequest.GetResponse();
                    //curCookie = myResp.Headers["Set-Cookie"];
                    //curCookie = curCookie.Substring(0, curCookie.IndexOf(";"));
                    myReader = new System.IO.StreamReader(myResp.GetResponseStream(), Encoding.UTF8, true);
                    string _s = myReader.ReadToEnd();
                    return _s;
                }
                catch (Exception ex)
                {
                    er = ex;
                    chongshi++;
                    Thread.Sleep(1000);
                    continue;
                }
            }
            if (showError)
            {
                MessageBox.Show("访问网络失败，请检查网络！  %>_<% \n", "!!!!!!哇去!!!!!!",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return "";
        }
    }
}
