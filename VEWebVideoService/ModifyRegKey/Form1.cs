using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RegistryKey reg = null;
            try
            {
                reg = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\services\\sdserver",true);
            }
            catch { }

            if (reg == null)
            {
                //throw new Exception("注册表键值不存在！");
                this.textBox1.Text = "注册表键值不存在！";
                return;
            }

            string strIp = (string)reg.GetValue("ClientIp");
            if (strIp == null)
            {
                //throw new Exception("无法获得终端ip!");
                this.textBox1.Text = "无法获得终端ip!";
            }
            else
                this.textBox1.Text = strIp;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RegistryKey reg = null;
            try
            {
                reg = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\services\\sdserver", true);
            }
            catch { }

            if (reg == null)
            {
                reg = Registry.LocalMachine.CreateSubKey("SYSTEM\\CurrentControlSet\\services\\sdserver");
            }

          
            if (reg == null)
            {
                //throw new Exception("无法获得终端ip!");
                this.textBox1.Text = "create subkey failed!";
                return;
            }
            try
            {
                if(this.textBox1.Text.Length > 0)
                    reg.SetValue("ClientIp", this.textBox1.Text);
            }
            catch {
                this.textBox1.Text = "create key value failed!";
                return;
            }
            this.textBox1.Text = "create key value succ!";
        }
    }
}
