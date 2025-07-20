using System;
using UnityEngine;
using UnityEngine.UI;

public class ResultUIController : MonoBehaviour
{
    public Button 다음_버튼;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;


        다음_버튼.onClick.AddListener(다음날버튼_눌림);
    }

    private void 다음날버튼_눌림()
    {
        GameManager.인스턴스.다음날();
        Debug.Log("다음날 버튼 눌림");
    }

}
