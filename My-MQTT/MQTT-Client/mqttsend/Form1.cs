using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace mqttsend
{
    public partial class Form1 : Form
    {
        private MqttClient client = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //这个字符串是向服务器发送的数据信息
            string strValue = "1,1,1,1,1,1";
            // 发送一个内容是123 字段是klabs的信息
            client.Publish("angular", Encoding.UTF8.GetBytes(strValue));
            //MessageBox.Show("发送消息的消息是："+strValue);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            client = new MqttClient(IPAddress.Parse("127.0.0.1"));
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            client.Connect("send");
            //client.Subscribe(new string[] { "angular" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
        }

        private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            //处理接收到的消息
            string msg = System.Text.Encoding.Default.GetString(e.Message);
            //label1.Text = msg;
            MessageBox.Show("接受到的消息是：" + msg);
            Console.WriteLine(msg);
        }
    }
}