using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.ListBox;

namespace VEDnsControl
{
    public partial class MainWnd : Form 
    {
      
        DnsConfigLoader     dnsLoader;
        DnsController       dnsController;
        class MainUIOutListener : UIListener
        {
            MainWnd mWnd;
            public MainUIOutListener(MainWnd mw)
            {
                mWnd = mw;
            }

            public override void WriteOut(string str)
            {
                mWnd.WriteOut(str);
            }
        }
      
        public MainWnd()
        {
            InitializeComponent();
            this.WriteOut("初始化...");

            try
            {
                UIListener globalOut = new MainUIOutListener(this);
                dnsLoader = new DnsConfigLoader(globalOut);
                dnsController = new DnsController(dnsLoader);

            }catch(Exception e)
            {
                this.WriteOut(e.Message);
            }
        }

        private void WriteOut(string strLine)
        {
            this.OutputBox.Items.Add(strLine);
            OutputBox.TopIndex = OutputBox.Items.Count - (int)(OutputBox.Height / OutputBox.ItemHeight);
            OutputBox.SetSelected(OutputBox.Items.Count - 1,true);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(this.DnsBox.SelectedItems.Count > 0)
                this.DelBtn.Enabled = true;
            else
                this.DelBtn.Enabled = false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.AddBtn.Enabled = true;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        //ui dnsbox to dnsloader.mdns
        private void UpdateInfo(bool bModified = false)
        {
            this.WriteOut("dns数量:" + dnsLoader.mDns.Count);
            LabelNum.Text = dnsLoader.mDns.Count.ToString();
             
            if (bModified)
                this.AppBtn.Enabled = true;
        }

        private void MainWnd_Load(object sender, EventArgs e)
        {
            UpdateInfo();
            foreach (DnsConfigLoader.DnsItem it in dnsLoader.mDns)
            {
                this.DnsBox.Items.Add(it.strDomain);
            }
            this.DnsBox.SelectedItem = 0;
            this.DnsBox.Select();
        }

        //添加
        private void button2_Click(object sender, EventArgs e)
        {
            this.AddBtn.Enabled = false;
            int index = this.DnsBox.Items.Add(this.dnsInputEdit.Text);
            this.dnsLoader.Add(this.dnsInputEdit.Text, "127.0.0.1");
            this.DnsBox.TopIndex = index;
            this.DnsBox.SelectedItem = index;
            UpdateInfo(true);
        }
 
        //删除
        private void DelBtn_Click(object sender, EventArgs e)
        {
            SelectedObjectCollection selItems = this.DnsBox.SelectedItems;
            this.WriteOut("删除:" + selItems.Count);
            while (selItems.Count >= 1)
            {
                foreach (object it in selItems)
                {
                    this.DnsBox.Items.Remove(it);
                    dnsLoader.Remove((string)it);
                    break;
                }
            }
            UpdateInfo(true);
        }

        //应用到系统
        private void AppBtn_Click(object sender, EventArgs e)
        {
            this.AppBtn.Enabled = false;
            this.dnsController.Applicate();
        }
    }
}
