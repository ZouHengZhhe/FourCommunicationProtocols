using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ConsoleApplication1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //创建客户端实例
            //MqttClient client = new MqttClient(IPAddress.Parse(MQTT_BROKER_ADDRESS)); //主机为IP时
            //MqttClient client = new MqttClient(MQTT_BROKER_ADDRESS); //当主机地址为域名时

            MqttClient client = new MqttClient(IPAddress.Parse("127.0.0.1"));

            // 注册消息接收处理事件，还可以注册消息订阅成功、取消订阅成功、与服务器断开等事件处理函数
            //client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            //生成客户端ID并连接服务器
            //string clientId = Guid.NewGuid().ToString();
            //client.Connect("aaa");
            //client.Connect("127.0.0.1");
            //Console.WriteLine(client.IsConnected);
            try
            {
                string clientId = Guid.NewGuid().ToString();
                Console.WriteLine(clientId);
                client.Connect(clientId);
                //client.Connect("MJ_124804073");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine(client.IsConnected);

            // 订阅主题"/home/temperature" 消息质量为 2
            //client.Subscribe(new string[] { "angular" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

            //client.Publish("angular", System.Text.Encoding.Default.GetBytes("Hello World"));
            client.Publish("angular", Encoding.UTF8.GetBytes("hello"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            Console.ReadKey();
        }

        private static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            //处理接收到的消息
            string msg = System.Text.Encoding.Default.GetString(e.Message);

            Console.WriteLine(msg);
        }
    }
}