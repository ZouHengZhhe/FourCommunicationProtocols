using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Client.Controls;
using OpcUaHelper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;

namespace opcua1
{
    public partial class Form2 : Form
    {
        private OpcUaClient m_OpcUaClient = null; //opcua客户端
        private string _uri; //opcua服务器端IP
        private MqttClient _mqttClient;
        private Action<float, Action> _monitorAction;
        private Action _action;

        public Form2()
        {
            InitializeComponent();
            OpcUaClientInit();
            try
            {
                _mqttClient = new MqttClient(IPAddress.Parse("127.0.0.1"));
                string clientId = Guid.NewGuid().ToString();
                _mqttClient.Connect(clientId);
            }
            catch
            {
                MessageBox.Show("连接MQTT服务器端失败！");
            }
        }

        //=================================================================================

        #region OpcUaClient初始化

        /// <summary>
        /// 初始化OpcUaClient
        /// </summary>
        private void OpcUaClientInit()
        {
            m_OpcUaClient = new OpcUaClient();
            m_OpcUaClient.OpcUaName = "ZouHeng OpcUaClient";
            m_OpcUaClient.OpcStatusChange += M_OpcUaClient_OpcStatusChange1;
            //m_OpcUaClient.ConnectComplete += M_OpcUaClient_ConnectComplete;
        }

        #region OpcUaClient回调函数

