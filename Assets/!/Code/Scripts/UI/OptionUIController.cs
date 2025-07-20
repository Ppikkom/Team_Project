using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionUIController : MonoBehaviour
{
    public static OptionUIController 인스턴스 { get; private set; }

    [Header("슬라이더")]
    public Slider 마스터볼륨슬라이더;
    public Slider 배경음슬라이더;
    public Slider 효과음슬라이더;

    [Header("화면")]
    public TMP_Dropdown 해상도드롭다운;
    public Toggle 전체화면토글;
    public Toggle 화면흔들림토글;

    [Header("옵션 패널")]
    public GameObject 옵션패널;
    public Button 닫기버튼; // 추가

    [SerializeField] private Button 초기화버튼;

    private static List<Resolution> 시스템해상도;

    [Header("옵션 효과음")]
    public AudioClip 옵션패널효과음;
    private AudioSource 옵션AudioSource;

    void Awake()
    {
        if (인스턴스 == null)
        {
            인스턴스 = this;
            Debug.Log("OptionUIController 초기화 완료");
        }
        else
        {
            Debug.LogWarning("중복된 OptionUIController 파괴");
            Destroy(gameObject);
        }

        // 옵션 효과음용 AudioSource 추가
        옵션AudioSource = gameObject.AddComponent<AudioSource>();
        옵션AudioSource.playOnAwake = false;
        옵션AudioSource.loop = false;
    }

    void OnEnable()
    {
        if (OptionManager.인스턴스 != null)
            OptionManager.인스턴스.옵션변경이벤트 += 옵션UI동기화;
    }

    void OnDisable()
    {
        if (OptionManager.인스턴스 != null)
            OptionManager.인스턴스.옵션변경이벤트 -= 옵션UI동기화;
    }

    void Start()
    {
        // 필수 UI 컴포넌트 null 체크
        if (마스터볼륨슬라이더 == null || 배경음슬라이더 == null || 효과음슬라이더 == null ||
            해상도드롭다운 == null || 전체화면토글 == null || 화면흔들림토글 == null || 닫기버튼 == null)
        {
            Debug.LogError("필수 UI 컴포넌트가 연결되지 않았습니다. 옵션 UI 비활성화.");
            gameObject.SetActive(false); // 옵션 UI 비활성화
            return;
        }

        // 슬라이더 초기화
        마스터볼륨슬라이더.value = OptionManager.인스턴스.마스터볼륨;
        마스터볼륨슬라이더.onValueChanged.AddListener(OptionManager.인스턴스.마스터볼륨설정);

        배경음슬라이더.value = OptionManager.인스턴스.배경음볼륨;
        배경음슬라이더.onValueChanged.AddListener(OptionManager.인스턴스.배경음볼륨설정);

        효과음슬라이더.value = OptionManager.인스턴스.효과음볼륨;
        효과음슬라이더.onValueChanged.AddListener(OptionManager.인스턴스.효과음볼륨설정);

        // 해상도 드롭다운: 시스템 지원 해상도 자동 감지
        해상도드롭다운.ClearOptions();
        시스템해상도 = Screen.resolutions
            .GroupBy(r => new { r.width, r.height })
            .Select(g => g.First())
            .OrderByDescending(r => r.width)
            .ToList();

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach (Resolution res in 시스템해상도)
        {
            int hz = Mathf.RoundToInt((float)res.refreshRateRatio.value);
            options.Add(new TMP_Dropdown.OptionData($"{res.width}x{res.height} ({hz}Hz)"));
        }
        해상도드롭다운.options = options;

        // OptionManager의 저장된 해상도로 드롭다운 초기화
        int index = 시스템해상도.FindIndex(res =>
            res.width == OptionManager.인스턴스.화면해상도.x &&
            res.height == OptionManager.인스턴스.화면해상도.y);
        if (index != -1)
            해상도드롭다운.value = index;
        else
            해상도드롭다운.value = 0; // 지원되지 않는 해상도면 첫 번째 항목 선택

        해상도드롭다운.onValueChanged.AddListener(해상도변경);

        // 전체화면 토글 설정
        전체화면토글.isOn = OptionManager.인스턴스.전체화면;
        전체화면토글.onValueChanged.AddListener(전체화면변경);

        // 화면 흔들림 토글 설정
        화면흔들림토글.isOn = OptionManager.인스턴스.화면흔들림허용;
        화면흔들림토글.onValueChanged.AddListener(OptionManager.인스턴스.화면흔들림설정);

        if (초기화버튼 != null)
        {
            초기화버튼.onClick.AddListener(() =>
            {
                OptionManager.인스턴스.옵션초기화();
                //마스터볼륨슬라이더.value = 1f;
                //배경음슬라이더.value = 1f;
                //효과음슬라이더.value = 1f;
                //전체화면토글.isOn = true;
                //화면흔들림토글.isOn = true;
                int index = 시스템해상도.FindIndex(res =>
                    res.width == OptionManager.인스턴스.화면해상도.x &&
                    res.height == OptionManager.인스턴스.화면해상도.y);
                if (index != -1)
                    해상도드롭다운.value = index;
            });
        }
        else
        {
            Debug.LogWarning("초기화버튼이 연결되지 않았습니다.");
        }

        // 닫기 버튼 이벤트 연결
        닫기버튼.onClick.AddListener(옵션창닫기);

        Debug.Log("OptionUIController 초기화 완료");
    }

    void Update()
    {
        // ESC 키로 OptionPanel 닫기
        if (Input.GetKeyDown(KeyCode.Escape) && 옵션패널.activeSelf)
        {
            옵션창닫기();
            Debug.Log("ESC 키로 OptionPanel 닫음");
        }
    }

    // 옵션 값을 UI로 동기화하는 함수
    public void 옵션UI동기화()
    {
        // 슬라이더 값 동기화
        마스터볼륨슬라이더.value = OptionManager.인스턴스.마스터볼륨;
        배경음슬라이더.value = OptionManager.인스턴스.배경음볼륨;
        효과음슬라이더.value = OptionManager.인스턴스.효과음볼륨;

        // 해상도 드롭다운 동기화
        int index = 시스템해상도.FindIndex(res =>
            res.width == OptionManager.인스턴스.화면해상도.x &&
            res.height == OptionManager.인스턴스.화면해상도.y);
        if (index != -1)
            해상도드롭다운.value = index;

        // 전체화면/화면흔들림 토글 동기화
        전체화면토글.isOn = OptionManager.인스턴스.전체화면;
        화면흔들림토글.isOn = OptionManager.인스턴스.화면흔들림허용;
    }

    public void 옵션창열기()
    {
        if (옵션패널 != null)
        {
            옵션패널.SetActive(true);

            // 효과음 재생
            if (옵션AudioSource != null && 옵션패널효과음 != null)
                옵션AudioSource.PlayOneShot(옵션패널효과음);
        }
        else
        {
            Debug.LogError("옵션패널이 연결되지 않았습니다.");
        }
        //Time.timeScale = 0f;
    }


    public void 옵션창닫기()
    {
        if (옵션패널 != null)
        {
            if (옵션AudioSource != null && 옵션패널효과음 != null)
                옵션AudioSource.PlayOneShot(옵션패널효과음);
            옵션패널.SetActive(false);
            Debug.Log("OptionPanel 닫힘");
        }
        else
        {
            Debug.LogError("옵션패널이 연결되지 않았습니다.");
        }

    }

    void 해상도변경(int index)
    {
        if (index < 0 || index >= 시스템해상도.Count) return;
        Resolution res = 시스템해상도[index];
        OptionManager.인스턴스.해상도설정(res.width, res.height, 전체화면토글.isOn);
    }

    void 전체화면변경(bool isOn)
    {
        OptionManager.인스턴스.해상도설정(OptionManager.인스턴스.화면해상도.x, OptionManager.인스턴스.화면해상도.y, isOn);
    }
}
