using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua;
using OpcUaHelper;

namespace opcua_ConsoleApplication1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ////利用OpcUaHelper
            //OpcUaClient client = new OpcUaClient();
            //try
            //{
            //    //client.ConnectServer("opc.tcp://DESKTOP-1CG62EG:51212");
            //    client.ConnectServer("opc.tcp://DESKTOP-1CG62EG:51212");
            //    Console.WriteLine("成功连接服务器！");
            //}
            //catch
            //{
            //    Console.WriteLine("连接服务器失败！");
            //}
            ////OpcNodeAttribute[] t = client.ReadNoteAttributes("ns=2;s=Robot1_Axis1");
            //OpcNodeAttribute[] t = client.ReadNoteAttributes("ns=2;s=Robot1");
            //Console.WriteLine(t[2].Value);
            //foreach (OpcNodeAttribute temp in t)
            //{
            //    Console.WriteLine(temp.Value);
            //}
            //Console.ReadKey();

            OpcuaClient client = new OpcuaClient();
            try
            {
                client.ConnectServer("opc.tcp://DESKTOP-1CG62EG:51212");
                Console.WriteLine("成功连接服务器！");
            }
            catch
            {
                Console.WriteLine("连接服务器失败！/");
            }
            DataValue[] t = client.ReadNoteDataValueAttributes("ns=2;s=Robot1_Axis2");
            foreach (DataValue d in t)
            {
                Console.WriteLine(d.ToString());
            }
            Console.ReadKey();
        }
    }
}