using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using UnityEngine;
using UnityEngine.UI;

public class UIMQTT : MonoBehaviour
{
    private MqttClient _client;

    #region UI控件

    public Button _btnConnect;
    public Text _ipTxt;

    #endregion UI控件

    public void OnClickBtnConnect()
    {
        try
        {
            _client = new MqttClient(IPAddress.Parse(_ipTxt.text));
        }
        catch
        {
        }
    }
}