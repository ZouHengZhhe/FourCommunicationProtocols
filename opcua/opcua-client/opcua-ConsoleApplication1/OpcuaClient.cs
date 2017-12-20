using Opc.Ua.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using System.ComponentModel;
using OpcUaHelper;

namespace opcua_ConsoleApplication1
{
    public class OpcuaClient
    {
        private int m_reconnectPeriod = 10;
        private ApplicationInstance application;
        private ApplicationConfiguration m_configuration;
        private Session m_session;
        private bool m_IsConnected;
        private bool m_useSecurity;
        private SessionReconnectHandler m_reconnectHandler;
        private EventHandler m_ReconnectComplete;
        private EventHandler m_ReconnectStarting;
        private EventHandler m_KeepAliveComplete;
        private EventHandler m_ConnectComplete;
        private EventHandler<OpcuaStatusEventArgs> m_OpcStatusChange;

        /// <summary>a name of application name show on server</summary>
        public string OpcUaName { get; set; } = "Zhhe Opcua";

        /// <summary>Whether to use security when connecting.</summary>
        public bool UseSecurity
        {
            get
            {
                return this.m_useSecurity;
            }
            set
            {
                this.m_useSecurity = value;
            }
        }

        /// <summary>The user identity to use when creating the session.</summary>
        public IUserIdentity UserIdentity { get; set; }

        /// <summary>The currently active session.</summary>
        public Session Session
        {
            get
            {
                return this.m_session;
            }
        }

        /// <summary>Indicate the connect status</summary>
        public bool Connected
        {
            get
            {
                return this.m_IsConnected;
            }
        }

        /// <summary>
        /// The number of seconds between reconnect attempts (0 means reconnect is disabled).
        /// </summary>
        [DefaultValue(10)]
        public int ReconnectPeriod
        {
            get
            {
                return this.m_reconnectPeriod;
            }
            set
            {
                this.m_reconnectPeriod = value;
            }
        }

        /// <summary>配置信息</summary>
        public ApplicationConfiguration AppConfig
        {
            get
            {
                return this.m_configuration;
            }
        }

        /// <summary>
        /// Raised when a good keep alive from the server arrives.
        /// </summary>
        public event EventHandler KeepAliveComplete
        {
            add
            {
                this.m_KeepAliveComplete = this.m_KeepAliveComplete + value;
            }
            remove
            {
                this.m_KeepAliveComplete = this.m_KeepAliveComplete - value;
            }
        }

        /// <summary>Raised when a reconnect operation starts.</summary>
        public event EventHandler ReconnectStarting
        {
            add
            {
                this.m_ReconnectStarting = this.m_ReconnectStarting + value;
            }
            remove
            {
                this.m_ReconnectStarting = this.m_ReconnectStarting - value;
            }
        }

        /// <summary>Raised when a reconnect operation completes.</summary>
        public event EventHandler ReconnectComplete
        {
            add
            {
                this.m_ReconnectComplete = this.m_ReconnectComplete + value;
            }
            remove
            {
                this.m_ReconnectComplete = this.m_ReconnectComplete - value;
            }
        }

        /// <summary>
        /// Raised after successfully connecting to or disconnecing from a server.
        /// </summary>
        public event EventHandler ConnectComplete
        {
            add
            {
                this.m_ConnectComplete = this.m_ConnectComplete + value;
            }
            remove
            {
                this.m_ConnectComplete = this.m_ConnectComplete - value;
            }
        }

        /// <summary>Raised after the client status change</summary>
        public event EventHandler<OpcuaStatusEventArgs> OpcStatusChange
        {
            add
            {
                this.m_OpcStatusChange = this.m_OpcStatusChange + value;
            }
            remove
            {
                this.m_OpcStatusChange = this.m_OpcStatusChange - value;
            }
        }

