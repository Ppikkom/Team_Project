using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController 인스턴스 { get; private set; }

    [SerializeField] private GameObject 일시정지패널;
    [SerializeField] private Button 재개버튼; 
    [SerializeField] private Button 옵션버튼;
    [SerializeField] private Button 메인메뉴버튼;
    [SerializeField] private Button 종료버튼;
    [SerializeField] private OptionUIController 옵션UI;

    public bool 일시정지상태 { get; private set; } = false;

    void Awake()
    {
        if (인스턴스 == null)
        {
            인스턴스 = this;
            Debug.Log("PauseMenuController 초기화 완료");
        }
        else
        {
            Debug.LogWarning("중복된 PauseMenuController 파괴");
            Destroy(gameObject);
            return;
        }

        // 오브젝트가 활성화 상태인지 확인
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("PauseMenuController 오브젝트가 비활성화 상태입니다. 활성화합니다.");
            gameObject.SetActive(true);
        }
    }

    void Start()
    {
        if (일시정지패널 != null)
            일시정지패널.SetActive(false);
        else
            Debug.LogError("일시정지패널이 연결되지 않았습니다.");

        if (재개버튼 != null)
            재개버튼.onClick.AddListener(게임재개);
        else
            Debug.LogWarning("재개버튼이 연결되지 않았습니다.");

        if (옵션버튼 != null)
            옵션버튼.onClick.AddListener(옵션열기);
        else
            Debug.LogWarning("옵션버튼이 연결되지 않았습니다.");

        if (메인메뉴버튼 != null)
            메인메뉴버튼.onClick.AddListener(메인메뉴로);
        else
            Debug.LogWarning("메인메뉴버튼이 연결되지 않았습니다.");

        if (종료버튼 != null)
            종료버튼.onClick.AddListener(게임종료);
        else
            Debug.LogWarning("종료버튼이 연결되지 않았습니다.");

        // [추가] 초기 커서 상태 설정
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("초기 커서 상태: 숨김");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (OptionUIController.인스턴스 != null && OptionUIController.인스턴스.옵션패널.activeSelf)
            {
                OptionUIController.인스턴스.옵션창닫기();
                Debug.Log("ESC로 옵션 닫음");
                return;
            }

            if (!일시정지상태)
                게임일시정지();
            else
                게임재개();
        }

        // 커서 상태 점검
        if (!일시정지상태 && (OptionUIController.인스턴스 == null || OptionUIController.인스턴스.옵션패널 == null || !OptionUIController.인스턴스.옵션패널.activeSelf))
        {
            if (Cursor.visible || Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                Debug.Log("Update: 커서 강제 숨김");
            }
        }
    }

    public void 게임일시정지()
    {
        일시정지상태 = true;
        Time.timeScale = 0f;
        if (일시정지패널 != null)
        {
            일시정지패널.SetActive(true);
            Debug.Log("일시정지 패널 활성화");
        }
        else
        {
            Debug.LogError("일시정지패널이 null입니다.");
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Debug.Log("일시정지: 커서 보임");
    }

    public void 게임재개()
    {
        StartCoroutine(게임재개_코루틴());
    }

    private IEnumerator 게임재개_코루틴()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        일시정지상태 = false;
        Time.timeScale = 1f;
        일시정지패널.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("게임 재개: 커서 즉시 숨김");

        if (OptionUIController.인스턴스 != null && OptionUIController.인스턴스.옵션패널.activeSelf)
        {
            OptionUIController.인스턴스.옵션창닫기();
        }
    }

    void 옵션열기()
    {
        if (옵션UI != null)
        {
            옵션UI.옵션창열기();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Debug.Log("옵션 패널 열기: 커서 보임");
        }
        else
            Debug.LogWarning("옵션 UI가 연결되지 않았습니다.");
    }

    void 메인메뉴로()
    {
        Time.timeScale = 1f;
        GameManager.인스턴스.메인메뉴로();
    }

    void 게임종료()
    {
        Application.Quit();
    }

    void OnDestroy()
    {
        if (인스턴스 == this)
        {
            인스턴스 = null;
            Debug.Log("PauseMenuController 파괴");
        }
    }
}
