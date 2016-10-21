using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace WFormSrc
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Db.path = AppDomain.CurrentDomain.BaseDirectory + "Update.ini";
            CheckVer checkver = new CheckVer();
            string serivceVer = checkver.GetServiceVer();
            string clientVer = Db.getver();
            if (clientVer != serivceVer)
            {
                DialogResult dialogResult = MessageBox.Show("有新版本，是否更新？", "升级", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (dialogResult == DialogResult.OK)
                {
                    Application.Exit();
                    Process.Start("Update.exe");
                }
            }
            else
            {
               // MessageBox.Show("已更新至最高版本！");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
