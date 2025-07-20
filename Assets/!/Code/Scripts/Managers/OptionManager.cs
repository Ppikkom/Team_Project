using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;

public class OptionManager : MonoBehaviour
{
    public static OptionManager 인스턴스 { get; private set; }

    [Header("오디오")]
    [SerializeField] private AudioMixer 오디오믹서; // 마스터, BGM, SFX 믹서
    public float 마스터볼륨 { get; private set; } = 1f;
    public float 배경음볼륨 { get; private set; } = 1f;
    public float 효과음볼륨 { get; private set; } = 1f;

    [Header("화면")]
    public Vector2Int 화면해상도 { get; private set; } = new Vector2Int(1920, 1080);
    public bool 전체화면 { get; private set; } = true;

    [Header("기타")]
    public bool 화면흔들림허용 { get; private set; } = true;

    public event Action 옵션변경이벤트;

    void Awake()
    {
        if (인스턴스 == null)
        {
            인스턴스 = this;
            DontDestroyOnLoad(gameObject);
            옵션불러오기(); // 옵션 불러오기
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable() => SceneManager.sceneLoaded += 씬로드시해상도복원;
    void OnDisable() => SceneManager.sceneLoaded -= 씬로드시해상도복원; //메모리 누수 방지

    void 씬로드시해상도복원(Scene 씬, LoadSceneMode 모드)
    {
        if (Application.isMobilePlatform)
        {
            Debug.Log("모바일 플랫폼: 해상도 변경 비활성화");
            return;
        }

        // 현재 해상도와 저장된 해상도가 같고, 전체화면 설정도 같으면 스킵
        if (Screen.width == 화면해상도.x && Screen.height == 화면해상도.y && Screen.fullScreen == 전체화면)
        {
            Debug.Log("해상도 및 전체화면 설정이 이미 일치함. 변경 스킵.");
            return;
        }

        // 지원되는 해상도인지 확인
        if (Screen.resolutions.Any(r => r.width == 화면해상도.x && r.height == 화면해상도.y))
        {
            Screen.SetResolution(화면해상도.x, 화면해상도.y, 전체화면);
            Debug.Log($"해상도 복원: {화면해상도.x}x{화면해상도.y}, 전체화면: {전체화면}");
        }
        else
        {
            Debug.LogWarning($"지원되지 않는 해상도: {화면해상도.x}x{화면해상도.y}, 기본 해상도로 복원");
            화면해상도 = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
            전체화면 = true;
            Screen.SetResolution(화면해상도.x, 화면해상도.y, 전체화면);
            PlayerPrefs.SetInt("ResolutionWidth", 화면해상도.x);
            PlayerPrefs.SetInt("ResolutionHeight", 화면해상도.y);
            PlayerPrefs.SetInt("Fullscreen", 전체화면 ? 1 : 0);
        }
    }

    // === 사운드 설정 ===
    public void 마스터볼륨설정(float 값)
    {
        if (오디오믹서 == null)
        {
            Debug.LogError("오디오믹서가 할당되지 않았습니다!");
            return;
        }
        마스터볼륨 = Mathf.Clamp01(값);
        오디오믹서.SetFloat("MasterVolume", Mathf.Log10(마스터볼륨 <= 0.01f ? 0.01f : 마스터볼륨) * 20);
        PlayerPrefs.SetFloat("MasterVolume", 마스터볼륨);
        옵션변경이벤트?.Invoke();
    }

    public void 배경음볼륨설정(float 값)
    {
        if (오디오믹서 == null)
        {
            Debug.LogError("오디오믹서가 할당되지 않았습니다!");
            return;
        }
        배경음볼륨 = Mathf.Clamp01(값);
        오디오믹서.SetFloat("BGMVolume", Mathf.Log10(배경음볼륨 <= 0.01f ? 0.01f : 배경음볼륨) * 20);
        PlayerPrefs.SetFloat("BGMVolume", 배경음볼륨);
        옵션변경이벤트?.Invoke();
    }

    public void 효과음볼륨설정(float 값)
    {
        if (오디오믹서 == null)
        {
            Debug.LogError("오디오믹서가 할당되지 않았습니다!");
            return;
        }
        효과음볼륨 = Mathf.Clamp01(값);
        오디오믹서.SetFloat("SFXVolume", Mathf.Log10(효과음볼륨 <= 0.01f ? 0.01f : 효과음볼륨) * 20);
        PlayerPrefs.SetFloat("SFXVolume", 효과음볼륨);
        옵션변경이벤트?.Invoke();
    }

    // 화면 해상도 및 전체화면 (코루틴 사용)
    public void 해상도설정(int 가로, int 세로, bool 전체)
    {
        if (Application.isMobilePlatform)
        {
            Debug.Log("모바일 플랫폼: 해상도 변경 비활성화");
            return;
        }
        if (!Screen.resolutions.Any(r => r.width == 가로 && r.height == 세로))
        {
            Debug.LogWarning($"지원되지 않는 해상도: {가로}x{세로}, 기본 해상도로 대체");
            가로 = Screen.currentResolution.width;
            세로 = Screen.currentResolution.height;
            전체 = true;
        }
        StartCoroutine(해상도코루틴(가로, 세로, 전체));
    }

    private IEnumerator 해상도코루틴(int 가로, int 세로, bool 전체)
    {
        // 1. 해상도 변경을 요청합니다.
        Screen.SetResolution(가로, 세로, 전체);
        yield return new WaitForEndOfFrame(); // 변경 적용을 위해 1프레임 대기

        Debug.Log($"해상도 설정 시도: {가로}x{세로}, 전체화면: {전체}");
        Debug.Log($"현재 런타임 해상도: {Screen.width}x{Screen.height}");

        // 2. [수정] 실제 적용된 크기(Screen.width)가 아닌, 사용자가 '의도한' 값(가로, 세로)을 저장합니다.
        화면해상도 = new Vector2Int(가로, 세로);
        전체화면 = 전체;

        // 3. 사용자의 선택을 PlayerPrefs에 저장합니다.
        PlayerPrefs.SetInt("ResolutionWidth", 화면해상도.x);
        PlayerPrefs.SetInt("ResolutionHeight", 화면해상도.y);
        PlayerPrefs.SetInt("Fullscreen", 전체 ? 1 : 0);
        PlayerPrefs.Save(); // 변경사항을 즉시 디스크에 저장하려면 호출하는 것이 안전합니다.

        // 4. UI 및 다른 시스템에 변경사항을 알립니다.
        옵션변경이벤트?.Invoke();
    }

    // 화면 흔들림
    public void 화면흔들림설정(bool 허용)
    {
        화면흔들림허용 = 허용;
        PlayerPrefs.SetInt("AllowScreenShake", 허용 ? 1 : 0);
        옵션변경이벤트?.Invoke();
    }

    // === 저장된 옵션 불러오기 ===
    public void 옵션불러오기()
    {
        마스터볼륨 = PlayerPrefs.GetFloat("MasterVolume", 1f);
        배경음볼륨 = PlayerPrefs.GetFloat("BGMVolume", 1f);
        효과음볼륨 = PlayerPrefs.GetFloat("SFXVolume", 1f);

        오디오믹서.SetFloat("MasterVolume", Mathf.Log10(마스터볼륨 <= 0.01f ? 0.01f : 마스터볼륨) * 20);
        오디오믹서.SetFloat("BGMVolume", Mathf.Log10(배경음볼륨 <= 0.01f ? 0.01f : 배경음볼륨) * 20);
        오디오믹서.SetFloat("SFXVolume", Mathf.Log10(효과음볼륨 <= 0.01f ? 0.01f : 효과음볼륨) * 20);

        int width = PlayerPrefs.GetInt("ResolutionWidth", Screen.currentResolution.width);
        int height = PlayerPrefs.GetInt("ResolutionHeight", Screen.currentResolution.height);
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        화면해상도 = new Vector2Int(width, height);
        전체화면 = fullscreen;
        화면흔들림허용 = PlayerPrefs.GetInt("AllowScreenShake", 1) == 1;

        Debug.Log($"옵션 불러오기: 해상도 {width}x{height}, 전체화면: {fullscreen}");
    }

    public void 옵션초기화()
    {
        PlayerPrefs.DeleteAll();
        마스터볼륨설정(1f);
        배경음볼륨설정(1f);
        효과음볼륨설정(1f);
        해상도설정(Screen.currentResolution.width, Screen.currentResolution.height, true);
        화면흔들림설정(true);

        Debug.Log("옵션 초기화 완료");
    }
}
