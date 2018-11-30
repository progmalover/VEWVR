using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VEWebVideoService;
namespace httpSeverTest
{
    public partial class Form1 : Form
    {
        HttpServer http_server =  new HttpServer();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            http_server.Start("http://localhost:8233/");
        }
    }
}
