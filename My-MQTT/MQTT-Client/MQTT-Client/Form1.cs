using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace MQTT_Client
{
    public partial class Form1 : Form
    {
        private MqttClient client;
        private string uri;
        private string subTopic;
        private string pubTopic;
        private string sendMsg;

        private delegate void RecvDel(string str);

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 连接服务器按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                client = new MqttClient(IPAddress.Parse(uri));
                string clientId = Guid.NewGuid().ToString();
                client.Connect(clientId);
                button1.BackColor = Color.Chartreuse;
            }
            catch
            {
                MessageBox.Show("连接服务器出现问题！");
                button1.BackColor = Color.Red;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            uri = textBox1.Text;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            uri = textBox1.Text;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.Disconnect();
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            subTopic = textBox3.Text;
        }

        /// <summary>
        /// 订阅按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(subTopic))
            {
                client.MqttMsgPublishReceived += OnReceive;
                client.Subscribe(new string[] { subTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                textBox4.Text = subTopic;
                RecvMsg.Text = null;
                RecvNum.Text = "0";
                button3.BackColor = Color.LawnGreen;
            }
            else
            {
                MessageBox.Show("订阅的主题为空！");
                button3.BackColor = Color.Red;
            }
        }

        private void OnReceive(object sender, MqttMsgPublishEventArgs e)
        {
            //处理接收到的消息
            string msg = System.Text.Encoding.UTF8.GetString(e.Message);
            this.BeginInvoke(new RecvDel(ShowRecvMsg), msg);
        }

        private void ShowRecvMsg(string str)
        {
            RecvNum.Text = (Int32.Parse(RecvNum.Text) + 1).ToString();
            RecvMsg.Text = str;
        }

        /// <summary>
        /// 发布按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(pubTopic))
            {
                client.Publish(pubTopic, Encoding.UTF8.GetBytes(sendMsg));
            }
        }

        private void SendMsg_TextChanged(object sender, EventArgs e)
        {
            sendMsg = SendMsg.Text;
        }

        private void SendTopic_TextChanged(object sender, EventArgs e)
        {
            pubTopic = SendTopic.Text;
        }
    }
}