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
    class identify
    {
        int time_out = Form1.time_out;
        string title = "", artist = "", album = "", year = "";
        int index1;
        bool cover = false;
        public string Title
        {
            get { return title; }
        }
        public string Artist
        {
            get { return artist; }
        }
        public string Album
        {
            get { return album; }
        }
        public string Year
        {
            get { return year; }
        }
        public bool Cover
        {
            get { return cover; }
        }
        //以上为属性

        public identify(string path, int index)
        {
            index1 = index;
            switch (Form1.ESetting)
            {
                case "All":
                    {
                        searchByEnmfp(path);
                        break;
                    }
                case "None":
                    {
                        string fileN = path.Substring(path.LastIndexOf("\\") + 1);
                        fileN = fileN.Remove(fileN.LastIndexOf("."));
                        getInfo(fileN);
                        break;
                    }
                case "EnOnly":
                    {
                        if (!Regex.IsMatch(path.Substring(path.LastIndexOf("\\") + 1), "[\u4e00-\u9fa5]")) searchByEnmfp(path);
                        else
                        {
                            string fileN = path.Substring(path.LastIndexOf("\\") + 1);
                            fileN = fileN.Remove(fileN.LastIndexOf("."));
                            getInfo(fileN);
                        }
                        break;
                    }
            }

        }
        private void searchByEnmfp(string path)
        {
            ENMFP zgh = new ENMFP(path);
            artist = zgh.Artist;
            title = zgh.Title;
            if (title == "" && artist == "")
            {
                string fileN = path.Substring(path.LastIndexOf("\\") + 1);
                fileN.Remove(fileN.LastIndexOf("."));
                getInfo(fileN);
            }
            else getInfo(artist + " " + title);
        }
        private void getInfo(string fileName)
        {
            string htmlData = (new GetHtml(Form1.time_out)).getHtml("http://www.xiami.com/search?key=" + ConvertToBianMa(fileName), false);
            if (htmlData.IndexOf("虾小米建议您") != -1 || (htmlData.IndexOf("所属专辑") == -1 && htmlData.IndexOf("精选集") != -1))
            {
                title = "#####";
                return;
            }
            htmlData = htmlData.Replace("\n", "");
            setBasicInfo(htmlData);
            if (title == "")
            {
                title = "#####";
                return;
            }
        }
        private bool getCover(string albumdata,int retry)
        {
            try
            {
                string imgUrl = Regex.Match(albumdata, "cover_lightbox.*?href=\"([^\"]*?)\"").Groups[1].Value;
                bool result=SavePhotoFromUrl(Application.StartupPath + "\\temp\\" + index1.ToString() + ".jpg", imgUrl, index1);
                if (!result && retry > 1) return getCover(albumdata, --retry);
                else return result;
            }
            catch
            {
                return false;
            }
        }
        public bool SavePhotoFromUrl(string FileName, string Url, int index2)
        {
            bool Value = false;
            WebResponse response = null;
            Stream stream = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Timeout = time_out * 1000;
                request.ReadWriteTimeout = time_out * 1000;
                response = request.GetResponse();
                stream = response.GetResponseStream();

                if (!response.ContentType.ToLower().StartsWith("text/"))
                {
                    Value = SaveBinaryFile(response, FileName, index2);

                }

            }
            catch (Exception err)
            {
                string aa = err.Message.ToString();
            }
            return Value;
        }
        /// <summary>
        /// Save a binary file to disk.
        /// </summary>
        /// <param name="response">The response used to save the file</param>
        // 将二进制文件保存到磁盘
        private static bool SaveBinaryFile(WebResponse response, string FileName, int index3)
        {
            bool Value = true;
            byte[] buffer = new byte[1024];

            try
            {
                if (File.Exists(FileName))
                    File.Delete(FileName);
                Stream outStream = System.IO.File.Create(FileName);
                Stream inStream = response.GetResponseStream();

                int l;
                do
                {
                    l = inStream.Read(buffer, 0, buffer.Length);
                    if (l > 0)
                        outStream.Write(buffer, 0, l);
                }
                while (l > 0);

                outStream.Close();
                inStream.Close();
            }
            catch
            {
                Value = false;
            }
            //if (md5_hash(Application.StartupPath + "\\temp\\" + index3.ToString() + ".jpg") == "FAED854A8D44FCDC17A2FB8AEED9BF59") Value = false;
            return Value;
        }

        private string ConvertToBianMa(string text)//将文字转成url中的编码
        {
            string resualt = HttpUtility.UrlEncode(text, System.Text.Encoding.GetEncoding("GB2312"));
            return resualt;
        }


        private string RemoveSpaceHtmlTag(string Input)
        {
            string input = Input;
            input = Regex.Replace(input, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"-->", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"<!--.*", "", RegexOptions.IgnoreCase);

            input = input.Replace("&#039;", "\'");
            input = Regex.Replace(input, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&(nbsp|#160);", " ", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"&#(\d+);", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"\((.[^\)]*)\)", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"（(.[^）]*)）", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"【(.[^】]*)】", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"\[(.[^\]]*)\]", "", RegexOptions.IgnoreCase);

            input = input.Replace("(", "");
            input = input.Replace("《", "");
            input = input.Replace("》", "");
            input = input.Replace(")", "");
            input = input.Replace("（", "");
            input = input.Replace("）", "");
            input = input.Replace("【", "");
            input = input.Replace("】", "");
            input = input.Replace("[", "");
            input = input.Replace("]", "");
            input = input.Replace("<", "");
            input = input.Replace(">", "");
            input = input.Replace("\r\n", "");
            //input = HttpContext.Current.Server.HtmlEncode(input).Trim();
            input = Regex.Replace(input.Trim(), "\\s+", " ");
            //if (input.LastIndexOf(".") != -1)input= input.Remove(input.LastIndexOf("."));
            //if (input.LastIndexOf("，") != -1) input = input.Remove(input.LastIndexOf("，"));
            //if (input.LastIndexOf(",") != -1) input = input.Remove(input.LastIndexOf(","));
            return input;
        }

        private void setBasicInfo(string htmlData)
        {
            try
            {
                //string ZGH = htmldata;
                //ZGH = Regex.Match(ZGH, "<td class=\"song_name\">\\s+?<a.*?>(.*?)</a>").Groups[1].Value;
                //return RemoveSpaceHtmlTag(ZGH);
                int i = 0;
                string block = getP(htmlData, i);
                while (block != "")
                {
                    title = Regex.Match(block, "<td class=\"song_name\">\\s+?<a.*?>(.*?)</a>").Groups[1].Value;
                    artist = Regex.Match(block, "<td class=\"song_artist\">\\s+?<a[^>]*?>\\s+?<b class=\"key_red\">(.*?)</a>\\s+?</td>\\s+?<td class=\"song_album\">\\s+?<a.*?>(.*?)</a").Groups[1].Value;
                    album = Regex.Match(block, "<td class=\"song_artist\">\\s+?<a[^>]*?>\\s+?<b class=\"key_red\">(.*?)</a>\\s+?</td>\\s+?<td class=\"song_album\">\\s+?<a.*?>(.*?)</a").Groups[2].Value;
                    title = RemoveSpaceHtmlTag(title);
                    artist = RemoveSpaceHtmlTag(artist);
                    album = RemoveSpaceHtmlTag(album);

                    if (album.IndexOf("精选") != -1) album = "";
                    if (album.IndexOf("演唱会") != -1) album = "";
                    if (album.IndexOf("格莱美") != -1) album = "";
                    if (album.IndexOf("葛莱美") != -1) album = "";
                    if (album.IndexOf("单曲") != -1) album = "";
                    if (album.IndexOf("金曲") != -1) album = "";
                    if (album.IndexOf("新歌") != -1) album = "";
                    if (album.IndexOf("新曲") != -1) album = "";
                    if (album.IndexOf("热搜") != -1) album = "";
                    if (album.IndexOf("榜单") != -1) album = "";
                    if (album.IndexOf("live") != -1) album = "";
                    if (album.IndexOf("Live") != -1) album = "";
                    if (album.IndexOf("Grammy") != -1) album = "";
                    if (album.IndexOf("grammy") != -1) album = "";

                    if (title != "" && artist != "" && album != "") break;
                    block = getP(htmlData, ++i);
                }
                if (title == "" || artist == "" || album == "")//如果找不到红字匹配，就找非红字的
                {
                    i = 0;
                    block = getP(htmlData, i);
                    while (block != "")
                    {
                        title = Regex.Match(block, "<td class=\"song_name\">\\s+?<a.*?>(.*?)</a>").Groups[1].Value;
                        artist = Regex.Match(block, "<td class=\"song_artist\">(.*?)</a>\\s+?</td>\\s+?<td class=\"song_album\">\\s+?<a.*?>(.*?)</a").Groups[1].Value;
                        album = Regex.Match(block, "<td class=\"song_artist\">(.*?)</a>\\s+?</td>\\s+?<td class=\"song_album\">\\s+?<a.*?>(.*?)</a").Groups[2].Value;
                        title = RemoveSpaceHtmlTag(title);
                        artist = RemoveSpaceHtmlTag(artist);
                        album = RemoveSpaceHtmlTag(album);
                        if (title != "" && artist != "" && album != "") break;
                        block = getP(htmlData, ++i);
                    }
                }
                string albumUrl = Regex.Match(block, "<td class=\"song_album\">\\s+?<a.*?href=\"(.*?)\".*?</a").Groups[1].Value;
                if (albumUrl == "" || albumUrl == null)
                {
                    cover = false;
                    return;
                }
                string albumData = (new GetHtml(Form1.time_out)).getHtml(albumUrl, false);
                albumData = albumData.Replace("\n", "");
                year = getYear(albumData);
                cover = getCover(albumData,3);
            }
            catch
            {
                //MessageBox.Show("  %>_<% \n" + ex.Message + "\n" + htmldata, "!!!!!!哇去!!!!!!",
                //MessageBoxButtons.OK, MessageBoxIcon.Error); 
                title = "";
            }
        }
    

        string getYear(string htmldata)
        {
            try
            {
                string ZGH = htmldata;
                ZGH = Regex.Match(ZGH, "发行时间：(.*?)年").Groups[1].Value;
                return RemoveSpaceHtmlTag(ZGH);
            }
            catch
            {
                //MessageBox.Show("  %>_<% \n" + ex.Message+"\n"+htmldata, "!!!!!!哇去!!!!!!",
                //MessageBoxButtons.OK, MessageBoxIcon.Error); 
                return "";
            }
        }
        public static string md5_hash(string path)
        {
            try
            {
                FileStream get_file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                System.Security.Cryptography.MD5CryptoServiceProvider get_md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] hash_byte = get_md5.ComputeHash(get_file);
                string resule = System.BitConverter.ToString(hash_byte);
                resule = resule.Replace("-", "");
                get_file.Close();
                return resule;
            }
            catch (Exception e)
            {

                return e.ToString();
            }
        }
        string getP(string data, int index)
        {
            string a = "", b = data;
            for (int i = 0; i <= index; i++)
            {
                try
                {
                    int s = b.IndexOf("<td class=\"song_act\">");
                    a = b.Substring(0, s);
                    b = b.Substring(s + 21);
                    a = Regex.Replace(a, "<td class=\"song_name\">\\s+?<a.*?该艺人演唱的其他版本\"></a>", "<td class=\"song_name\">");
                }
                catch
                {
                    return "";
                }
            }
            return a;
        }
    }
}