        /// <summary>Constructors method</summary>
        public OpcuaClient()
        {
            CertificateValidator certificateValidator1 = new CertificateValidator();
            certificateValidator1.CertificateValidation += (CertificateValidationEventHandler)((sender, eventArgs) =>
            {
                if (ServiceResult.IsGood(eventArgs.Error))
                {
                    eventArgs.Accept = true;
                }
                else
                {
                    if ((int)eventArgs.Error.StatusCode.Code != -2145779712)
                        throw new Exception(string.Format("Failed to validate certificate with error code {0}: {1}", (object)eventArgs.Error.Code, (object)eventArgs.Error.AdditionalInfo));
                    eventArgs.Accept = true;
                }
            });
            ApplicationInstance applicationInstance = new ApplicationInstance();
            applicationInstance.ApplicationType = ApplicationType.Client;
            string opcUaName1 = this.OpcUaName;
            applicationInstance.ConfigSectionName = opcUaName1;
            ApplicationConfiguration applicationConfiguration = new ApplicationConfiguration();
            applicationConfiguration.ApplicationUri = "";
            string opcUaName2 = this.OpcUaName;
            applicationConfiguration.ApplicationName = opcUaName2;
            int num1 = 1;
            applicationConfiguration.ApplicationType = (ApplicationType)num1;
            CertificateValidator certificateValidator2 = certificateValidator1;
            applicationConfiguration.CertificateValidator = certificateValidator2;
            applicationConfiguration.ServerConfiguration = new ServerConfiguration()
            {
                MaxSubscriptionCount = 100,
                MaxMessageQueueSize = 100,
                MaxNotificationQueueSize = 100,
                MaxPublishRequestCount = 100
            };
            applicationConfiguration.SecurityConfiguration = new SecurityConfiguration()
            {
                AutoAcceptUntrustedCertificates = true
            };
            applicationConfiguration.TransportQuotas = new TransportQuotas()
            {
                OperationTimeout = 600000,
                MaxStringLength = 1048576,
                MaxByteStringLength = 1048576,
                MaxArrayLength = (int)ushort.MaxValue,
                MaxMessageSize = 4194304,
                MaxBufferSize = (int)ushort.MaxValue,
                ChannelLifetime = 600000,
                SecurityTokenLifetime = 3600000
            };
            applicationConfiguration.ClientConfiguration = new ClientConfiguration()
            {
                DefaultSessionTimeout = 60000,
                MinSubscriptionLifetime = 10000
            };
            int num2 = 1;
            applicationConfiguration.DisableHiResClock = num2 != 0;
            applicationInstance.ApplicationConfiguration = applicationConfiguration;
            this.application = applicationInstance;
            this.m_configuration = this.application.ApplicationConfiguration;
        }

        /// <summary>connect to server</summary>
        /// <param name="serverUrl"></param>
        public void ConnectServer(string serverUrl)
        {
            this.m_session = this.Connect(serverUrl);
        }

        /// <summary>Creates a new session.</summary>
        /// <returns>The new session object.</returns>
        private Session Connect(string serverUrl)
        {
            this.Disconnect();
            if (this.m_configuration == null)
                throw new ArgumentNullException("m_configuration");
            this.m_session = Session.Create(this.m_configuration, new ConfiguredEndpoint((ConfiguredEndpointCollection)null, Opc.Ua.Client.Controls.ClientUtils.SelectEndpoint(serverUrl, this.UseSecurity), EndpointConfiguration.Create(this.m_configuration)), false, false, string.IsNullOrEmpty(this.OpcUaName) ? this.m_configuration.ApplicationName : this.OpcUaName, 60000U, this.UserIdentity, (IList<string>)new string[0]);
            this.m_session.KeepAlive += new KeepAliveEventHandler(this.Session_KeepAlive);
            this.m_IsConnected = true;
            this.DoConnectComplete((object)null);
            return this.m_session;
        }

        /// <summary>Disconnects from the server.</summary>
        public void Disconnect()
        {
            this.UpdateStatus(false, DateTime.UtcNow, "Disconnected");
            if (this.m_reconnectHandler != null)
            {
                this.m_reconnectHandler.Dispose();
                this.m_reconnectHandler = (SessionReconnectHandler)null;
            }
            if (this.m_session != null)
            {
                this.m_session.Close(10000);
                this.m_session = (Session)null;
            }
            this.m_IsConnected = false;
            this.DoConnectComplete((object)null);
        }

        /// <summary>Report the client status</summary>
        /// <param name="error">Whether the status represents an error.</param>
        /// <param name="time">The time associated with the status.</param>
        /// <param name="status">The status message.</param>
        /// <param name="args">Arguments used to format the status message.</param>
        private void UpdateStatus(bool error, DateTime time, string status, params object[] args)
        {
            EventHandler<OpcuaStatusEventArgs> opcStatusChange = this.m_OpcStatusChange;
            if (opcStatusChange == null)
                return;
            OpcuaStatusEventArgs e = new OpcuaStatusEventArgs();
            e.Error = error;
            DateTime localTime = time.ToLocalTime();
            e.Time = localTime;
            string str = string.Format(status, args);
            e.Text = str;
            opcStatusChange((object)this, e);
        }

