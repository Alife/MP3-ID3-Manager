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

    public partial class Form1 : Form
    {
        //这里是全局变量
        public static int workLines = 5;
        int[] waitWorkedFileIndex;
        string[] waitFileNameInListView;
        //string[,] workList;
        bool stopSingle = true;
        bool canBeDelete = true;
        int shengyuxianchengshu = 1;
        public static string fileNameForRename;
        bool watingClose = false;
        public static int zgh = workLines;
        public static string version = "2.00";
        public static int time_out = 60;//设置超时
        public static string ESetting = "None";
        string upServer = "http://mp3meneger.sinaapp.com/";//必须以/结尾
        //完

        void RecordError(Exception ex, string fileName)
        {
            lock (this)//避免同时操作
            {
                StreamWriter sw = new StreamWriter(Application.StartupPath + "\\error.log", true,
Encoding.Unicode);

                sw.WriteLine(DateTime.Now.ToString());
                sw.WriteLine(fileName);
                sw.WriteLine(ex.Message);
                sw.WriteLine("");
                sw.Close();
            }
        }

        bool ID3Edit(string filename, string coverpath, string title, string artist, string album, string year)
        {
            // 加载MP3
            ID3Info info;


            try
            {
                info = new ID3Info(filename, true);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("哇去！文件加载失败！%>_<% \n" +"\n"+filename, "!!!!!!哇去!!!!!!",
                //MessageBoxButtons.OK, MessageBoxIcon.Error);
                RecordError(ex, filename);
                return false;
            }




            info.ID3v1Info.HaveTag = true;
            info.ID3v2Info.HaveTag = true;
            info.ID3v2Info.SetMinorVersion(Convert.ToInt32("3"));
            // 创建新封面
            if (coverpath != "")
            {
                try
                {
                    AttachedPictureFrame pic = new AttachedPictureFrame(
                        new FrameFlags(),
                        //FrameFlags.Compression
                        "cover.jpg", TextEncodings.UTF_16,
                        "",
                        AttachedPictureFrame.PictureTypes.Cover_Front,
                        new MemoryStream(File.ReadAllBytes(coverpath)));
                    // 添加新封面到MP3中
                    info.ID3v2Info.AttachedPictureFrames.Clear();
                    info.ID3v2Info.AttachedPictureFrames.Add(pic);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("  %>_<% \n" + ex.Message, "!!!!!!哇去!!!!!!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }


            info.ID3v1Info.TrackNumber = Convert.ToByte("1");//音轨
            //info.ID3v1Info.Genre = Convert.ToByte("60");//风格
            info.ID3v1Info.Comment = "";
            info.ID3v1Info.Year = year;
            if (title.Length >= 30) info.ID3v1Info.Title = title.Remove(29);
            else info.ID3v1Info.Title = title;
            if (artist.Length >= 30) info.ID3v1Info.Artist = artist.Remove(29);
            else info.ID3v1Info.Artist = artist;
            if (album.Length >= 30) info.ID3v1Info.Album = album.Remove(29);
            else info.ID3v1Info.Album = album;



            // 设置其它属性

            info.ID3v2Info.SetTextFrame("TIT2", title);

            info.ID3v2Info.SetTextFrame("TPE1", artist);
            info.ID3v2Info.SetTextFrame("TPE2", artist);
            info.ID3v2Info.SetTextFrame("TOPE", artist);

            info.ID3v2Info.SetTextFrame("TYER", year);

            info.ID3v2Info.SetTextFrame("TALB", album);
            info.ID3v2Info.SetTextFrame("TRCK", "1");
            //info.ID3v2Info.SetTextFrame("TCON", "60");
            // info.ID3v2Info.SetTextFrame("COMM", "4");

            // 保存到MP3中
            //info.Save(Convert.ToInt32("3"));

            try
            {
                info.ID3v2Info.Save();
                info.ID3v1Info.Save();
            }
            catch (Exception error)
            {
                //MessageBox.Show("哇去！信息保存失败！  %>_<% \n" + error.Message, "!!!!!!哇去!!!!!!",
                //MessageBoxButtons.OK, MessageBoxIcon.Error);
                RecordError(error, filename);
                return false;
            }
            return true;
        }


        public Form1()
        {
            InitializeComponent();
        }







        private void toolStripButton1_Click(object sender, EventArgs e)
        {

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (string ffile in openFileDialog1.FileNames)
                {
                    bool isReshow = false;
                    for (int i = 0; i <= listView1.Items.Count - 1; i++)  //用于检查列表框是否有重复的文件
                    {
                        if (listView1.Items[i].Text == ffile)
                        {
                            isReshow = true;
                        }
                    }
                    if (isReshow)
                    {
                        MessageBox.Show("这歌列表里有啦！  -_-#\n" + ffile, "这歌列表里有啦！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {

                        ListViewItem lv = new ListViewItem(ffile);//第一列的记录为
                        lv.SubItems.Add("否");//添加第三列的内容
                        lv.SubItems.Add("");
                        lv.SubItems.Add("");
                        lv.SubItems.Add("");
                        lv.SubItems.Add("");
                        lv.ImageIndex = 0;//指定图像的索引
                        listView1.Items.Add(lv);
                    }
                }
            }
        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            if (stopSingle == false)
            {
                stopSingle = true;
                watingClose = true;
                e.Cancel = true;
                MessageBox.Show("正在等待线程结束", "结束！", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                this.ControlBox = false;
                this.Text = "华丽的MP3伴侣 - "+version+"  正在等待线程结束";
            }
            else Application.Exit();


        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0)
            {
                MessageBox.Show("  %>_<% \n" + "你没有添加任何歌曲！！！", "!!!!!!哇去!!!!!!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error); return;
            }
            toolStripButton1.Enabled = false;
            toolStripButton2.Enabled = false;
            toolStripButton3.Enabled = false;
            toolStripButton4.Enabled = false;
            //将listview内容载入worklist[,]
            //workList = new string[5, listView1.Items.Count];
            //for (int i = 0; i <= listView1.Items.Count - 1;i++ )
            //{
            //    workList[0, i] = listView1.Items[i].Text;
            //    workList[1, i] = "否";
            //    workList[2, i] = "";
            //    workList[3, i] = "";
            //    workList[4, i] = "";
            //}
            canBeDelete = false;
            shengyuxianchengshu = 1;
            if (listView1.Items.Count < workLines) workLines = listView1.Items.Count;
            else workLines = zgh;
            stopSingle = false;

            gowork();//工作！
        }

        void gowork()
        {
            try
            {
                foreach (string d in Directory.GetFileSystemEntries(".\\temp"))
                {
                    if (File.Exists(d))
                    {
                        File.Delete(d);//直接删除其中的文件  
                    }
                }
            }
            catch(Exception e){
                ME(e.Message,"删除临时文件时出错！");
                return;
            }
            //将列表读入数组
            int temp = 0;
            for (int i = 0; i <= listView1.Items.Count - 1; i++)
            {
                if (listView1.Items[i].SubItems[1].Text != "完成") temp += 1;
            }
            waitWorkedFileIndex = new int[temp];
            waitFileNameInListView = new string[temp];
            int temp2 = 0;
            for (int i = 0; i <= listView1.Items.Count - 1; i++)
            {
                if (listView1.Items[i].SubItems[1].Text != "完成")
                {
                    waitFileNameInListView[temp2] = listView1.Items[i].Text.Substring(listView1.Items[i].Text.LastIndexOf("\\") + 1);
                    waitWorkedFileIndex[temp2] = i;
                    temp2 += 1;
                }
            }//读入结束
            if (waitWorkedFileIndex.Length == 0)
            {
                MessageBox.Show("木有需要整理的歌曲啊");
                canBeDelete = true;
                toolStripButton1.Enabled = true;
                toolStripButton2.Enabled = true;
                toolStripButton3.Enabled = true;
                toolStripButton4.Enabled = true;
                shengyuxianchengshu = 1;
                stopSingle = true;
                return;
            }
            MessageBox.Show("开始整理" + waitWorkedFileIndex.Length.ToString() + "首歌曲!\nO(∩_∩)O~~");
            //启动线程
            Thread wWorkLine = null;
            if (workLines > waitWorkedFileIndex.Length) workLines = waitWorkedFileIndex.Length;
            int kao = waitWorkedFileIndex.Length;//用于防止在启动线程时waitworkedfileindex被先前启动的线程删除成员而少启动线程
            for (int i = 1; i <= workLines; i++)
            {
                if (i > kao) break;

                wWorkLine = new Thread(new ThreadStart(work));
                //MessageBox.Show(i.ToString()+waitWorkedFileIndex.Length.ToString());

                wWorkLine.Start();
            }

        }
        public void work()
        {
            while (waitWorkedFileIndex.Length > 0 && stopSingle == false)
            {       //工作开始
                int nowIndex = 0;
                lock (this)
                {
                    if (waitWorkedFileIndex.Length > 0 && stopSingle == false)
                    {

                        int[] temzhuanggengheng = new int[waitWorkedFileIndex.Length - 1];
                        nowIndex = waitWorkedFileIndex[0];//读入一个引索用于工作
                        for (int i = 0; i <= waitWorkedFileIndex.Length - 2; i++)//除去waitworkedindex的第一个成员,即刚刚读的那个
                        {
                            temzhuanggengheng[i] = waitWorkedFileIndex[i + 1];
                        }//for
                        waitWorkedFileIndex = temzhuanggengheng;
                    }//if
                    else { canBeDelete = true; return; }
                }//lock
                lock (this)
                {
                    listView1.Items[nowIndex].SubItems[1].Text = "查找中";//更改工作状态
                }
                /*旧代码
                
                string nowWorkedFileName = listView1.Items[nowIndex].Text.Substring(listView1.Items[nowIndex].Text.LastIndexOf("\\") + 1);
                nowWorkedFileName = nowWorkedFileName.Remove(nowWorkedFileName.LastIndexOf("."));//读入文件名而不包括".mp3"
                //读搜索结果
                string htmlData = getHtml(
                    "http://cgi.music.soso.com/fcgi-bin/m.q?w=" + ConvertToBianMa(nowWorkedFileName) + "&source=1&t=0"
                    ,false);//读完
                //提取各种信息
                if (htmlData.IndexOf("抱歉，找不到") != -1)
                {
                    listView1.Items[nowIndex].SubItems[1].Text = "找不到匹配信息！";
                    
                }
                else{
                    var a0 = GuoLvContect(htmlData);
                    if (a0 == "" || a0 == null)
                    {
                        lock (this)
                        {
                            listView1.Items[nowIndex].SubItems[1].Text = "否";
                            
                        }
                    }
                    else
                    {
                    string a1 = GetSongName(a0);
                    string a2 = GetArtist(a0);
                    string a3 = GetAlbum(a0);
                    lock (this)
                    {
                        listView1.Items[nowIndex].SubItems[2].Text = a1;
                        listView1.Items[nowIndex].SubItems[3].Text = a2;
                        listView1.Items[nowIndex].SubItems[4].Text = a3;

                    }
                 */
                identify hehe = new identify(listView1.Items[nowIndex].Text, nowIndex);
                if (hehe.Title.IndexOf("#####") != -1)
                {
                    lock (this)
                    {
                        listView1.Items[nowIndex].SubItems[1].Text = "找不到匹配信息！";
                    }
                }
                else
                {
                    string a1 = hehe.Title;
                    string a2 = hehe.Artist;
                    string a3 = hehe.Album;
                    lock (this)
                    {
                        listView1.Items[nowIndex].SubItems[2].Text = a1;
                        listView1.Items[nowIndex].SubItems[3].Text = a2;
                        listView1.Items[nowIndex].SubItems[4].Text = a3;

                    }
                    if (a1 == "")//如果没有歌名，直接放弃查找专辑图和写入标签
                    {
                        lock (this)
                        {
                            listView1.Items[nowIndex].SubItems[1].Text = "否";

                        }
                    }
                    else
                    {
                        if (hehe.Cover) lock (this) { listView1.Items[nowIndex].SubItems[5].Text = "找到"; }
                        else lock (this) { listView1.Items[nowIndex].SubItems[5].Text = "无"; }
                        //开始写入标签
                        string isFound;
                        lock (this)
                        {
                            isFound=listView1.Items[nowIndex].SubItems[5].Text;
                        }
                        if (isFound != "找到")
                        {
                            if (ID3Edit(listView1.Items[nowIndex].Text, "", a1, a2, a3, hehe.Year) == false) lock (this) { listView1.Items[nowIndex].SubItems[1].Text = "保存时出错"; }
                            else { lock (this) { listView1.Items[nowIndex].SubItems[1].Text = "完成"; } }
                        }
                        else
                        {
                            if (ID3Edit(listView1.Items[nowIndex].Text, Application.StartupPath + "\\temp\\" + nowIndex.ToString() + ".jpg", a1, a2, a3, hehe.Year) == false) listView1.Items[nowIndex].SubItems[1].Text = "保存时出错";
                            else { lock (this) { listView1.Items[nowIndex].SubItems[1].Text = "完成"; } }
                        }
                        //更改工作状态
                    }
                }
            }//while
            lock (this)
            {
                //while ()


                if (shengyuxianchengshu == workLines)
                {
                    canBeDelete = true;
                    toolStripButton1.Enabled = true;
                    toolStripButton2.Enabled = true;
                    toolStripButton3.Enabled = true;
                    toolStripButton4.Enabled = true;
                    shengyuxianchengshu = 1;
                    stopSingle = true;
                    if (watingClose == true) Application.Exit();
                    else MessageBox.Show("整理完成！ \nO(∩_∩)O~~", "完成", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                else shengyuxianchengshu++;
            }//lock
        }//work

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (!stopSingle)
            {
                stopSingle = true;
                MessageBox.Show("正在等待线程结束", "结束！", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (!canBeDelete)
            {
                MessageBox.Show("工作中不能删除.", "出错", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            if (listView1.SelectedIndices.Count == 0)
            {
                MessageBox.Show("你你你，都没有选怎么删除？\n注:按Ctrl或Shift可以多选.", "出错", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            for (int j = listView1.SelectedIndices.Count - 1; j >= 0; j--)
            {
                listView1.Items.Remove(listView1.SelectedItems[j]);
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            setting settingshow = new setting();
            settingshow.ShowDialog();
        }

        private void listview1_DoubleClick(object sender, System.EventArgs e)
        //双击重命名
        {

            if (stopSingle == true)
            {

                fileNameForRename = "";
                ListViewItem cmicItem = listView1.SelectedItems[0];//cmicItem就是你双击的是哪项 
                Rename.filenameforshow = cmicItem.Text;
                Rename ZGH = new Rename();
                ZGH.ShowDialog();
                if (fileNameForRename != "")
                {
                    cmicItem.Text = fileNameForRename;
                    cmicItem.SubItems[1].Text = "否";
                    cmicItem.SubItems[2].Text = "";
                    cmicItem.SubItems[3].Text = "";
                    cmicItem.SubItems[4].Text = "";
                    cmicItem.SubItems[5].Text = "";
                }
            }
            else ME("工作中不能重命名!", "出错");
        }

        private void Form1_SizeChanged(object sender, System.EventArgs e)
        {
            listView1.Size = new System.Drawing.Size(this.Size.Width - 42, this.Size.Height - 112);
        }

        private static void ME(string text, string title)
        {
            MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            about ZGH = new about();
            ZGH.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            workLines = Properties.Settings.Default.WorkLine;
            time_out = Properties.Settings.Default.TimeOut;
            ESetting = Properties.Settings.Default.ENMFP;
            Thread zgh = new Thread(happanWhenStart);
            zgh.Start();
            this.Text = "华丽的MP3伴侣 - "+version;
        }

        public void UUpdate()
        {
            //统计
            string s=(new GetHtml(Form1.time_out)).getHtml(upServer+"tip.php?action=tongji", true);
            (new GetHtml(Form1.time_out)).getHtml(s, true);
            //
            var zghh = (new GetHtml(Form1.time_out)).getHtml(upServer+"tip.php?action=getversion", true);
            zghh = Regex.Match(zghh, "(\\d\\.\\d\\d)").Groups[1].Value;
            if (zghh == null || zghh == "") return;
            if (zghh != version)
            {
                update zgh = new update();
                zgh.ShowDialog();
            }
        }

        void happanWhenStart()
        {
            if (File.Exists(Application.StartupPath + "\\error.log") == false)
            {

                try
                {
                    StreamWriter sw = new StreamWriter(Application.StartupPath + "\\error.log", true, Encoding.Unicode);
                    sw.Close();
                }
                catch (Exception ex)
                {
                    ME("创建文件时出错，请检查权限，点击确定退出。\n" + ex.Message, "创建失败 %>_<%");
                    Application.Exit();
                }
                Directory.CreateDirectory(Application.StartupPath + "\\temp");
            }
            //检测是否第一次运行
            if(Properties.Settings.Default.isFirstRun){
                MessageBox.Show("2014年1月21日 更新说明：\n\n  1.加入全新的音乐识别引擎(使用开源的Echo Nest). 不过对中文支持不好，且CPU占用较高，建议对依靠文件名不能识别的音乐使用。\n  2.使用全新的音乐信息库(虾米乐库), 有着更全的专辑信息和高分辨率的专辑图! (感谢 @比那名居艾姆紫 提供的信息)\n  3.支持拖拽加入文件。\n  4.修正一个BUG, 这个BUG曾导致在保存某些mp3文件时出错。\n  5.加入全新的过滤规则，使识别结果更加准确。\n  6.感谢大家的支持。",
                    "感谢支持 O(∩_∩)O~~", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                Properties.Settings.Default.isFirstRun = false;
                Properties.Settings.Default.Save();
            }
            UUpdate();
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            Array aryFiles = ((System.Array)e.Data.GetData(DataFormats.FileDrop));
            for (int ii = 0; ii < aryFiles.Length; ii++)
            {
                ///通过For循环把所有文件路径加到字符串label4.text里面.
                string ffile = aryFiles.GetValue(ii).ToString();// +Environment.NewLine;
                if (!ffile.EndsWith(".mp3",false,null)) continue;
                bool isReshow = false;
                for (int i = 0; i <= listView1.Items.Count - 1; i++)  //用于检查列表框是否有重复的文件
                {
                    if (listView1.Items[i].Text == ffile)
                    {
                        isReshow = true;
                        break;
                    }
                }
                if (isReshow)
                {
                    MessageBox.Show("这歌列表里有啦！  -_-#\n" + ffile, "这歌列表里有啦！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {

                    ListViewItem lv = new ListViewItem(ffile);//第一列的记录为
                    lv.SubItems.Add("否");//添加第三列的内容
                    lv.SubItems.Add("");
                    lv.SubItems.Add("");
                    lv.SubItems.Add("");
                    lv.SubItems.Add("");
                    lv.ImageIndex = 0;//指定图像的索引
                    listView1.Items.Add(lv);
                }
            }
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else e.Effect = DragDropEffects.None;
        }
    }
}
