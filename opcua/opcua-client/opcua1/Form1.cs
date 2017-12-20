using OpcUaHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace opcua1
{
    public partial class Form1 : Form
    {
        private OpcUaClient opcUaClient = new OpcUaClient();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (FormBrowseServer form = new FormBrowseServer())
            {
                form.ShowDialog();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            opcUaClient.Disconnect();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //opcUaClient.ConnectServer("opc.tcp://desktop-1cg62eg:51510/UA/DemoServer");
            opcUaClient.ConnectServer("opc.tcp://DESKTOP-1CG62EG:51212");
        }

        /// <summary>
        /// 读取数据按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                var value = opcUaClient.ReadNode<int>("ns=2;s=Robot1_Axis1");
                MessageBox.Show(value.ToString()); // 显示测试数据
            }
            catch (Exception ex)
            {
                // 使用了opc ua的错误处理机制来处理错误，网络不通或是读取拒绝
                Opc.Ua.Client.Controls.ClientUtils.HandleException(Text, ex);
            }
        }

        /// <summary>
        /// 写入节点数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                bool IsSuccess = opcUaClient.WriteNode("ns=3;i=10221", 87665174);
                MessageBox.Show(IsSuccess.ToString()); // 显示True，如果成功的话
            }
            catch (Exception ex)
            {
                // 使用了opc ua的错误处理机制来处理错误，网络不通或是读取拒绝
                Opc.Ua.Client.Controls.ClientUtils.HandleException(Text, ex);
            }
        }

        /// <summary>
        /// 数据订阅
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            opcUaClient.MonitorValue("ns=3;i=10849",
        new Action<int, Action>(MonitorTestValueFloat));
        }

        private void MonitorTestValueFloat(int value, Action action)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    MonitorTestValueFloat(value, action);
                }));
                return;
            }

            // 更新显示，间隔为100ms刷新
            label1.Text = value.ToString();
            // 如果需要停止，调用action即可
            // action?.Invoke();
        }
    }
}