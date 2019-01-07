namespace VEDnsControl
{
    partial class MainWnd
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWnd));
            this.DnsBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dnsInputEdit = new System.Windows.Forms.TextBox();
            this.AddBtn = new System.Windows.Forms.Button();
            this.DelBtn = new System.Windows.Forms.Button();
            this.AppBtn = new System.Windows.Forms.Button();
            this.OutputBox = new System.Windows.Forms.ListBox();
            this.LabelNum = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // DnsBox
            // 
            this.DnsBox.AllowDrop = true;
            this.DnsBox.FormattingEnabled = true;
            this.DnsBox.ItemHeight = 12;
            this.DnsBox.Location = new System.Drawing.Point(11, 26);
            this.DnsBox.Name = "DnsBox";
            this.DnsBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.DnsBox.Size = new System.Drawing.Size(496, 364);
            this.DnsBox.TabIndex = 0;
            this.DnsBox.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(161, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "Dns重定向域名（域名/ip）：";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // dnsInputEdit
            // 
            this.dnsInputEdit.Location = new System.Drawing.Point(15, 396);
            this.dnsInputEdit.Name = "dnsInputEdit";
            this.dnsInputEdit.Size = new System.Drawing.Size(205, 21);
            this.dnsInputEdit.TabIndex = 3;
            this.dnsInputEdit.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // AddBtn
            // 
            this.AddBtn.Enabled = false;
            this.AddBtn.Location = new System.Drawing.Point(235, 394);
            this.AddBtn.Name = "AddBtn";
            this.AddBtn.Size = new System.Drawing.Size(75, 23);
            this.AddBtn.TabIndex = 4;
            this.AddBtn.Text = "添加";
            this.AddBtn.UseVisualStyleBackColor = true;
            this.AddBtn.Click += new System.EventHandler(this.button2_Click);
            // 
            // DelBtn
            // 
            this.DelBtn.Enabled = false;
            this.DelBtn.Location = new System.Drawing.Point(324, 394);
            this.DelBtn.Name = "DelBtn";
            this.DelBtn.Size = new System.Drawing.Size(75, 23);
            this.DelBtn.TabIndex = 2;
            this.DelBtn.Text = "删除";
            this.DelBtn.UseVisualStyleBackColor = true;
            this.DelBtn.Click += new System.EventHandler(this.DelBtn_Click);
            // 
            // AppBtn
            // 
            this.AppBtn.Location = new System.Drawing.Point(413, 394);
            this.AppBtn.Name = "AppBtn";
            this.AppBtn.Size = new System.Drawing.Size(75, 23);
            this.AppBtn.TabIndex = 5;
            this.AppBtn.Text = "应用";
            this.AppBtn.UseVisualStyleBackColor = true;
            this.AppBtn.Click += new System.EventHandler(this.AppBtn_Click);
            // 
            // OutputBox
            // 
            this.OutputBox.BackColor = System.Drawing.SystemColors.MenuBar;
            this.OutputBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.OutputBox.FormattingEnabled = true;
            this.OutputBox.HorizontalScrollbar = true;
            this.OutputBox.ItemHeight = 12;
            this.OutputBox.Location = new System.Drawing.Point(14, 423);
            this.OutputBox.Name = "OutputBox";
            this.OutputBox.ScrollAlwaysVisible = true;
            this.OutputBox.Size = new System.Drawing.Size(493, 62);
            this.OutputBox.TabIndex = 6;
            // 
            // LabelNum
            // 
            this.LabelNum.AutoSize = true;
            this.LabelNum.Location = new System.Drawing.Point(164, 11);
            this.LabelNum.Name = "LabelNum";
            this.LabelNum.Size = new System.Drawing.Size(11, 12);
            this.LabelNum.TabIndex = 7;
            this.LabelNum.Text = "0";
            // 
            // MainWnd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(516, 491);
            this.Controls.Add(this.LabelNum);
            this.Controls.Add(this.OutputBox);
            this.Controls.Add(this.AppBtn);
            this.Controls.Add(this.AddBtn);
            this.Controls.Add(this.dnsInputEdit);
            this.Controls.Add(this.DelBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.DnsBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainWnd";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Dns控制台";
            this.Load += new System.EventHandler(this.MainWnd_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox DnsBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox dnsInputEdit;
        private System.Windows.Forms.Button AddBtn;
        private System.Windows.Forms.Button DelBtn;
        private System.Windows.Forms.Button AppBtn;
        private System.Windows.Forms.ListBox OutputBox;
        private System.Windows.Forms.Label LabelNum;
    }
}