        /// <summary>Handles a keep alive event from a session.</summary>
        private void Session_KeepAlive(Session session, KeepAliveEventArgs e)
        {
            try
            {
                if (session != this.m_session)
                    return;
                if (ServiceResult.IsBad(e.Status))
                {
                    if (this.m_reconnectPeriod <= 0)
                    {
                        this.UpdateStatus(1 != 0, e.CurrentTime, "Communication Error ({0})", (object)e.Status);
                    }
                    else
                    {
                        this.UpdateStatus(1 != 0, e.CurrentTime, "Reconnecting in {0}s", (object)this.m_reconnectPeriod);
                        if (this.m_reconnectHandler != null)
                            return;
                        EventHandler reconnectStarting = this.m_ReconnectStarting;
                        if (reconnectStarting != null)
                        {
                            KeepAliveEventArgs keepAliveEventArgs = e;
                            reconnectStarting((object)this, (EventArgs)keepAliveEventArgs);
                        }
                        this.m_reconnectHandler = new SessionReconnectHandler();
                        this.m_reconnectHandler.BeginReconnect(this.m_session, this.m_reconnectPeriod * 1000, new EventHandler(this.Server_ReconnectComplete));
                    }
                }
                else
                {
                    this.UpdateStatus(0 != 0, e.CurrentTime, "Connected [{0}]", (object)session.Endpoint.EndpointUrl);
                    EventHandler keepAliveComplete = this.m_KeepAliveComplete;
                    if (keepAliveComplete == null)
                        return;
                    KeepAliveEventArgs keepAliveEventArgs = e;
                    keepAliveComplete((object)this, (EventArgs)keepAliveEventArgs);
                }
            }
            catch (Exception ex)
            {
                Opc.Ua.Client.Controls.ClientUtils.HandleException(this.OpcUaName, ex);
            }
        }

        /// <summary>
        /// Handles a reconnect event complete from the reconnect handler.
        /// </summary>
        private void Server_ReconnectComplete(object sender, EventArgs e)
        {
            try
            {
                if (sender != this.m_reconnectHandler)
                    return;
                this.m_session = this.m_reconnectHandler.Session;
                this.m_reconnectHandler.Dispose();
                this.m_reconnectHandler = (SessionReconnectHandler)null;
                EventHandler reconnectComplete = this.m_ReconnectComplete;
                if (reconnectComplete == null)
                    return;
                EventArgs e1 = e;
                reconnectComplete((object)this, e1);
            }
            catch (Exception ex)
            {
                Opc.Ua.Client.Controls.ClientUtils.HandleException(this.OpcUaName, ex);
            }
        }

        /// <summary>设置OPC客户端的日志输出</summary>
        /// <param name="filePath">完整的文件路径</param>
        /// <param name="deleteExisting">是否删除原文件</param>
        public void SetLogPathName(string filePath, bool deleteExisting)
        {
            Utils.SetTraceLog(filePath, deleteExisting);
            Utils.SetTraceMask(515);
        }

        /// <summary>Read a value node from server</summary>
        /// <param name="nodeId">node id</param>
        /// <returns></returns>
        public DataValue ReadNode(NodeId nodeId)
        {
            ReadValueId readValueId1 = new ReadValueId()
            {
                NodeId = nodeId,
                AttributeId = 13
            };
            ReadValueIdCollection valueIdCollection = new ReadValueIdCollection();
            ReadValueId readValueId2 = readValueId1;
            valueIdCollection.Add(readValueId2);
            ReadValueIdCollection nodesToRead = valueIdCollection;
            DataValueCollection results;
            DiagnosticInfoCollection diagnosticInfos;
            this.m_session.Read((RequestHeader)null, 0.0, TimestampsToReturn.Neither, nodesToRead, out results, out diagnosticInfos);
            ClientBase.ValidateResponse((IList)results, (IList)nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, (IList)nodesToRead);
            return results[0];
        }

        /// <summary>Read a value node from server</summary>
        /// <typeparam name="T">type of value</typeparam>
        /// <param name="tag">node id</param>
        /// <returns></returns>
        public T ReadNode<T>(string tag)
        {
            ReadValueId readValueId1 = new ReadValueId()
            {
                NodeId = new NodeId(tag),
                AttributeId = 13
            };
            ReadValueIdCollection valueIdCollection = new ReadValueIdCollection();
            ReadValueId readValueId2 = readValueId1;
            valueIdCollection.Add(readValueId2);
            ReadValueIdCollection nodesToRead = valueIdCollection;
            DataValueCollection results;
            DiagnosticInfoCollection diagnosticInfos;
            this.m_session.Read((RequestHeader)null, 0.0, TimestampsToReturn.Neither, nodesToRead, out results, out diagnosticInfos);
            ClientBase.ValidateResponse((IList)results, (IList)nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, (IList)nodesToRead);
            return (T)results[0].Value;
        }

