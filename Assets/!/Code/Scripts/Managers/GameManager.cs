using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager 인스턴스 { get; private set; }
    public int 현재날짜 = 1;
    public bool 도전모드인가 = false;
    public float 생존시간 = 0f;
    [SerializeField] private ReputationManager 평판;
    [SerializeField] private DayManager 날짜;
    [SerializeField] private Image 페이드이미지; // FadeCanvas의 검은색 Image
    [SerializeField] private float 페이드속도 = 1f;
    private Canvas 페이드캔버스; // FadeCanvas 참조

    void Awake()
    {
        if (인스턴스 == null)
        {
            인스턴스 = this;
            DontDestroyOnLoad(gameObject);

            // FadeCanvas 설정
            페이드캔버스 = GameObject.Find("FadeCanvas")?.GetComponent<Canvas>();
            if (페이드캔버스 != null)
            {
                DontDestroyOnLoad(페이드캔버스.gameObject);
                페이드이미지 = 페이드캔버스.GetComponentInChildren<Image>();
                페이드이미지.color = new Color(0, 0, 0, 0); // 초기 투명
            }

            SceneManager.sceneLoaded += 씬로드완료시;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void 씬로드완료시(Scene scene, LoadSceneMode mode)
    {
        평판 = FindAnyObjectByType<ReputationManager>();
        날짜 = FindAnyObjectByType<DayManager>();

        // FadeCanvas가 없으면 새로 찾기
        if (페이드캔버스 == null)
        {
            페이드캔버스 = GameObject.Find("FadeCanvas")?.GetComponent<Canvas>();
            if (페이드캔버스 != null)
            {
                DontDestroyOnLoad(페이드캔버스.gameObject);
                페이드이미지 = 페이드캔버스.GetComponentInChildren<Image>();
                페이드이미지.color = new Color(0, 0, 0, 1); // 로드 후 검은색
            }
        }

        if (scene.name == "Play")
        {
            if (평판 != null)
            {
                평판.초기화();
            }
            else
            {
                Debug.LogWarning("Play 씬에서 평판 객체 찾기 실패");
            }

            if (날짜 != null)
            {
                날짜.시작(); // 날짜 관리자 초기화
            }
            else
            {
                Debug.LogWarning("Play 씬에서 날짜 객체 찾기 실패");
            }
        }

        // 페이드인 실행
        if (페이드이미지 != null)
        {
            StartCoroutine(페이드인());
        }
        else
        {
            Debug.LogWarning("페이드이미지 찾기 실패");
        }
    }

    IEnumerator 페이드아웃(string 씬이름)
    {
        if (페이드이미지 == null)
        {
            SceneManager.LoadScene(씬이름);
            yield break;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 페이드속도;
            페이드이미지.color = new Color(0, 0, 0, t);
            yield return null;
        }

        SceneManager.LoadScene(씬이름);
    }

    IEnumerator 페이드인()
    {
        if (페이드이미지 == null)
            yield break;

        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * 페이드속도;
            페이드이미지.color = new Color(0, 0, 0, t);
            yield return null;
        }
    }

    public void 게임시작()
    {
        현재날짜 = 1;
        StartCoroutine(페이드아웃("Play"));
    }

    public void 결과화면으로()
    {
        StartCoroutine(페이드아웃("Result"));
    }

    public void 다음날()
    {
        if (현재날짜 < 5)
        {
            현재날짜++;
            StartCoroutine(페이드아웃("Play"));
        }
        else
        {
            결과종합();
        }
    }

    public void 결과종합()
    {
        StartCoroutine(페이드아웃("Ending1"));
    }

    internal void 게임오버()
    {
        StartCoroutine(페이드아웃("Ending2"));
    }

    public void 메인메뉴로()
    {
        도전모드인가 = false;
        생존시간 = 0f;
        StartCoroutine(페이드아웃("MainMenu"));
    }

    void Update()
    {
        if (도전모드인가)
        {
            생존시간 += Time.deltaTime;
        }

        if (날짜 != null && 날짜.현재날짜 > 현재날짜)
        {
            다음날();
        }

        if (현재날짜 > 5)
        {
            결과종합();
        }

        // L키 입력으로 결과종합 호출
        if (Input.GetKeyDown(KeyCode.L))
        {
            결과종합();
        }
    }

    public ReputationManager 평판참조
    {
        get
        {
            if (평판 == null)
                평판 = FindAnyObjectByType<ReputationManager>();
            return 평판;
        }
    }

    public DayManager 날짜참조
    {
        get
        {
            if (날짜 == null)
                날짜 = FindAnyObjectByType<DayManager>();
            return 날짜;
        }
    }

    public void 도전모드시작()
    {
        도전모드인가 = true;
        현재날짜 = 1;
        생존시간 = 0f;
        StartCoroutine(페이드아웃("Play"));
    }

    public void 도전모드종료()
    {
        도전모드인가 = false;
        StartCoroutine(페이드아웃("ChallengeEnd"));
    }
}
