﻿using System;
using System.Net;
using System.IO;
using System.Windows.Forms;

namespace 歌曲标签自动整理
{
    public class GatherPic
    {

        private string savePath;
        private string getUrl;
        private WebBrowser wb;
        private int iImgCount;

        //初始化参数
        public GatherPic(string sWebUrl, string sSavePath)
        {
            this.getUrl = sWebUrl;
            this.savePath = sSavePath;
        }

        //开始采集
        public bool start()
        {

            if (getUrl.Trim().Equals(""))
            {
                MessageBox.Show("哪来的虾米连网址都没输！");
                return false;
            }

            this.wb = new WebBrowser();
            this.wb.Navigate(getUrl);
            //委托事件
            this.wb.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(DocumentCompleted);
            return true;
        }

        //WebBrowser.DocumentCompleted委托事件
        private void DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //页面里框架iframe加载完成不掉用SearchImgList()
            if (e.Url != wb.Document.Url) return;
            SearchImgList();
        }

        //检查出所有图片并采集到本地
        public void SearchImgList()
        {
            string sImgUrl;
            //取得所有图片地址
            HtmlElementCollection elemColl = this.wb.Document.GetElementsByTagName("img");
            this.iImgCount = elemColl.Count;
            foreach (HtmlElement elem in elemColl)
            {
                sImgUrl = elem.GetAttribute("src");
                //调用保存远程图片函数
                SaveImageFromWeb(sImgUrl, this.savePath);
            }
        }
        //保存远程图片函数
        public int SaveImageFromWeb(string imgUrl, string path)
        {
            string imgName = imgUrl.ToString().Substring(imgUrl.ToString().LastIndexOf("/") + 1);
            path = path + "\\" + imgName;
            string defaultType = ".jpg";
            string[] imgTypes = new string[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            string imgType = imgUrl.ToString().Substring(imgUrl.ToString().LastIndexOf("."));
            foreach (string it in imgTypes)
            {
                if (imgType.ToLower().Equals(it))
                    break;
                if (it.Equals(".bmp"))
                    imgType = defaultType;
            }
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(imgUrl);
                request.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; Natas.Robot)";
                request.Timeout = 10000;
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                if (response.ContentType.ToLower().StartsWith("image/"))
                {
                    byte[] arrayByte = new byte[1024];
                    int imgLong = (int)response.ContentLength;
                    int l = 0;
                    // CreateDirectory(path);
                    FileStream fso = new FileStream(path, FileMode.Create);
                    while (l < imgLong)
                    {
                        int i = stream.Read(arrayByte, 0, 1024);
                        fso.Write(arrayByte, 0, i);
                        l += i;
                    }
                    fso.Close();
                    stream.Close();
                    response.Close();
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch (WebException)
            {
                return 0;
            }
            catch (UriFormatException)
            {
                return 0;
            }
        }

    }
}

//-----------------调用代码--------------------

//GatherPic gatherpic = new GatherPic(“http://www.baidu.com”,"C:\test");

//请确保c:\下存在test路径

//gatherpic.start()