        /// <summary>
        /// OPC 客户端的状态变化后的消息提醒
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void M_OpcUaClient_OpcStatusChange1(object sender, OpcUaStatusEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    M_OpcUaClient_OpcStatusChange1(sender, e);
                }));
                return;
            }

            if (e.Error)
            {
                toolStripStatusLabel1.BackColor = Color.Red;
            }
            else
            {
                toolStripStatusLabel1.BackColor = SystemColors.Control;
            }
            toolStripStatusLabel_opc.Text = e.ToString();
        }

        /// <summary>
        /// 连接服务器结束后马上浏览根节点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void M_OpcUaClient_ConnectComplete(object sender, EventArgs e)
        {
            try
            {
                // populate the browse view.
                PopulateBranch(ObjectIds.ObjectsFolder, BrowseNodesTV.Nodes);
                BrowseNodesTV.Enabled = true;
            }
            catch (Exception exception)
            {
                ClientUtils.HandleException(Text, exception);
            }
        }

        #endregion OpcUaClient回调函数

        #endregion OpcUaClient初始化

        //=================================================================================

        #region Winform中控件内置的回调函数

        /// <summary>
        /// Form2加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form2_Load(object sender, EventArgs e)
        {
            _uri = textBox1.Text;
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
                m_OpcUaClient.ConnectServer(_uri);
                button1.BackColor = Color.LimeGreen;
            }
            catch (Exception ex)
            {
                ClientUtils.HandleException(Text, ex);
            }
        }

        /// <summary>
        /// 展示内置客户端按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            using (FormBrowseServer form = new FormBrowseServer())
            {
                form.ShowDialog();
            }
        }

        /// <summary>
        /// texBox1(服务器IP地址)改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            _uri = textBox1.Text;
        }

        /// <summary>
        /// 读取数据按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            OpcNodeAttribute[] t = m_OpcUaClient.ReadNoteAttributes("ns=2;s=Robot1_Axis1");
            label2.Text = t[2].Value.ToString();
            label3.Text = m_OpcUaClient.ReadNode("ns=2;s=Robot1_Axis1").Value.ToString();
        }

        /// <summary>
        /// 数据订阅按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            label4.Text = m_OpcUaClient.ReadNoteAttributes("ns=2;s=Robot1_Axis2")[2].Value.ToString();

            _monitorAction += MonitorTestValueFloat;
            m_OpcUaClient.MonitorValue("ns=2;s=Robot1_Axis2", _monitorAction);
            button4.Enabled = false;
        }

        /// <summary>
        /// Form2界面关闭完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            _action?.Invoke();
            m_OpcUaClient.Disconnect();
            if (_mqttClient.IsConnected) _mqttClient.Disconnect();
        }

        #endregion Winform中控件内置的回调函数

        //=================================================================================

        #region ThePublicMethod

        /// <summary>
        /// 创建自己的数据订阅器
        /// </summary>
        public void CreateUserSubscription()
        {
            // 创建一个数据订阅任务，ps：一个订阅任务可以订阅多个子项
            var sub = new Subscription
            {
                PublishingInterval = 0,
                PublishingEnabled = true,
                LifetimeCount = 0,
                KeepAliveCount = 0,
                DisplayName = "订阅显示的名称",
                Priority = byte.MaxValue
            };

            // 在这里添加数据订阅的节点信息
            var item = new MonitoredItem
            {
                StartNodeId = "[节点名称]",
                AttributeId = Attributes.Value, // 订阅的数据是值类型
                DisplayName = "[节点的显示名称]",
                SamplingInterval = 100 // 间隔时间
            };
            sub.AddItem(item);
            m_OpcUaClient.Session.AddSubscription(sub); // 添加到总客户端,m_OpcUaClient为客户端对象
            sub.Create();
            sub.ApplyChanges();

            // 添加子项处理逻辑
            item.Notification += (monitoredItem, args) =>
            {
                var notification = (MonitoredItemNotification)args.NotificationValue;
                if (notification == null) return; //如果为空就退出
                var t = notification.Value.WrappedValue.Value;

                // 到这里我们就获取到了值，这是是子项一的监视值
            };
        }

        /// <summary>
        /// 填入分支，Populates the branch in the tree view.
        /// </summary>
        /// <param name="sourceId">The NodeId of the Node to browse.</param>
        /// <param name="nodes">The node collect to populate.</param>
        private async void PopulateBranch(NodeId sourceId, TreeNodeCollection nodes)
        {
            nodes.Clear();
            nodes.Add(new TreeNode("Browsering...", 7, 7));
            // fetch references from the server.
            TreeNode[] listNode = await Task.Run(() =>
            {
                ReferenceDescriptionCollection references = GetReferenceDescriptionCollection(sourceId);
                List<TreeNode> list = new List<TreeNode>();
                if (references != null)
                {
                    // process results.
                    for (int ii = 0; ii < references.Count; ii++)
                    {
                        ReferenceDescription target = references[ii];
                        TreeNode child = new TreeNode(Utils.Format("{0}", target));

                        child.Tag = target;
                        string key = GetImageKeyFromDescription(target, sourceId);
                        child.ImageKey = key;
                        child.SelectedImageKey = key;

                        // if (target.NodeClass == NodeClass.Object || target.NodeClass == NodeClass.Unspecified || expanded)
                        // {
                        //     child.Nodes.Add(new TreeNode());
                        // }

                        if (GetReferenceDescriptionCollection((NodeId)target.NodeId).Count > 0)
                        {
                            child.Nodes.Add(new TreeNode());
                        }

                        list.Add(child);
                    }
                }

                return list.ToArray();
            });

            // update the attributes display.
            // DisplayAttributes(sourceId);
            nodes.Clear();
            nodes.AddRange(listNode.ToArray());
        }

        private ReferenceDescriptionCollection GetReferenceDescriptionCollection(NodeId sourceId)
        {
            TaskCompletionSource<ReferenceDescriptionCollection> task =
                new TaskCompletionSource<ReferenceDescriptionCollection>();

            // find all of the components of the node.
            BrowseDescription nodeToBrowse1 = new BrowseDescription();

            nodeToBrowse1.NodeId = sourceId;
            nodeToBrowse1.BrowseDirection = BrowseDirection.Forward;
            nodeToBrowse1.ReferenceTypeId = ReferenceTypeIds.Aggregates;
            nodeToBrowse1.IncludeSubtypes = true;
            nodeToBrowse1.NodeClassMask =
                (uint)
                (NodeClass.Object | NodeClass.Variable | NodeClass.Method | NodeClass.ReferenceType |
                 NodeClass.ObjectType | NodeClass.View | NodeClass.VariableType | NodeClass.DataType);
            nodeToBrowse1.ResultMask = (uint)BrowseResultMask.All;

            // find all nodes organized by the node.
            BrowseDescription nodeToBrowse2 = new BrowseDescription();

            nodeToBrowse2.NodeId = sourceId;
            nodeToBrowse2.BrowseDirection = BrowseDirection.Forward;
            nodeToBrowse2.ReferenceTypeId = ReferenceTypeIds.Organizes;
            nodeToBrowse2.IncludeSubtypes = true;
            nodeToBrowse2.NodeClassMask =
                (uint)
                (NodeClass.Object | NodeClass.Variable | NodeClass.Method | NodeClass.View | NodeClass.ReferenceType |
                 NodeClass.ObjectType | NodeClass.VariableType | NodeClass.DataType);
            nodeToBrowse2.ResultMask = (uint)BrowseResultMask.All;

            BrowseDescriptionCollection nodesToBrowse = new BrowseDescriptionCollection();
            nodesToBrowse.Add(nodeToBrowse1);
            nodesToBrowse.Add(nodeToBrowse2);

            // fetch references from the server.
            ReferenceDescriptionCollection references = FormUtils.Browse(m_OpcUaClient.Session, nodesToBrowse, false);
            return references;
        }

        private string GetImageKeyFromDescription(ReferenceDescription target, NodeId sourceId)
        {
            if (target.NodeClass == NodeClass.Variable)
            {
                DataValue dataValue = m_OpcUaClient.ReadNode((NodeId)target.NodeId);

                if (dataValue.WrappedValue.TypeInfo != null)
                {
                    if (dataValue.WrappedValue.TypeInfo.ValueRank == ValueRanks.Scalar)
                    {
                        return "Enum_582";
                    }
                    else if (dataValue.WrappedValue.TypeInfo.ValueRank == ValueRanks.OneDimension)
                    {
                        return "brackets";
                    }
                    else if (dataValue.WrappedValue.TypeInfo.ValueRank == ValueRanks.TwoDimensions)
                    {
                        return "Module_648";
                    }
                    else
                    {
                        return "ClassIcon";
                    }
                }
                else
                {
                    return "ClassIcon";
                }
            }
            else if (target.NodeClass == NodeClass.Object)
            {
                if (sourceId == ObjectIds.ObjectsFolder)
                {
                    return "VirtualMachine";
                }
                else
                {
                    return "ClassIcon";
                }
            }
            else if (target.NodeClass == NodeClass.Method)
            {
                return "Method_636";
            }
            else
            {
                return "ClassIcon";
            }
        }

        #endregion ThePublicMethod

        #region Help Function

        /// <summary>
        /// 订阅数据函数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="action"></param>
        private void MonitorTestValueFloat(float value, Action action)
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
            label5.Text = value.ToString();
            // 如果需要停止，调用action即可
            // action?.Invoke();
            _action = action;
            //向MQTT服务器端发布消息
            _mqttClient.Publish("opcua", Encoding.UTF8.GetBytes(value.ToString()));
        }

        #endregion Help Function
    }
}