        /// <summary>Read a tag asynchronously</summary>
        /// <typeparam name="T">The type of tag to read</typeparam>
        /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
        /// E.g: the tag `foo.bar` reads the tag `bar` on the folder `foo`</param>
        /// <returns>The value retrieved from the OPC</returns>
        public Task<T> ReadNodeAsync<T>(string tag)
        {
            ReadValueIdCollection valueIdCollection = new ReadValueIdCollection();
            valueIdCollection.Add(new ReadValueId()
            {
                NodeId = new NodeId(tag),
                AttributeId = 13U
            });
            ReadValueIdCollection nodesToRead = valueIdCollection;
            TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();
            this.m_session.BeginRead((RequestHeader)null, 0.0, TimestampsToReturn.Neither, nodesToRead, (AsyncCallback)(ar =>
            {
                DataValueCollection results;
                DiagnosticInfoCollection diagnosticInfos;
                ResponseHeader responseHeader = this.m_session.EndRead(ar, out results, out diagnosticInfos);
                try
                {
                    this.CheckReturnValue(responseHeader.ServiceResult);
                    this.CheckReturnValue(results[0].StatusCode);
                    taskCompletionSource.TrySetResult((T)results[0].Value);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.TrySetException(ex);
                }
            }), (object)null);
            return taskCompletionSource.Task;
        }

        /// <summary>read several value nodes from server</summary>
        /// <param name="Tags">all Tags</param>
        /// <returns></returns>
        public IEnumerable<DataValue> ReadNodes(string[] Tags)
        {
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
            for (int i = 0; i < Tags.Length; ++i)
                nodesToRead.Add(new ReadValueId()
                {
                    NodeId = new NodeId(Tags[i]),
                    AttributeId = 13U
                });
            DataValueCollection results;
            DiagnosticInfoCollection diagnosticInfos;
            this.m_session.Read((RequestHeader)null, 0.0, TimestampsToReturn.Neither, nodesToRead, out results, out diagnosticInfos);
            ClientBase.ValidateResponse((IList)results, (IList)nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, (IList)nodesToRead);
            foreach (DataValue dataValue in (List<DataValue>)results)
            {
                DataValue v = dataValue;
                yield return v;
                v = (DataValue)null;
            }
            List<DataValue>.Enumerator enumerator = new List<DataValue>.Enumerator();
        }

        /// <summary>write a note to server(you should use try catch)</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tag"></param>
        /// <param name="value"></param>
        /// <returns>if success True,otherwise False</returns>
        public bool WriteNode<T>(string tag, T value)
        {
            WriteValue writeValue1 = new WriteValue()
            {
                NodeId = new NodeId(tag),
                AttributeId = 13
            };
            writeValue1.Value.Value = (object)value;
            writeValue1.Value.StatusCode = (StatusCode)0U;
            writeValue1.Value.ServerTimestamp = DateTime.MinValue;
            writeValue1.Value.SourceTimestamp = DateTime.MinValue;
            WriteValueCollection writeValueCollection = new WriteValueCollection();
            WriteValue writeValue2 = writeValue1;
            writeValueCollection.Add(writeValue2);
            WriteValueCollection nodesToWrite = writeValueCollection;
            StatusCodeCollection results;
            DiagnosticInfoCollection diagnosticInfos;
            this.m_session.Write((RequestHeader)null, nodesToWrite, out results, out diagnosticInfos);
            ClientBase.ValidateResponse((IList)results, (IList)nodesToWrite);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, (IList)nodesToWrite);
            return !StatusCode.IsBad(results[0]);
        }

