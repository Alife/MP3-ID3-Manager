using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Web;

namespace 歌曲标签自动整理
{
    public class ENMFP
    {
        string title="", artist="";
        public string Title
        {
            get { return title; }
        }
        public string Artist
        {
            get { return artist; }
        }
        
        //构造函数
        public ENMFP(string path)
        {
            string json = getJson(path);
            string response = "";
            if (json.StartsWith("[") && json.IndexOf("error") == -1)
            {
                response = post(json);
            }
            if (response != "" && response.IndexOf("Success") != -1)
            {
                title = getValue("title", response);
                artist = getValue("artist_name", response);
                if (title == "" || title == "Encoded") title = artist = "";
            }
        }

        private string getJson(string fileName)
        {
            Process process = new Process();
            process.StartInfo.FileName = "codegen.exe";
            process.StartInfo.Arguments = "\"" + fileName + "\"";

            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.PriorityClass = ProcessPriorityClass.BelowNormal;
            

            using (StreamReader streamReader = process.StandardOutput)
            {
                return streamReader.ReadToEnd();
            }
        }
        private string post(string json)
        {
            string ret = string.Empty;
            try
            {
                Encoding dataEncode = Encoding.UTF8;
                byte[] byteArray = dataEncode.GetBytes(json); //转化
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri("http://developer.echonest.com/api/v4/song/identify?api_key=DKQZ70LEJEXLOXHI2"));
                webReq.Method = "POST";
                webReq.ContentType = "application/octet-stream";

                webReq.ContentLength = byteArray.Length;
                Stream newStream = webReq.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);//写入参数
                newStream.Close();
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                ret = sr.ReadToEnd();
                sr.Close();
                response.Close();
                newStream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return ret;
        }
        string getValue(string name,string json)
        {
            return Regex.Match(json, "\""+name+"\": \"([^\"]*)?\",").Groups[1].Value;
        }
    }
}
