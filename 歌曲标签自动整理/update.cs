using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;

namespace 歌曲标签自动整理
{
    public partial class update : Form
    {
        static string upServer = "http://mp3meneger.sinaapp.com/";//必须以/结尾
        string downloadSite = Regex.Match((new GetHtml(Form1.time_out)).getHtml(upServer+"tip.php?action=getdownsite", true),"(http.*)").Groups[1].Value;
        Thread wWorkLine = null;
        public update()
        {
            InitializeComponent();
        }

        private void update_Load(object sender, EventArgs e)
        {

            string ggg = (new GetHtml(Form1.time_out)).getHtml(upServer+"tip.php?action=gxsm", true);
            if(ggg!="")textBox1.Lines= ggg.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
            string gxfs=(new GetHtml(Form1.time_out)).getHtml(upServer+"tip.php?action=gxfs", true);
            if (gxfs=="2")
            {
                wWorkLine = new Thread(new ThreadStart(startdownload));
                wWorkLine.Start();
            }
            else if(gxfs=="1")
            {
                if(MessageBox.Show("检测到新版本，请在页面中下载更新.", "有新版本", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk)==DialogResult.OK)
                {
                    string url = (new GetHtml(Form1.time_out)).getHtml(upServer+"tip.php?action=getdownpage", true);
                    url = Regex.Match(url, "(http.*)").Groups[1].Value;
                    if(url!="")Process.Start(url);

                    Environment.Exit(0);
                }
                this.Close();
            }
            
        }

        public void DownloadFile(string URL, string filename, System.Windows.Forms.ProgressBar prog)
        {
            float percent = 0;
            try
            {
                System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
                System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                long totalBytes = myrp.ContentLength;
                if (prog != null)
                {
                    prog.Maximum = (int)totalBytes;
                }
                System.IO.Stream st = myrp.GetResponseStream();
                System.IO.Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                long totalDownloadedByte = 0;
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    totalDownloadedByte = osize + totalDownloadedByte;
                    System.Windows.Forms.Application.DoEvents();
                    so.Write(by, 0, osize);
                    if (prog != null)
                    {
                        prog.Value = (int)totalDownloadedByte;
                    }
                    osize = st.Read(by, 0, (int)by.Length);

                    percent = (float)totalDownloadedByte / (float)totalBytes * 100;
                    //label1.Text = "当前补丁下载进度" + percent.ToString() + "%";
                    statusStrip1.Items[0].Text = "当前更新包下载进度" + percent.ToString() + "%";
                    System.Windows.Forms.Application.DoEvents(); //必须加注这句代码，否则label1将因为循环执行太快而来不及显示信息
                }
                so.Close();
                st.Close();
            }
            catch (System.Exception)
            {
                throw;
            }
        }
        void startdownload()
        {
            DownloadFile(downloadSite, Application.StartupPath + "\\up.zip", progressBar1);
            Process.Start(Application.StartupPath + "\\up.exe", "wmsjb");
        }
        private void Update_Closing(object sender, FormClosingEventArgs e)
        {

            //Environment.Exit(0);
        }
    }
}