        /// <summary>Write a value on the specified opc tag asynchronously</summary>
        /// <typeparam name="T">The type of tag to write on</typeparam>
        /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name.
        /// E.g: the tag `foo.bar` writes on the tag `bar` on the folder `foo`</param>
        /// <param name="value">The value for the item to write</param>
        public Task<bool> WriteNodeAsync<T>(string tag, T value)
        {
            WriteValue writeValue1 = new WriteValue()
            {
                NodeId = new NodeId(tag),
                AttributeId = 13
            };
            writeValue1.Value.Value = (object)value;
            writeValue1.Value.StatusCode = (StatusCode)0U;
            writeValue1.Value.ServerTimestamp = DateTime.MinValue;
            writeValue1.Value.SourceTimestamp = DateTime.MinValue;
            WriteValueCollection writeValueCollection = new WriteValueCollection();
            WriteValue writeValue2 = writeValue1;
            writeValueCollection.Add(writeValue2);
            WriteValueCollection valuesToWrite = writeValueCollection;
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            this.m_session.BeginWrite((RequestHeader)null, valuesToWrite, (AsyncCallback)(ar =>
            {
                StatusCodeCollection results;
                DiagnosticInfoCollection diagnosticInfos;
                this.m_session.EndWrite(ar, out results, out diagnosticInfos);
                try
                {
                    ClientBase.ValidateResponse((IList)results, (IList)valuesToWrite);
                    ClientBase.ValidateDiagnosticInfos(diagnosticInfos, (IList)valuesToWrite);
                    taskCompletionSource.SetResult(StatusCode.IsGood(results[0]));
                }
                catch (Exception ex)
                {
                    taskCompletionSource.TrySetException(ex);
                }
            }), (object)null);
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 所有的节点都写入成功，返回<c>True</c>，否则返回<c>False</c>
        /// </summary>
        /// <param name="tags">节点名称数组</param>
        /// <param name="values">节点的值数据</param>
        /// <returns></returns>
        public bool WriteNodes(string[] tags, object[] values)
        {
            WriteValueCollection nodesToWrite = new WriteValueCollection();
            for (int index = 0; index < tags.Length; ++index)
            {
                if (index < values.Length)
                {
                    WriteValue writeValue = new WriteValue()
                    {
                        NodeId = new NodeId(tags[index]),
                        AttributeId = 13
                    };
                    writeValue.Value.Value = values[index];
                    writeValue.Value.StatusCode = (StatusCode)0U;
                    writeValue.Value.ServerTimestamp = DateTime.MinValue;
                    writeValue.Value.SourceTimestamp = DateTime.MinValue;
                    nodesToWrite.Add(writeValue);
                }
            }
            StatusCodeCollection results;
            DiagnosticInfoCollection diagnosticInfos;
            this.m_session.Write((RequestHeader)null, nodesToWrite, out results, out diagnosticInfos);
            ClientBase.ValidateResponse((IList)results, (IList)nodesToWrite);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, (IList)nodesToWrite);
            bool flag = true;
            foreach (StatusCode code in (List<StatusCode>)results)
            {
                if (StatusCode.IsBad(code))
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }

        /// <summary>
        /// 删除一个节点的操作，除非服务器配置允许，否则引发异常，成功返回<c>True</c>，否则返回<c>False</c>
        /// </summary>
        /// <param name="tag">节点文本描述</param>
        /// <returns></returns>
        public bool DeleteExsistNode(string tag)
        {
            DeleteNodesItemCollection nodesToDelete = new DeleteNodesItemCollection();
            new DeleteNodesItem().NodeId = new NodeId(tag);
            StatusCodeCollection results;
            DiagnosticInfoCollection diagnosticInfos;
            this.m_session.DeleteNodes((RequestHeader)null, nodesToDelete, out results, out diagnosticInfos);
            ClientBase.ValidateResponse((IList)results, (IList)nodesToDelete);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, (IList)nodesToDelete);
            return !StatusCode.IsBad(results[0]);
        }

