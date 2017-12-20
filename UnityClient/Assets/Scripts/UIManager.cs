using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button btnMQTT;
    public Image panelMQTT;

    private void Start()
    {
        panelMQTT.gameObject.SetActive(false);
    }

    public void OnClickBtnMQTT()
    {
        if (!panelMQTT.gameObject.activeInHierarchy)
        {
            panelMQTT.gameObject.SetActive(true);
        }
    }
}