using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace 歌曲标签自动整理
{
    public partial class setting : Form
    {
        public setting()
        {
            InitializeComponent();
        }

        private void setting_Load(object sender, EventArgs e)
        {
            textBox1.Text = Form1.zgh.ToString();
            textBox2.Text = Form1.time_out.ToString();
            switch (Form1.ESetting)
            {
                case "EnOnly":
                    {
                        radioButtonEnOnly.Checked = true;
                        radioButtonAll.Checked = false;
                        radioButtonNone.Checked = false;
                        break;
                    }
                case "None":
                    {
                        radioButtonNone.Checked = true;
                        radioButtonEnOnly.Checked = false;
                        radioButtonAll.Checked = false;
                        break;
                    }
                case "All":
                    {
                        radioButtonNone.Checked = false;
                        radioButtonEnOnly.Checked = false;
                        radioButtonAll.Checked = true;
                        break;
                    }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (int.Parse(textBox1.Text) < 1 || int.Parse(textBox1.Text) > 100)
                {
                    MessageBox.Show("  %>_<% \n线程数应在1-100之间", "!!!!!!哇去!!!!!!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (int.Parse(textBox2.Text) <= 0)
                {
                    MessageBox.Show("  %>_<% \n超时时间不在合理范围啊啊啊啊啊", "!!!!!!哇去!!!!!!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error); return;
                }
                Form1.workLines = int.Parse(textBox1.Text);
                Form1.zgh = int.Parse(textBox1.Text);
                Form1.time_out = int.Parse(textBox2.Text);
                if (radioButtonEnOnly.Checked) Form1.ESetting = "EnOnly";
                if (radioButtonAll.Checked) Form1.ESetting = "All";
                if (radioButtonNone.Checked) Form1.ESetting = "None";
                Properties.Settings.Default.ENMFP = Form1.ESetting;
                Properties.Settings.Default.TimeOut = Form1.time_out;
                Properties.Settings.Default.WorkLine = Form1.workLines;
                Properties.Settings.Default.Save();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("  %>_<% \n" + ex.Message, "!!!!!!哇去!!!!!!",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        


        public partial class nTextBox : System.Windows.Forms.TextBox
        {
            #region 自定义成员
            private int maxIntegerLength = 10;
            private int minIntegerLength = 0;
            private int maxDecimalLength = 4;
            private int minDecimalLength = 0;
            private int integerLength;
            private int decimalLength;
            #endregion

            #region 自定义属性
            /// <summary>
            /// 最大整数位数
            /// </summary>
            public int MaxIntegerLength
            {
                get
                {
                    return maxIntegerLength;
                }
                set
                {
                    if (value >= 0 && value >= minIntegerLength)
                    {
                        maxIntegerLength = value;
                    }
                    else
                    {
                        throw new Exception("最大整数位数不应小于最小整数位数");
                    }
                }
            }

            /// <summary>
            /// 最小整数位数
            /// </summary>
            public int MinIntegerLength
            {
                get
                {
                    return minIntegerLength;
                }
                set
                {
                    if (value >= 0 && value <= maxIntegerLength)
                    {
                        minIntegerLength = value;
                    }
                    else
                    {
                        throw new Exception("最小整数位数不应大于最大整数位数");
                    }
                }
            }

            /// <summary>
            /// 最大小数位数
            /// </summary>
            public int MaxDecimalLength
            {
                get
                {
                    return maxDecimalLength;
                }
                set
                {
                    if (value >= 0 && value >= minDecimalLength)
                    {
                        maxDecimalLength = value;
                    }
                    else
                    {
                        throw new Exception("最大小数位数不应小于最小小数位数");
                    }
                }
            }

            /// <summary>
            /// 最小小数位数
            /// </summary>
            public int MinDecimalLength
            {
                get
                {
                    return minDecimalLength;
                }
                set
                {
                    if (value >= 0 && value <= maxDecimalLength)
                    {
                        minDecimalLength = value;
                    }
                    else
                    {
                        throw new Exception("最小小数位数不应大于最大小数位数");
                    }
                }
            }
            #endregion

            #region 重写方法
            protected override void OnKeyPress(KeyPressEventArgs e)
            {
                int editIndex = SelectionStart;         //获取当前编辑位

                if (e.KeyChar == (char)Keys.Back) return;   //放行"退格"键

                if (e.KeyChar.Equals('.') || Char.IsNumber(e.KeyChar)) //过滤非数字与非小数点
                {
                    if (Text.IndexOf(".") > -1)     //是否存在小数点
                    {
                        //禁止重复输入小数点
                        if (e.KeyChar.Equals('.'))
                        {
                            e.Handled = true;
                            return;
                        }
                        else
                        {
                            if (SelectedText.Length > 0)
                            {
                                return;
                            }

                            integerLength = Text.IndexOf(".");
                            decimalLength = Text.Length - integerLength - 1;

                            //控制最大小数位数
                            if (decimalLength >= maxDecimalLength && editIndex > Text.IndexOf("."))
                            {
                                e.Handled = true;
                                return;
                            }

                            //控制最大整数位数
                            if (integerLength >= maxIntegerLength && editIndex <= Text.IndexOf("."))
                            {
                                e.Handled = true;
                                return;
                            }
                        }
                    }
                    else
                    {
                        //控制最大整数位数
                        integerLength = Text.Length;
                        if (integerLength == maxIntegerLength && !e.KeyChar.Equals('.'))
                        {
                            e.Handled = true;
                        }
                    }
                }
                else
                {
                    e.Handled = true;
                }

                base.OnKeyPress(e);
            }


            protected override void OnLeave(EventArgs e)
            {
                if (Text == null || Text == "") return;

                Text = Text.TrimStart('0');

                //取整数位数与小数位数
                if (Text.IndexOf(".") == -1)
                {
                    integerLength = Text.Length;
                    decimalLength = 0;
                }
                else
                {
                    integerLength = Text.IndexOf(".");
                    decimalLength = Text.Length - integerLength - 1;

                    //验证小数位数是否符合最小值(不足补零)            
                    if (decimalLength < minDecimalLength)
                    {
                        Text = Text.PadRight(integerLength + minDecimalLength + 1, '0');
                    }
                }

                //整数未输自动补零
                if (integerLength == 0)
                {
                    Text = "0" + Text;
                }

                //验证整数位数是否符合最小值
                if (integerLength < minIntegerLength)
                {
                    Focus();
                    Select(0, integerLength);
                }

                //验证整数位数是否符合最大值
                if (integerLength > maxIntegerLength)
                {
                    Focus();
                    Select(0, integerLength);
                }
                base.OnLeave(e);
            }
            #endregion
        }

        

        
    }
}