        /// <summary>新增一个节点数据</summary>
        /// <param name="parent">父节点tag名称</param>
        [Obsolete("还未经过测试，无法使用")]
        public void AddNewNode(NodeId parent)
        {
            AddNodesItem addNodesItem1 = new AddNodesItem();
            addNodesItem1.ParentNodeId = (ExpandedNodeId)new NodeId(parent);
            addNodesItem1.ReferenceTypeId = (NodeId)47U;
            addNodesItem1.RequestedNewNodeId = (ExpandedNodeId)null;
            addNodesItem1.BrowseName = new QualifiedName("DataVariable1");
            addNodesItem1.NodeClass = NodeClass.Variable;
            addNodesItem1.NodeAttributes = (ExtensionObject)null;
            addNodesItem1.TypeDefinition = (ExpandedNodeId)VariableTypeIds.BaseDataVariableType;
            VariableAttributes variableAttributes = new VariableAttributes();
            variableAttributes.DisplayName = (LocalizedText)"DataVariable1";
            variableAttributes.Description = (LocalizedText)"DataVariable1 Description";
            variableAttributes.Value = new Opc.Ua.Variant(123);
            variableAttributes.DataType = (NodeId)6U;
            variableAttributes.ValueRank = -1;
            variableAttributes.ArrayDimensions = new UInt32Collection();
            variableAttributes.AccessLevel = (byte)3;
            variableAttributes.UserAccessLevel = (byte)3;
            variableAttributes.MinimumSamplingInterval = 0.0;
            variableAttributes.Historizing = false;
            variableAttributes.WriteMask = 0U;
            variableAttributes.UserWriteMask = 0U;
            variableAttributes.SpecifiedAttributes = 4194303U;
            addNodesItem1.NodeAttributes = new ExtensionObject((object)variableAttributes);
            AddNodesItemCollection nodesItemCollection = new AddNodesItemCollection();
            AddNodesItem addNodesItem2 = addNodesItem1;
            nodesItemCollection.Add(addNodesItem2);
            AddNodesItemCollection nodesToAdd = nodesItemCollection;
            AddNodesResultCollection results;
            DiagnosticInfoCollection diagnosticInfos;
            this.m_session.AddNodes((RequestHeader)null, nodesToAdd, out results, out diagnosticInfos);
            ClientBase.ValidateResponse((IList)results, (IList)nodesToAdd);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, (IList)nodesToAdd);
        }

        /// <summary>Monitor a value from server</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tag"></param>
        /// <param name="callback"></param>
        public void MonitorValue<T>(string tag, Action<T, Action> callback)
        {
            NodeId nodeId = new NodeId(tag);
            Subscription sub = new Subscription()
            {
                PublishingInterval = 0,
                PublishingEnabled = true,
                LifetimeCount = 0,
                KeepAliveCount = 0,
                DisplayName = tag,
                Priority = byte.MaxValue
            };
            MonitoredItem monitoredItem1 = new MonitoredItem()
            {
                StartNodeId = nodeId,
                AttributeId = 13,
                DisplayName = tag,
                SamplingInterval = 100
            };
            sub.AddItem(monitoredItem1);
            this.m_session.AddSubscription(sub);
            sub.Create();
            sub.ApplyChanges();
            monitoredItem1.Notification += (MonitoredItemNotificationEventHandler)((monitoredItem, args) =>
            {
                MonitoredItemNotification notificationValue = (MonitoredItemNotification)args.NotificationValue;
                if (notificationValue == null)
                    return;
                object obj1 = notificationValue.Value.WrappedValue.Value;
                Action action1 = (Action)(() =>
                {
                    sub.RemoveItems(sub.MonitoredItems);
                    sub.Delete(true);
                    this.m_session.RemoveSubscription(sub);
                    sub.Dispose();
                });
                Action<T, Action> action2 = callback;
                if (action2 == null)
                    return;
                T obj2 = (T)obj1;
                Action action3 = action1;
                action2(obj2, action3);
            });
        }

        /// <summary>read History data</summary>
        /// <param name="tag">节点的索引</param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="count">读取的个数</param>
        /// <param name="containBound">是否包含边界</param>
        /// <returns></returns>
        public IEnumerable<DataValue> ReadHistoryRawDataValues(string tag, DateTime start, DateTime end, uint count = 1, bool containBound = false)
        {
            HistoryReadValueId m_nodeToContinue = new HistoryReadValueId()
            {
                NodeId = new NodeId(tag)
            };
            ReadRawModifiedDetails rawModifiedDetails = new ReadRawModifiedDetails();
            rawModifiedDetails.StartTime = start;
            rawModifiedDetails.EndTime = end;
            rawModifiedDetails.NumValuesPerNode = count;
            int num1 = 0;
            rawModifiedDetails.IsReadModified = num1 != 0;
            int num2 = containBound ? 1 : 0;
            rawModifiedDetails.ReturnBounds = num2 != 0;
            ReadRawModifiedDetails m_details = rawModifiedDetails;
            HistoryReadValueIdCollection nodesToRead = new HistoryReadValueIdCollection();
            nodesToRead.Add(m_nodeToContinue);
            HistoryReadResultCollection results;
            DiagnosticInfoCollection diagnosticInfos;
            this.m_session.HistoryRead((RequestHeader)null, new ExtensionObject((object)m_details), TimestampsToReturn.Both, false, nodesToRead, out results, out diagnosticInfos);
            ClientBase.ValidateResponse((IList)results, (IList)nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, (IList)nodesToRead);
            if (StatusCode.IsBad(results[0].StatusCode))
                throw new ServiceResultException((ServiceResult)results[0].StatusCode);
            HistoryData values = ExtensionObject.ToEncodeable(results[0].HistoryData) as HistoryData;
            foreach (DataValue dataValue in (List<DataValue>)values.DataValues)
            {
                DataValue value = dataValue;
                yield return value;
                value = (DataValue)null;
            }
            List<DataValue>.Enumerator enumerator = new List<DataValue>.Enumerator();
        }

        /// <summary>读取一连串的历史数据，并将其转化成指定的类型</summary>
        /// <param name="url">节点的索引</param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="count">读取的个数</param>
        /// <param name="containBound">是否包含边界</param>
        /// <returns></returns>
        public IEnumerable<T> ReadHistoryRawDataValues<T>(string url, DateTime start, DateTime end, uint count = 1, bool containBound = false)
        {
            HistoryReadValueId m_nodeToContinue = new HistoryReadValueId()
            {
                NodeId = new NodeId(url)
            };
            ReadRawModifiedDetails rawModifiedDetails = new ReadRawModifiedDetails();
            rawModifiedDetails.StartTime = start.ToUniversalTime();
            rawModifiedDetails.EndTime = end.ToUniversalTime();
            rawModifiedDetails.NumValuesPerNode = count;
            int num1 = 0;
            rawModifiedDetails.IsReadModified = num1 != 0;
            int num2 = containBound ? 1 : 0;
            rawModifiedDetails.ReturnBounds = num2 != 0;
            ReadRawModifiedDetails m_details = rawModifiedDetails;
            HistoryReadValueIdCollection nodesToRead = new HistoryReadValueIdCollection();
            nodesToRead.Add(m_nodeToContinue);
            HistoryReadResultCollection results;
            DiagnosticInfoCollection diagnosticInfos;
            this.m_session.HistoryRead((RequestHeader)null, new ExtensionObject((object)m_details), TimestampsToReturn.Both, false, nodesToRead, out results, out diagnosticInfos);
            ClientBase.ValidateResponse((IList)results, (IList)nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, (IList)nodesToRead);
            if (StatusCode.IsBad(results[0].StatusCode))
                throw new ServiceResultException((ServiceResult)results[0].StatusCode);
            HistoryData values = ExtensionObject.ToEncodeable(results[0].HistoryData) as HistoryData;
            foreach (DataValue dataValue in (List<DataValue>)values.DataValues)
            {
                DataValue value = dataValue;
                yield return (T)value.Value;
                value = (DataValue)null;
            }
            List<DataValue>.Enumerator enumerator = new List<DataValue>.Enumerator();
        }

        /// <summary>浏览一个节点的引用</summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public ReferenceDescription[] BrowseNodeReference(string tag)
        {
            NodeId nodeId = new NodeId(tag);
            BrowseDescription browseDescription1 = new BrowseDescription();
            browseDescription1.NodeId = nodeId;
            browseDescription1.BrowseDirection = BrowseDirection.Forward;
            browseDescription1.ReferenceTypeId = ReferenceTypeIds.Aggregates;
            browseDescription1.IncludeSubtypes = true;
            browseDescription1.NodeClassMask = 7U;
            browseDescription1.ResultMask = 63U;
            BrowseDescription browseDescription2 = new BrowseDescription();
            browseDescription2.NodeId = nodeId;
            browseDescription2.BrowseDirection = BrowseDirection.Forward;
            browseDescription2.ReferenceTypeId = ReferenceTypeIds.Organizes;
            browseDescription2.IncludeSubtypes = true;
            browseDescription2.NodeClassMask = 3U;
            browseDescription2.ResultMask = 63U;
            BrowseDescriptionCollection nodesToBrowse = new BrowseDescriptionCollection();
            nodesToBrowse.Add(browseDescription1);
            nodesToBrowse.Add(browseDescription2);
            return FormUtils.Browse(this.m_session, nodesToBrowse, false).ToArray();
        }

        /// <summary>读取一个节点的所有属性</summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        //public OpcuaNodeAttribute[] ReadNoteAttributes(string tag)
        //{
        //    NodeId nodeId = new NodeId(tag);
        //    ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
        //    for (uint index = 2; index <= 22U; ++index)
        //        nodesToRead.Add(new ReadValueId()
        //        {
        //            NodeId = nodeId,
        //            AttributeId = index
        //        });
        //    int count = nodesToRead.Count;
        //    BrowseDescription browseDescription = new BrowseDescription();
        //    browseDescription.NodeId = nodeId;
        //    browseDescription.BrowseDirection = BrowseDirection.Forward;
        //    browseDescription.ReferenceTypeId = ReferenceTypeIds.HasProperty;
        //    browseDescription.IncludeSubtypes = true;
        //    browseDescription.NodeClassMask = 0U;
        //    browseDescription.ResultMask = 63U;
        //    BrowseDescriptionCollection nodesToBrowse = new BrowseDescriptionCollection();
        //    nodesToBrowse.Add(browseDescription);
        //    ReferenceDescriptionCollection descriptionCollection = FormUtils.Browse(this.m_session, nodesToBrowse, false);
        //    if (descriptionCollection == null)
        //        return new OpcuaNodeAttribute[0];
        //    for (int index = 0; index < descriptionCollection.Count; ++index)
        //    {
        //        if (!descriptionCollection[index].NodeId.IsAbsolute)
        //            nodesToRead.Add(new ReadValueId()
        //            {
        //                NodeId = (NodeId)descriptionCollection[index].NodeId,
        //                AttributeId = 13U
        //            });
        //    }
        //    DataValueCollection results = (DataValueCollection)null;
        //    DiagnosticInfoCollection diagnosticInfos = (DiagnosticInfoCollection)null;
        //    this.m_session.Read((RequestHeader)null, 0.0, TimestampsToReturn.Neither, nodesToRead, out results, out diagnosticInfos);
        //    ClientBase.ValidateResponse((IList)results, (IList)nodesToRead);
        //    ClientBase.ValidateDiagnosticInfos(diagnosticInfos, (IList)nodesToRead);
        //    List<OpcNodeAttribute> opcNodeAttributeList = new List<OpcNodeAttribute>();
        //    for (int index = 0; index < results.Count; ++index)
        //    {
        //        OpcNodeAttribute opcNodeAttribute1 = new OpcNodeAttribute();
        //        BuiltInType builtInType;
        //        if (index < count)
        //        {
        //            if (!(results[index].StatusCode == 2150957056U))
        //            {
        //                opcNodeAttribute1.Name = Attributes.GetBrowseName(nodesToRead[index].AttributeId);
        //                if (StatusCode.IsBad(results[index].StatusCode))
        //                {
        //                    opcNodeAttribute1.Type = Utils.Format("{0}", (object)Attributes.GetDataTypeId(nodesToRead[index].AttributeId));
        //                    opcNodeAttribute1.Value = (object)Utils.Format("{0}", (object)results[index].StatusCode);
        //                }
        //                else
        //                {
        //                    TypeInfo typeInfo = TypeInfo.Construct(results[index].Value);
        //                    OpcNodeAttribute opcNodeAttribute2 = opcNodeAttribute1;
        //                    builtInType = typeInfo.BuiltInType;
        //                    string str = builtInType.ToString();
        //                    opcNodeAttribute2.Type = str;
        //                    if (typeInfo.ValueRank >= 0)
        //                        opcNodeAttribute1.Type += "[]";
        //                    opcNodeAttribute1.Value = results[index].Value;
        //                }
        //            }
        //            else
        //                continue;
        //        }
        //        else if (!(results[index].StatusCode == 2150891520U))
        //        {
        //            opcNodeAttribute1.Name = Utils.Format("{0}", (object)descriptionCollection[index - count]);
        //            if (StatusCode.IsBad(results[index].StatusCode))
        //            {
        //                opcNodeAttribute1.Type = string.Empty;
        //                opcNodeAttribute1.Value = (object)Utils.Format("{0}", (object)results[index].StatusCode);
        //            }
        //            else
        //            {
        //                TypeInfo typeInfo = TypeInfo.Construct(results[index].Value);
        //                OpcNodeAttribute opcNodeAttribute2 = opcNodeAttribute1;
        //                builtInType = typeInfo.BuiltInType;
        //                string str = builtInType.ToString();
        //                opcNodeAttribute2.Type = str;
        //                if (typeInfo.ValueRank >= 0)
        //                    opcNodeAttribute1.Type += "[]";
        //                opcNodeAttribute1.Value = results[index].Value;
        //            }
        //        }
        //        else
        //            continue;
        //        opcNodeAttributeList.Add(opcNodeAttribute1);
        //    }
        //    return opcNodeAttributeList.ToArray();
        //}

        /// <summary>读取一个节点的所有属性</summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public DataValue[] ReadNoteDataValueAttributes(string tag)
        {
            NodeId nodeId = new NodeId(tag);
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
            for (uint index = 1; index <= 22U; ++index)
                nodesToRead.Add(new ReadValueId()
                {
                    NodeId = nodeId,
                    AttributeId = index
                });
            int count = nodesToRead.Count;
            BrowseDescription browseDescription = new BrowseDescription();
            browseDescription.NodeId = nodeId;
            browseDescription.BrowseDirection = BrowseDirection.Forward;
            browseDescription.ReferenceTypeId = ReferenceTypeIds.HasProperty;
            browseDescription.IncludeSubtypes = true;
            browseDescription.NodeClassMask = 0U;
            browseDescription.ResultMask = 63U;
            BrowseDescriptionCollection nodesToBrowse = new BrowseDescriptionCollection();
            nodesToBrowse.Add(browseDescription);
            ReferenceDescriptionCollection descriptionCollection = FormUtils.Browse(this.m_session, nodesToBrowse, false);
            if (descriptionCollection == null)
                return new DataValue[0];
            for (int index = 0; index < descriptionCollection.Count; ++index)
            {
                if (!descriptionCollection[index].NodeId.IsAbsolute)
                    nodesToRead.Add(new ReadValueId()
                    {
                        NodeId = (NodeId)descriptionCollection[index].NodeId,
                        AttributeId = 13U
                    });
            }
            DataValueCollection results = (DataValueCollection)null;
            DiagnosticInfoCollection diagnosticInfos = (DiagnosticInfoCollection)null;
            this.m_session.Read((RequestHeader)null, 0.0, TimestampsToReturn.Neither, nodesToRead, out results, out diagnosticInfos);
            ClientBase.ValidateResponse((IList)results, (IList)nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, (IList)nodesToRead);
            return results.ToArray();
        }

        /// <summary>call a server method</summary>
        /// <param name="tagParent"></param>
        /// <param name="tag"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object[] CallMethodByNodeId(string tagParent, string tag, params object[] args)
        {
            if (this.m_session == null)
                return (object[])null;
            return this.m_session.Call(new NodeId(tagParent), new NodeId(tag), args).ToArray<object>();
        }

        /// <summary>
        /// Raises the connect complete event on the main GUI thread.
        /// </summary>
        private void DoConnectComplete(object state)
        {
            EventHandler connectComplete = this.m_ConnectComplete;
            if (connectComplete == null)
                return;
            // ISSUE: variable of the null type
            object local = null;
            connectComplete((object)this, (EventArgs)local);
        }

        private void CheckReturnValue(StatusCode status)
        {
            if (!StatusCode.IsGood(status))
                throw new Exception(string.Format("Invalid response from the server. (Response Status: {0})", (object)status));
        }
    }
}