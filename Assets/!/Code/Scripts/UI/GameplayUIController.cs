using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameplayUIController : MonoBehaviour
{
    public static GameplayUIController 인스턴스 { get; private set; }

    [Header("킬로그 알림")]
    [SerializeField] private Transform 킬로그패널; // 킬로그 텍스트들이 추가될 부모 패널
    [SerializeField] private GameObject 킬로그텍스트프리팹; // 킬로그용 TextMeshProUGUI 프리팹
    private List<TextMeshProUGUI> 킬로그리스트 = new List<TextMeshProUGUI>();
    private Queue<TextMeshProUGUI> 텍스트풀 = new Queue<TextMeshProUGUI>(); // 오브젝트 풀// 클래스 상단에 추가
    private Dictionary<TextMeshProUGUI, Coroutine> 페이드코루틴맵 = new Dictionary<TextMeshProUGUI, Coroutine>();
    private const float 알림표시시간 = 5f;
    private const int 최대알림수 = 5; // 화면에 표시할 최대 알림 수
    private const int 초기풀크기 = 20; // 초기 풀 크기

    [Header("왼쪽 UI")]
    public TextMeshProUGUI 평판텍스트;
    public TextMeshProUGUI 일반문제_텍스트;
    public TextMeshProUGUI 진상문제_텍스트;
    [Header("카드 이미지")]
    public GameObject 엘로우카드이미지;
    public GameObject 레드카드이미지;

    [Header("오른쪽 알림")]
    public TextMeshProUGUI 진상알람_텍스트;

    [Header("중앙 UI")]
    public TextMeshProUGUI 날짜텍스트;
    public TextMeshProUGUI 시간텍스트;
    [Header("도전모드 타이머")]
    public TextMeshProUGUI 도전모드타이머텍스트;

    [Header("일반모드 UI")]
    public GameObject 일반모드패널; // 날짜/시간 UI를 담는 패널(부모 오브젝트)
    [Header("도전모드 UI")]
    public GameObject 도전모드패널; // 도전모드타이머텍스트를 담는 패널(부모 오브젝트)

    private Coroutine 알람코루틴;

    // 6월 20일 추가 : 문제 카운트 함수 부분 추가
    private int normal = 0;
    private int jinsang = 0;


    [Header("카드 효과음")]
    public AudioClip 옐로카드생성효과음;
    public AudioClip 레드카드생성효과음;
    private AudioSource 카드AudioSource;

    [Header("킬로그 효과음")]
    public AudioClip 킬로그알림효과음;
    private AudioSource 킬로그AudioSource;


    void Awake()
    {
        if (인스턴스 == null)
        {
            인스턴스 = this;
        }
        else
        {
            Destroy(gameObject);
        }

        카드AudioSource = gameObject.AddComponent<AudioSource>();
        카드AudioSource.playOnAwake = false;
        카드AudioSource.loop = false;
    }


    void Start()
    {
        // 필수 UI 연결 확인
        if (평판텍스트 == null || 시간텍스트 == null || 날짜텍스트 == null || 진상알람_텍스트 == null || 킬로그패널 == null || 킬로그텍스트프리팹 == null)
        {
            Debug.LogError("UI 텍스트가 하나 이상 연결되지 않음");
            return;
        }

        초기화텍스트풀();

        ReputationManager.인스턴스.E평판변경.AddListener(평판UI업데이트);
        DayManager.인스턴스.날짜변경이벤트.AddListener(날짜UI업데이트);
        DayManager.인스턴스.시간변경이벤트.AddListener(시간UI업데이트);

        // 일반 문제 업데이트 부분
        //Debug.Log($"[UI] 연결 전 이벤트 리스너 수: {ProblemManager.인스턴스.E일반문제업데이트.GetPersistentEventCount()}");

        //ProblemManager.인스턴스.E일반문제업데이트.AddListener(일반문제UI업데이트);

        //Debug.Log("[UI] 일반문제UI업데이트 연결 완료");

        //ProblemManager.인스턴스.E진상문제업데이트.AddListener(진상문제UI업데이트);

        날짜UI업데이트(DayManager.인스턴스.현재날짜);
        시간UI업데이트(900); // 오전 9시 초기값
        평판UI업데이트(ReputationManager.인스턴스.현재점수);

        일반문제UI업데이트(0);
        진상문제UI업데이트(0);

        if (일반모드패널 != null && 도전모드패널 != null)
        {
            일반모드패널.SetActive(!GameManager.인스턴스.도전모드인가);
            도전모드패널.SetActive(GameManager.인스턴스.도전모드인가);
        }
    }

    private void 초기화텍스트풀()
    {
        if (킬로그텍스트프리팹 == null || 킬로그패널 == null)
        {
            Debug.LogError("[킬로그] 킬로그텍스트프리팹 또는 킬로그패널이 Inspector에서 연결되지 않음!");
            return;
        }

        for (int i = 0; i < 초기풀크기; i++)
        {
            GameObject 텍스트오브젝트 = Instantiate(킬로그텍스트프리팹, 킬로그패널);
            TextMeshProUGUI 텍스트 = 텍스트오브젝트.GetComponentInChildren<TextMeshProUGUI>();

            if (텍스트 == null)
            {
                Debug.LogError($"[킬로그] 킬로그텍스트프리팹에 TextMeshProUGUI 컴포넌트가 없음! 프리팹을 확인하세요.");
                continue;
            }
            텍스트오브젝트.SetActive(false);
            텍스트풀.Enqueue(텍스트);
        }
    }

    public void 킬로그알림추가(string 메시지)
    {
        if (킬로그텍스트프리팹 == null || 킬로그패널 == null)
        {
            Debug.LogWarning("[킬로그] 킬로그텍스트프리팹 또는 킬로그패널이 연결되지 않음");
            return;
        }

        TextMeshProUGUI 텍스트;
        if (텍스트풀.Count > 0)
        {
            텍스트 = 텍스트풀.Dequeue();
            if (텍스트 == null || 텍스트.gameObject == null)
            {
                Debug.LogWarning("[킬로그] 풀에서 가져온 텍스트가 null임");
                GameObject 새텍스트 = Instantiate(킬로그텍스트프리팹, 킬로그패널);

                텍스트 = 새텍스트.GetComponentInChildren<TextMeshProUGUI>();
            }
            else
            {
                Transform 부모 = 텍스트.transform.parent;
                if (부모 != null)
                {
                    부모.gameObject.SetActive(true);
                    
                    UnityEngine.UI.Image 이미지 = 부모.GetComponentInChildren<UnityEngine.UI.Image>();
                    if (이미지 != null)
                    {
                        Color 이미지색상 = 이미지.color;
                        이미지색상.a = 1f;
                        이미지.color = 이미지색상;
                    }
                }
                else
                {
                    텍스트.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            GameObject 새텍스트 = Instantiate(킬로그텍스트프리팹, 킬로그패널);
            
            텍스트 = 새텍스트.GetComponent<TextMeshProUGUI>();
            if (텍스트 == null)
            {
                텍스트 = 새텍스트.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        텍스트.text = 메시지;
        텍스트.color = new Color(1f, 1f, 1f, 1f);
        킬로그리스트.Add(텍스트);

        if (킬로그리스트.Count > 최대알림수)
        {
            TextMeshProUGUI 오래된텍스트 = 킬로그리스트[0];

            if (페이드코루틴맵.TryGetValue(오래된텍스트, out Coroutine 기존코루틴))
            {
                StopCoroutine(기존코루틴);
                페이드코루틴맵.Remove(오래된텍스트);
            }

            오래된텍스트.gameObject.SetActive(false);
            
            Transform 오래된부모 = 오래된텍스트.transform.parent;
            if (오래된부모 != null)
            {
                오래된부모.gameObject.SetActive(false);
            }
            
            킬로그리스트.RemoveAt(0);
            텍스트풀.Enqueue(오래된텍스트);
        }
        
        for (int i = 0; i < 킬로그리스트.Count; i++)
        {
            TextMeshProUGUI 로그텍스트 = 킬로그리스트[i];
            Transform 로그부모 = 로그텍스트.transform.parent;
            
            RectTransform rt = null;
            if (로그부모 != null)
            {
                rt = 로그부모.GetComponent<RectTransform>();
            }
            else
            {
                rt = 로그텍스트.GetComponent<RectTransform>();
            }
            
            if (rt != null)
                rt.anchoredPosition = new Vector2(0, -30f * i);
        }
        if (킬로그AudioSource != null && 킬로그알림효과음 != null)
            킬로그AudioSource.PlayOneShot(킬로그알림효과음);

        if (페이드코루틴맵.TryGetValue(텍스트, out Coroutine 중복코루틴))
        {
            StopCoroutine(중복코루틴);
            페이드코루틴맵.Remove(텍스트);
        }

        Coroutine 새코루틴 = StartCoroutine(킬로그페이드아웃(텍스트));
        페이드코루틴맵[텍스트] = 새코루틴;


    }

    private IEnumerator 킬로그페이드아웃(TextMeshProUGUI 텍스트)
    {
        yield return new WaitForSeconds(알림표시시간);

        if (텍스트 == null || 텍스트.Equals(null) || 텍스트.gameObject == null || 텍스트.gameObject.Equals(null)) yield break;

        Transform 부모 = 텍스트.transform.parent;
        UnityEngine.UI.Image 이미지 = null;
        if (부모 != null)
        {
            이미지 = 부모.GetComponentInChildren<UnityEngine.UI.Image>();
        }

        float 알파 = 1f;
        Color 텍스트색상 = 텍스트.color;
        Color 이미지색상 = 이미지 != null ? 이미지.color : Color.white;

        while (알파 > 0f)
        {
            if (텍스트 == null || 텍스트.Equals(null) || 텍스트.gameObject == null || 텍스트.gameObject.Equals(null)) yield break;

            알파 -= Time.deltaTime;
            
            텍스트색상.a = 알파;
            텍스트.color = 텍스트색상;
            
            if (이미지 != null)
            {
                이미지색상.a = 알파;
                이미지.color = 이미지색상;
            }
            
            yield return null;
        }

        if (텍스트 == null || 텍스트.Equals(null) || 텍스트.gameObject == null || 텍스트.gameObject.Equals(null)) yield break;

        킬로그리스트.Remove(텍스트);
        
        if (부모 != null)
        {
            부모.gameObject.SetActive(false);
        }
        else
        {
            텍스트.gameObject.SetActive(false);
        }
        
        텍스트풀.Enqueue(텍스트);

        if (페이드코루틴맵.ContainsKey(텍스트))
            페이드코루틴맵.Remove(텍스트);

        for (int i = 0; i < 킬로그리스트.Count; i++)
        {
            TextMeshProUGUI 로그 = 킬로그리스트[i];
            if (로그 != null && 로그.gameObject != null)
            {
                Transform 로그부모 = 로그.transform.parent;
                RectTransform rt = null;
                
                if (로그부모 != null)
                {
                    rt = 로그부모.GetComponent<RectTransform>();
                }
                else
                {
                    rt = 로그.GetComponent<RectTransform>();
                }
                
                if (rt != null)
                    rt.anchoredPosition = new Vector2(0, -30f * i);
            }
        }
    }

    void OnDestroy()
    {
        if (ReputationManager.인스턴스 != null)
            ReputationManager.인스턴스.E평판변경.RemoveListener(평판UI업데이트);

        if (DayManager.인스턴스 != null)
        {
            DayManager.인스턴스.날짜변경이벤트.RemoveListener(날짜UI업데이트);
            DayManager.인스턴스.시간변경이벤트.RemoveListener(시간UI업데이트);
        }

        if (ProblemManager.인스턴스 != null)
        {
            //ProblemManager.인스턴스.E일반문제업데이트.RemoveListener(일반문제UI업데이트);
            //ProblemManager.인스턴스.E진상문제업데이트.RemoveListener(진상문제UI업데이트);
        } 
    }

    void Update()
    {
        if (GameManager.인스턴스.도전모드인가 && 도전모드타이머텍스트 != null)
        {
            float 생존시간 = GameManager.인스턴스.생존시간;

            int 시 = (int)(생존시간 / 3600);
            int 분 = (int)(생존시간 % 3600) / 60;
            int 초 = (int)(생존시간 % 60);
            도전모드타이머텍스트.text = $"Survival {시:D2}:{분:D2}:{초:D2}";
        }
    }

    #region 화면 왼쪽
    public void 평판UI업데이트(int 새로운점수)
    {
        int 표시점수 = Mathf.Max(0, 새로운점수);
        평판텍스트.text = 표시점수.ToString();
    }

    public void 일반문제UI업데이트(int 수)
    {
        //Debug.Log($"[UI 업데이트] 실제 받은 수: {수}");

        if (수 < 0)
        {
            Debug.LogWarning("[UI] 음수 문제 수 수신됨. 0으로 보정");
            수 = 0;
        }

        //Debug.Log($"[UI 업데이트] 실제 받은 수: {수}");

        if (일반문제_텍스트 != null)
        {
            일반문제_텍스트.text = 수.ToString();
            일반문제_텍스트.gameObject.SetActive(수 > 0);
        }

        if (엘로우카드이미지 != null)
        {
            엘로우카드이미지.SetActive(수 > 0);
        }

    }

    // 일반 문제 카운트 함수
    public void normalCount()
    {
        normal++;
        일반문제UI업데이트(normal);

        // 옐로카드 생성 효과음
        if (카드AudioSource != null && 옐로카드생성효과음 != null)
            카드AudioSource.PlayOneShot(옐로카드생성효과음);
    }

    public void normalDisCount()
    {
        normal = Mathf.Max(0, normal - 1);
        일반문제UI업데이트(normal);
    }


    public void 진상문제UI업데이트(int 수)
    {
        bool 활성화여부 = 수 > 0;

        if (진상문제_텍스트 != null)
        {
            진상문제_텍스트.text = 수.ToString();
            진상문제_텍스트.gameObject.SetActive(활성화여부);
        }

        if (레드카드이미지 != null)
            레드카드이미지.SetActive(수 > 0); // 진상문제 있을 때만 표시
    }

    // 진상 문제 카운트 함수
    public void jinsangCount()
    {
        jinsang++;
        진상문제UI업데이트(jinsang);

        // 레드카드 생성 효과음
        if (카드AudioSource != null && 레드카드생성효과음 != null)
            카드AudioSource.PlayOneShot(레드카드생성효과음);
    }

    public void jinsangDisCount()
    {
        jinsang = Mathf.Max(0, jinsang - 1);
        진상문제UI업데이트(jinsang);
    }

    #endregion

    #region 화면 중앙
    public void 시간UI업데이트(int 현재시간) // 예: 1030 → 10:30
    {
        int 시24 = 현재시간 / 100;
        int 분 = 현재시간 % 100;

        string 오전오후 = 시24 < 12 ? "AM" : "PM";
        int 시12 = 시24 % 12;
        if (시12 == 0) 시12 = 12;

        시간텍스트.text = $"{오전오후} {시12:D2}:{분:D2}";
    }

    void 날짜UI업데이트(int 날짜)
    {
        Debug.Log($"날짜 UI 업데이트: Day {날짜}");
        날짜텍스트.text = $"Day {날짜}";
    }
    #endregion

    #region 화면 오른쪽
    public void 진상위치알람(string 진상이름 = "", string 위치 = "")
    {
        if (진상알람_텍스트 == null) return;

        if (알람코루틴 != null)
            StopCoroutine(알람코루틴);

        알람코루틴 = StartCoroutine(알람표시코루틴(진상이름, 위치));
    }

    IEnumerator 알람표시코루틴(string 진상이름, string 위치)
    {
        진상알람_텍스트.color = new Color(1f, 1f, 1f, 1f);
        진상알람_텍스트.gameObject.SetActive(true);

        string 전체메시지 = $"<color=#FF4444>Alert</color>\n\n{진상이름} JinSang\n{위치}\nFound!";
        string 현재메시지 = "";

        for (int i = 0; i < 전체메시지.Length; i++)
        {
            현재메시지 += 전체메시지[i];
            진상알람_텍스트.text = 현재메시지;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(5f);

        float 알파 = 1f;
        Color 색상 = 진상알람_텍스트.color;
        while (알파 > 0)
        {
            알파 -= Time.deltaTime;
            색상.a = 알파;
            진상알람_텍스트.color = 색상;
            yield return null;
        }

        진상알람_텍스트.gameObject.SetActive(false);
        진상알람_텍스트.color = new Color(색상.r, 색상.g, 색상.b, 1f);
    }
    #endregion
}
