using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button 시작_버튼;
    public Button 옵션_버튼;
    public Button 종료_버튼;
    public Button 도전모드_버튼;

    [SerializeField] private GameObject 옵션패널;
    [SerializeField] private OptionUIController 옵션UI;

    void Start()
    {
        시작_버튼.onClick.AddListener(시작버튼_눌림);
        도전모드_버튼.onClick.AddListener(도전모드버튼_눌림);
        옵션_버튼.onClick.AddListener(옵션버튼_눌림);
        종료_버튼.onClick.AddListener(종료버튼_눌림);

        if (옵션패널 != null)
            옵션패널.SetActive(false);
        else
            Debug.LogWarning("옵션패널이 연결되지 않았습니다.");
    }

    private void 시작버튼_눌림()
    {
        GameManager.인스턴스.게임시작();
    }

    private void 도전모드버튼_눌림()
    {
        GameManager.인스턴스.도전모드시작();
    }

    private void 옵션버튼_눌림()
    {
        if (옵션UI != null)
        {
            옵션UI.옵션창열기();
        }
        else
        {
            Debug.LogWarning("옵션 UI가 연결되지 않았습니다.");
            if (옵션패널 != null)
                옵션패널.SetActive(true);
        }
    }

    private void 종료버튼_눌림()
    {
        Debug.Log("종료 버튼 눌림");
        Application.Quit();
    }
}
