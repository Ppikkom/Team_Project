using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingUIController : MonoBehaviour
{
    public Button 메인메뉴버튼;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (메인메뉴버튼 != null)
        {
            메인메뉴버튼.onClick.AddListener(메인메뉴로);
        }
        else
        {
            Debug.LogWarning("메인메뉴버튼이 연결되지 않았습니다.");
        }
    }

    void 메인메뉴로()
    {
        GameManager.인스턴스.메인메뉴로();
    }
}
