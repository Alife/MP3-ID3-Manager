using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace 歌曲标签自动整理
{
    public partial class Rename : Form

    {
        public static string filenameforshow;
        public Rename()
        {
            InitializeComponent();
        }

        private void Rename_Load(object sender, EventArgs e)
        {
            textBox1.Text = filenameforshow.Substring(filenameforshow.LastIndexOf("\\" )+1);
            textBox1.Text=textBox1.Text.Remove(textBox1.Text.LastIndexOf("."));
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var muluzgh = filenameforshow.Remove(filenameforshow.LastIndexOf("\\")+1);
                if(textBox1.Text!=filenameforshow)
                {
                    File.Move(filenameforshow, muluzgh + textBox1.Text + ".mp3");
                    Form1.fileNameForRename = muluzgh + textBox1.Text + ".mp3";
                }
                this.Close();
            }
            catch(Exception ex){
                MessageBox.Show("  %>_<% \n" + ex.Message + "\n" , "!!!!!!哇去!!!!!!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
