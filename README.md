# 四种通讯协议
包含MQTT，OPCUA，Modbus，WebSocket

#Unity客户端
其中UnityClient为Unity客户端；该客户端可选择使用四种通讯协议；（其中OPCUA因.NetFramework版本过高无法导入Unity，在VS中写opcua客户端，再利用MQTT将数据传入Unity）
UI较多，暂未完成；

#MQTT
要在Unity中建立Client，这里需要导入M2Mqtt.dll，利用该动态链接库进行相应的开发
