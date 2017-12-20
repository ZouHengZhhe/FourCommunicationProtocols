using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Opc.Ua;
using Opc.Ua.Client;

namespace opcua2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Opc.Ua.ClientBase c;

        private void Form1_Load(object sender, EventArgs e)
        {
            //ClientBase b = new ClientBase();
        }
    }
}