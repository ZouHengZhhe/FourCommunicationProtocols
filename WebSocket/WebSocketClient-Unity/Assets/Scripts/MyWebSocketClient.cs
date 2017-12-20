using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MyWebSocketClient : MonoBehaviour
{
    private WebSocket _ws = null;

    public Button SendBtn;
    public InputField SendInput;
    public Text SendTxt;
    public Text NoticeTxt;
    public Text ReceiveTxt;

    private bool _isReceive = false;    //表示接收到信息
    private string _receiveStr = null;  //记录接收到的信息

    private IEnumerator Start()
    {
        _ws = new WebSocket(new Uri("ws://127.0.0.1:8730/Test"));
        yield return StartCoroutine(_ws.Connect());
        //AddText(NoticeTxt, "Successfully connect to the server!");
        while (true)
        {
            string reply = _ws.RecvString();
            if (reply != null)
            {
                OnReceive(reply);
            }
            if (_ws.error != null)
            {
                Debug.LogError("Error: " + _ws.error);
                break;
            }
            yield return 0;
        }
    }

    /// <summary>
    /// 发送按钮点击事件
    /// </summary>
    public void OnClickBtnSend()
    {
        _ws.SendString(SendInput.text);
        AddText(SendTxt, "Send msg to server: " + SendInput.text);
        SendInput.text = null;
    }

    /// <summary>
    /// 关闭连接按钮点击事件
    /// </summary>
    public void OnClickBtnClose()
    {
        _ws.Close();
    }

    /// <summary>
    /// 接收到信息的回调函数
    /// </summary>
    /// <param name="o"></param>
    /// <param name="e"></param>
    public void OnReceive(string str)
    {
        _receiveStr = "Receive msg from server: " + str;
        _isReceive = true;
    }

    #region Unity方法

    private void Update()
    {
        if (_isReceive)
        {
            AddText(ReceiveTxt, _receiveStr);
            _receiveStr = null;
            _isReceive = false;
        }
    }

    private void OnDestroy()
    {
        _ws.Close();
    }

    #endregion Unity方法

    #region 帮助方法

    /// <summary>
    /// 添加文字（Notice,Receive,Send）
    /// </summary>
    /// <param name="t"></param>
    /// <param name="str"></param>
    private void AddText(Text t, string str)
    {
        t.text += str + "\n";
    }

    #endregion 帮助方法
}